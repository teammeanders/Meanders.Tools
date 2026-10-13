using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Meanders.Tools.Core;
using Meanders.Tools.Grasshopper.Goo;
using Meanders.Tools.Grasshopper.Parameters;
using Meanders.Tools.Plugin;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Meanders.Tools.Grasshopper.Components
{
    public class ME_Bake_Component : GH_Component
    {
        private bool _groupBranchItems = true;
        private string _lastMessage = "Ready";
        private bool _previousBakeState = false;

        public ME_Bake_Component()
            : base(
                "ME Bake",
                "ME Bake",
                "Bake geometry into Rhino with Meanders attributes.",
                "Meanders",
                "Objects")
        {
            Attributes = new ME_Bake_ComponentAttributes(this);
        }

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter(
                "Geometry",
                "G",
                "Geometry or ME Objects to bake.",
                GH_ParamAccess.tree);

            pManager.AddParameter(
                new ME_Attribute_Param(),
                "Attribute",
                "A",
                "Meanders attributes to apply while baking.",
                GH_ParamAccess.item);

            pManager.AddBooleanParameter(
                "Bake",
                "B",
                "Bake geometry automatically when true.",
                GH_ParamAccess.item,
                true);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(
                "Message",
                "M",
                "Bake result message.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> geometryTree = null;
            ME_Attribute_Goo attributeGoo = null;
            bool bake = true;

            if (!DA.GetDataTree(0, out geometryTree))
                return;

            DA.GetData(1, ref attributeGoo);
            DA.GetData(2, ref bake);

            ME_Attribute attribute =
                attributeGoo != null && attributeGoo.Value != null
                    ? attributeGoo.Value.Duplicate()
                    : new ME_Attribute();

            if (!bake)
            {
                _previousBakeState = false;
                _lastMessage = "Bake input is false.";
                DA.SetData(0, _lastMessage);
                return;
            }

            // Bake only when Bake changes from false to true.
            // Other Grasshopper recomputes do not bake again while it remains true.
            if (!_previousBakeState)
            {
                _lastMessage =
                    BakeTree(
                        geometryTree,
                        attribute);
            }
            else
            {
                _lastMessage = "Ready to bake.";
            }

            _previousBakeState = true;

            DA.SetData(0, _lastMessage);
        }

        private void BakeTree(
            GH_Structure<IGH_Goo> geometryTree,
            ME_Attribute defaultAttribute)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            if (doc == null)
            {
                _lastMessage = "No active Rhino document.";
                return;
            }

            int bakedCount = 0;
            int branchCount = 0;

            foreach (GH_Path path in geometryTree.Paths)
            {
                IList branch = geometryTree.get_Branch(path);

                if (branch == null || branch.Count == 0)
                    continue;

                List<Guid> bakedIds = new List<Guid>();

                foreach (object item in branch)
                {
                    if (!(item is IGH_Goo goo))
                        continue;

                    ME_Object meObject = ExtractMEObject(goo);
                    if (meObject == null || meObject.Geometry == null)
                        continue;

                    ObjectAttributes attributes =
                        meObject.Attributes != null
                            ? meObject.Attributes.Duplicate()
                            : defaultAttribute.Attributes.Duplicate();

                    Guid id;

                    if (BakeGeometry(
                        doc,
                        meObject.Geometry,
                        attributes,
                        out id))
                    {
                        bakedIds.Add(id);
                        bakedCount++;
                    }
                }

                if (_groupBranchItems && bakedIds.Count > 0)
                {
                    int groupIndex = doc.Groups.Add(
                        "ME Bake " + path.ToString());

                    if (groupIndex >= 0)
                    {
                        foreach (Guid id in bakedIds)
                            doc.Groups.AddToGroup(groupIndex, id);

                        branchCount++;
                    }
                }
            }

            doc.Views.Redraw();

            _lastMessage =
                bakedCount == 0
                    ? "Nothing was baked."
                    : string.Format(
                        "Baked {0} object{1}{2}.",
                        bakedCount,
                        bakedCount == 1 ? "" : "s",
                        _groupBranchItems
                            ? string.Format(
                                " in {0} branch group{1}",
                                branchCount,
                                branchCount == 1 ? "" : "s")
                            : "");
        }

        private ME_Object ExtractMEObject(IGH_Goo goo)
        {
            if (goo == null)
                return null;

            if (goo is ME_Object_Goo meObjectGoo)
                return meObjectGoo.Value;

            if (goo is GH_ObjectWrapper wrapper)
            {
                if (wrapper.Value is ME_Object meObject)
                    return meObject;

                if (wrapper.Value is Rhino.Geometry.GeometryBase geometry)
                    return new ME_Object(geometry);

                if (wrapper.Value is Rhino.Geometry.Point3d point)
                    return new ME_Object(point);
            }

            if (goo is GH_Point point)
                return new ME_Object(point.Value);

            if (goo is GH_Curve curve)
                return new ME_Object(curve.Value);

            if (goo is GH_Brep brep)
                return new ME_Object(brep.Value);

            if (goo is GH_Surface surface)
                return new ME_Object(surface.Value);

            if (goo is GH_Mesh mesh)
                return new ME_Object(mesh.Value);

            if (goo is GH_SubD subD)
                return new ME_Object(subD.Value);

            if (goo is GH_Rectangle rectangle)
                return new ME_Object(rectangle.Value);

            if (goo is GH_Line line)
                return new ME_Object(line.Value);

            return null;
        }

        private bool BakeGeometry(
            RhinoDoc doc,
            object geometry,
            ObjectAttributes attributes,
            out Guid objectId)
        {
            objectId = Guid.Empty;

            if (geometry is Rhino.Geometry.SubD subD)
            {
                objectId = doc.Objects.AddSubD(subD, attributes);
                return objectId != Guid.Empty;
            }

            if (geometry is Rhino.Geometry.GeometryBase rhinoGeometry)
            {
                objectId = doc.Objects.Add(rhinoGeometry, attributes);
                return objectId != Guid.Empty;
            }

            if (geometry is Rhino.Geometry.Point3d point)
            {
                objectId = doc.Objects.AddPoint(point, attributes);
                return objectId != Guid.Empty;
            }

            if (geometry is Rhino.Geometry.Rectangle3d rectangle)
            {
                objectId = doc.Objects.AddCurve(
                    rectangle.ToNurbsCurve(),
                    attributes);
                return objectId != Guid.Empty;
            }

            return false;
        }

        protected override void AppendAdditionalComponentMenuItems(
            ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            ToolStripMenuItem groupItem =
                Menu_AppendItem(
                    menu,
                    "Group branch items",
                    Menu_GroupBranchItems,
                    true,
                    _groupBranchItems);

            groupItem.ToolTipText =
                "Bake all items from each Grasshopper branch into one Rhino group.";
        }

        private void Menu_GroupBranchItems(
            object sender,
            EventArgs e)
        {
            _groupBranchItems = !_groupBranchItems;
            ExpireSolution(true);
        }

        public bool GroupBranchItems
        {
            get { return _groupBranchItems; }
        }

        public string LastMessage
        {
            get { return _lastMessage; }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        protected override Bitmap Icon =>
            MeandersIconLoader.Load("me-bake.png");

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "9D1C4E72-5A83-4F16-B8D9-27C6E0A5413F");
            }
        }
    }

}
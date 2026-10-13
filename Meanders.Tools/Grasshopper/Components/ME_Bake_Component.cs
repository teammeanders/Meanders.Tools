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
        private bool _previousBakeState = false;
        private string _lastMessage = "Ready";

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
                "Bake geometry when changed from false to true.",
                GH_ParamAccess.item,
                false);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(
                "Message",
                "M",
                "Message describing the bake result.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> geometryTree = null;
            ME_Attribute_Goo attributeGoo = null;
            bool bake = false;

            if (!DA.GetDataTree(
                0,
                out geometryTree))
            {
                return;
            }

            DA.GetData(
                1,
                ref attributeGoo);

            DA.GetData(
                2,
                ref bake);

            ME_Attribute attribute =
                attributeGoo != null &&
                attributeGoo.Value != null
                    ? attributeGoo.Value.Duplicate()
                    : new ME_Attribute();

            /*
             * Bake is edge-triggered.
             *
             * False -> True:
             *     Bake once.
             *
             * True -> True:
             *     Do not bake again when Grasshopper recomputes.
             *
             * True -> False:
             *     Reset the trigger.
             *
             * This prevents connecting wires or changing other inputs
             * from accidentally creating additional Rhino objects.
             */
            if (!bake)
            {
                _previousBakeState = false;

                _lastMessage =
                    "Ready to bake.";

                DA.SetData(
                    0,
                    _lastMessage);

            GH_Structure<IGH_Goo> geometryTree = null;
            ME_Attribute_Goo attributeGoo = null;

            if (!Params.Input[0].VolatileData.AllData(true).GetEnumerator().MoveNext())
            {
                _lastMessage = "No geometry to bake.";
                ExpireSolution(true);
                return;
            }

            if (_previousBakeState)
            {
                _lastMessage =
                    "Ready to bake.";

                DA.SetData(
                    0,
                    _lastMessage);

                return;
                }
            }

            _previousBakeState = true;

            _lastMessage =
                BakeTree(
                    geometryTree,
                    attribute);

            DA.SetData(
                0,
                _lastMessage);
        }

        private string BakeTree(
            GH_Structure<IGH_Goo> geometryTree,
            ME_Attribute defaultAttribute)
        {
            RhinoDoc doc =
                RhinoDoc.ActiveDoc;

            if (doc == null)
            {
                return
                    "No active Rhino document.";
            }

            int bakedCount = 0;
            int groupCount = 0;

            foreach (
                GH_Path path in
                geometryTree.Paths)
            {
                IList branch =
                    geometryTree.get_Branch(path);

                if (branch == null ||
                    branch.Count == 0)
                {
                    continue;
                }

                List<Guid> bakedIds =
                    new List<Guid>();

                foreach (
                    object item in branch)
                {
                    IGH_Goo goo =
                        item as IGH_Goo;

                    if (goo == null)
                        continue;

                    ME_Object meObject =
                        ExtractMEObject(goo);

                    if (meObject == null ||
                        meObject.Geometry == null)
                    {
                        continue;
                    }

                    ObjectAttributes attributes =
                        meObject.Attributes != null
                            ? meObject.Attributes.Duplicate()
                            : defaultAttribute.Attributes.Duplicate();

                    Guid objectId;

                    if (BakeGeometry(
                        doc,
                        meObject.Geometry,
                        attributes,
                        out objectId))
                    {
                        bakedIds.Add(
                            objectId);

                        bakedCount++;
                    }
                }

                if (_groupBranchItems &&
                    bakedIds.Count > 0)
                {
                    int groupIndex =
                        doc.Groups.Add(
                            "ME Bake " +
                            path.ToString());

                    if (groupIndex >= 0)
                    {
                        foreach (
                            Guid id in bakedIds)
                        {
                            doc.Groups.AddToGroup(
                                groupIndex,
                                id);
                        }

                        groupCount++;
                    }
                }
            }

            doc.Views.Redraw();

            if (bakedCount == 0)
            {
                return
                    "Nothing was baked.";
            }

            if (!_groupBranchItems)
            {
                return string.Format(
                    "Baked {0} object{1}.",
                        bakedCount,
                    bakedCount == 1
                        ? ""
                        : "s");
            }

            return string.Format(
                "Baked {0} object{1} in {2} branch group{3}.",
                bakedCount,
                bakedCount == 1
                    ? ""
                    : "s",
                groupCount,
                groupCount == 1
                    ? ""
                    : "s");
        }

        private ME_Object ExtractMEObject(
            IGH_Goo goo)
        {
            if (goo == null)
                return null;

            if (goo is ME_Object_Goo meObjectGoo)
            {
                return meObjectGoo.Value;
            }

            if (goo is GH_ObjectWrapper wrapper)
            {
                if (wrapper.Value is ME_Object meObject)
                {
                    return meObject;
                }

                if (wrapper.Value is Rhino.Geometry.GeometryBase geometry)
                {
                    return new ME_Object(
                        geometry);
                }

                if (wrapper.Value is Rhino.Geometry.Point3d point)
                {
                    return new ME_Object(
                        point);
                }
            }

            if (goo is GH_Point ghPoint)
            {
                return new ME_Object(
                    ghPoint.Value);
            }
            if (goo is GH_Curve curve)
            {
                return new ME_Object(
                    curve.Value);
            }

            if (goo is GH_Brep brep)
            {
                return new ME_Object(
                    brep.Value);
            }

            if (goo is GH_Surface surface)
            {
                return new ME_Object(
                    surface.Value);
            }

            if (goo is GH_Mesh mesh)
            {
                return new ME_Object(
                    mesh.Value);
            }

            if (goo is GH_SubD subD)
            {
                return new ME_Object(
                    subD.Value);
            }

            if (goo is GH_Rectangle rectangle)
            {
                return new ME_Object(
                    rectangle.Value);
            }

            if (goo is GH_Line line)
            {
                return new ME_Object(
                    line.Value);
            }

            return null;
        }

        private bool BakeGeometry(
            RhinoDoc doc,
            object geometry,
            ObjectAttributes attributes,
            out Guid objectId)
        {
            objectId =
                Guid.Empty;

            if (geometry is Rhino.Geometry.SubD subD)
            {
                objectId =
                    doc.Objects.AddSubD(
                        subD,
                        attributes);

                return
                    objectId != Guid.Empty;
            }

            if (geometry is Rhino.Geometry.GeometryBase rhinoGeometry)
            {
                objectId =
                    doc.Objects.Add(
                        rhinoGeometry,
                        attributes);

                return
                    objectId != Guid.Empty;
            }

            if (geometry is Rhino.Geometry.Point3d point)
            {
                objectId =
                    doc.Objects.AddPoint(
                        point,
                        attributes);

                return
                    objectId != Guid.Empty;
            }

            if (geometry is Rhino.Geometry.Rectangle3d rectangle)
            {
                objectId =
                    doc.Objects.AddCurve(
                    rectangle.ToNurbsCurve(),
                    attributes);

                return
                    objectId != Guid.Empty;
            }

            return false;
        }

        protected override void
            AppendAdditionalComponentMenuItems(
            ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(
                menu);

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
            _groupBranchItems =
                !_groupBranchItems;

            ExpireSolution(true);
        }

        public bool GroupBranchItems
        {
            get
            {
                return _groupBranchItems;
            }
        }

        public string LastMessage
        {
            get
            {
                return _lastMessage;
            }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.primary;
            }
        }

        protected override Bitmap Icon =>
            MeandersIconLoader.Load(
                "me-bake.png");

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "9D1C4E72-5A83-4F16-B8D9-27C6E0A5413F");
            }
        }
    }

    internal class ME_Bake_ComponentAttributes : GH_ComponentAttributes
    {
        private Rectangle _bakeButtonBounds;

        public ME_Bake_ComponentAttributes(
            ME_Bake_Component owner)
            : base(owner)
        {
        }

        protected override void Layout()
        {
            base.Layout();

            Rectangle bounds = Bounds;

            _bakeButtonBounds =
                new Rectangle(
                    bounds.X + 4,
                    bounds.Bottom - 24,
                    bounds.Width - 8,
                    18);

            Bounds =
                new Rectangle(
                    bounds.X,
                    bounds.Y,
                    bounds.Width,
                    bounds.Height + 22);
        }

        protected override void Render(
            GH_Canvas canvas,
            System.Drawing.Graphics graphics,
            GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel != GH_CanvasChannel.Objects)
                return;

            var owner =
                Owner as ME_Bake_Component;

            if (owner == null)
                return;

            GH_Capsule button =
                GH_Capsule.CreateTextCapsule(
                    _bakeButtonBounds,
                    "Bake");

            button.Render(
                graphics,
                Selected,
                owner.Locked,
                false);

            button.Dispose();
        }

        public override GH_ObjectResponse RespondToMouseDown(
            GH_Canvas sender,
            GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left &&
                _bakeButtonBounds.Contains(e.CanvasLocation))
            {
                var owner =
                    Owner as ME_Bake_Component;

                if (owner != null && !owner.Locked)
                {
                    owner.BakeFromButton();
                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
    }
}

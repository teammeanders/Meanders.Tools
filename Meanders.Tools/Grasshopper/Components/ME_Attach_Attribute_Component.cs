using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Meanders.Tools.Core;
using Meanders.Tools.Grasshopper.Goo;
using System;
using System.Collections;
using System.Drawing;
using Meanders.Tools.Grasshopper.Parameters;
using Meanders.Tools.Plugin;

namespace Meanders.Tools.Grasshopper.Components

{
    public class ME_Attach_Attribute_Component : GH_Component
    {
        public ME_Attach_Attribute_Component()
            : base(
                "ME Attach Attribute",
                "ME Attach",
                "Attach Meanders attributes to geometry or Grasshopper data.",
                "Meanders",
                "Objects")
        {
        }

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter(
                "Geometry",
                "G",
                "Geometry or Grasshopper data to attach attributes to.",
                GH_ParamAccess.tree);

            pManager.AddGenericParameter(
                "Attributes",
                "A",
                "ME Attribute or Rhino ObjectAttributes.",
                GH_ParamAccess.tree);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
            pManager.AddParameter(
                new ME_Object_Param(),
                "ME Objects",
                "O",
                "Geometry/data with attached Meanders attributes.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> geometryTree = null;
            GH_Structure<IGH_Goo> attributeTree = null;

            if (!DA.GetDataTree(0, out geometryTree))
                return;

            DA.GetDataTree(1, out attributeTree);

            GH_Structure<ME_Object_Goo> output =
                new GH_Structure<ME_Object_Goo>();

            foreach (GH_Path path in geometryTree.Paths)
            {
                IList geometryBranch =
                    geometryTree.get_Branch(path);

                IList attributeBranch = null;

                if (attributeTree != null &&
                    attributeTree.PathCount > 0)
                {
                    attributeBranch =
                        attributeTree.get_Branch(path);

                    if (attributeBranch == null)
                    {
                        attributeBranch =
                            attributeTree.get_Branch(0);
                    }
                }

                for (int i = 0; i < geometryBranch.Count; i++)
                {
                    IGH_Goo geometryGoo =
                        geometryBranch[i] as IGH_Goo;

                    object geometry =
                        ExtractValue(geometryGoo);

                    ME_Attribute attribute =
                        GetAttribute(attributeBranch, i);

                    ME_Object meObject =
                        new ME_Object(
                            geometry,
                            attribute != null
                                ? attribute.Attributes
                                : null);

                    output.Append(
                        new ME_Object_Goo(meObject),
                        path);
                }
            }

            DA.SetDataTree(0, output);
        }

        private object ExtractValue(IGH_Goo goo)
        {
            if (goo == null)
                return null;

            // Already an ME Object
            if (goo is ME_Object_Goo meObjectGoo)
            {
                if (meObjectGoo.Value == null)
                    return null;

                return meObjectGoo.Value.Geometry;
            }

            // Common Grasshopper geometry types
            if (goo is GH_Point ghPoint)
                return ghPoint.Value;

            if (goo is GH_Curve ghCurve)
                return ghCurve.Value;

            if (goo is GH_Brep ghBrep)
                return ghBrep.Value;

            if (goo is GH_Surface ghSurface)
                return ghSurface.Value;

            if (goo is GH_SubD ghSubD)
                return ghSubD.Value;

            if (goo is GH_GeometryGroup ghGroup)
                return ghGroup;

            if (goo is GH_Mesh ghMesh)
                return ghMesh.Value;

            if (goo is GH_Rectangle ghRectangle)
                return ghRectangle.Value;

            if (goo is GH_Line ghLine)
                return ghLine.Value;

            // Generic wrapper
            if (goo is GH_ObjectWrapper wrapper)
                return wrapper.Value;

            // Fallback
            return goo;
        }

        private ME_Attribute GetAttribute(
            IList branch,
            int index)
        {
            if (branch == null || branch.Count == 0)
                return null;

            int attributeIndex =
                Math.Min(index, branch.Count - 1);

            IGH_Goo goo =
                branch[attributeIndex] as IGH_Goo;

            if (goo == null)
                return null;

            if (goo is ME_Attribute_Goo meAttributeGoo)
            {
                return meAttributeGoo.Value != null
                    ? meAttributeGoo.Value.Duplicate()
                    : new ME_Attribute();
            }

            if (goo is GH_ObjectWrapper wrapper &&
                wrapper.Value is ME_Attribute meAttribute)
            {
                return meAttribute.Duplicate();
            }

            if (goo is GH_ObjectWrapper wrapper2 &&
                wrapper2.Value is Rhino.DocObjects.ObjectAttributes rhinoAttributes)
            {
                return new ME_Attribute(rhinoAttributes);
            }

            return null;
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        protected override Bitmap Icon =>
    MeandersIconLoader.Load("me-attach-attribute.png");

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "E42B8C71-93F6-4A05-A6D2-5C17F9B83426");
            }
        }
    }
}
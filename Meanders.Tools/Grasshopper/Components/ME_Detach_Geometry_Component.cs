using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Meanders.Tools.Core;
using Meanders.Tools.Grasshopper.Goo;
using Meanders.Tools.Grasshopper.Parameters;
using Meanders.Tools.Plugin;
using System;
using System.Drawing;


namespace Meanders.Tools.Grasshopper.Components
{
    public class ME_Detach_Geometry_Component : GH_Component
    {
        public ME_Detach_Geometry_Component()
            : base(
                "ME Detach Geometry",
                "ME Detach",
                "Detach geometry and attributes from Meanders objects.",
                "Meanders",
                "Objects")
        {
        }

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddParameter(
                new ME_Object_Param(),
                "ME Objects",
                "O",
                "Meanders objects to detach.",
                GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter(
                "Geometry",
                "G",
                "Detached geometry.",
                GH_ParamAccess.tree);

            pManager.AddGenericParameter(
                "Attributes",
                "A",
                "Detached attributes.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            GH_Structure<ME_Object_Goo> objectTree;

            if (!DA.GetDataTree(0, out objectTree))
                return;


            GH_Structure<IGH_Goo> geometryTree =
                new GH_Structure<IGH_Goo>();

            GH_Structure<IGH_Goo> attributeTree =
                new GH_Structure<IGH_Goo>();


            foreach (GH_Path path in objectTree.Paths)
            {
                var branch =
                    objectTree.get_Branch(path);


                foreach (ME_Object_Goo goo in branch)
                {
                    if (goo == null || goo.Value == null)
                        continue;


                    ME_Object obj = goo.Value;


                    geometryTree.Append(
                        new GH_ObjectWrapper(obj.Geometry),
                        path);


                    attributeTree.Append(
                        new ME_Attribute_Goo(
                            new ME_Attribute(obj.Attributes)),
                        path);
                }
            }


            DA.SetDataTree(0, geometryTree);
            DA.SetDataTree(1, attributeTree);
        }


        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }


        protected override Bitmap Icon =>
     MeandersIconLoader.Load("me-detach-geometry.png");


        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "B7A8C3D2-8A7B-4B4A-9F25-6D2D7E8A9F11");
            }
        }
    }
}
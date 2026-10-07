using Grasshopper.Kernel;
using Meanders.Tools.Core.Fabrication;
using Meanders.Tools.Plugin;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Meanders.Tools.Grasshopper.Components
{
    public class ME_Fab_Text_Component : GH_Component
    {
        public ME_Fab_Text_Component()
            : base(
                "ME Fab Text",
                "ME Fab",
                "Create fabrication-ready text geometry with fabrication tokens.",
                "Meanders",
                "Fabrication")
        {
        }

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter(
                "Plane",
                "P",
                "Plane used to place the fabrication text.",
                GH_ParamAccess.item);

            pManager.AddTextParameter(
                "Text",
                "T",
                "Fabrication text and fabrication tokens.",
                GH_ParamAccess.item);

            pManager.AddNumberParameter(
                "Text Size",
                "S",
                "Text height.",
                GH_ParamAccess.item);

            pManager.AddIntegerParameter(
                "Justification",
                "J",
                "Text justification. 0=Bottom Left, 4=Middle Center, 8=Top Right.",
                GH_ParamAccess.item);

            pManager.AddNumberParameter(
                "Spacing",
                "Sp",
                "Additional spacing between fabrication characters.",
                GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter(
                "Polylines",
                "C",
                "Fabrication text polylines.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            Plane plane =
                Plane.WorldXY;

            string text =
                string.Empty;

            double textSize =
                1.0;

            int justification =
                4;

            double spacing =
                0.0;

            // P = 0
            DA.GetData(
                0,
                ref plane);

            // T = 1
            if (!DA.GetData(
                1,
                ref text))
            {
                return;
            }

            // S = 2
            if (DA.GetData(
                2,
                ref textSize))
            {
                if (textSize <= 0.0)
                {
                    AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Error,
                        "Text Size must be greater than zero.");

                    return;
                }
            }

            // J = 3
            DA.GetData(
                3,
                ref justification);

            // Sp = 4
            DA.GetData(
                4,
                ref spacing);

            if (spacing < 0.0)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "Spacing cannot be negative.");

                return;
            }

            if (
                justification < 0 ||
                justification > 8)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "Justification must be between 0 and 8.");

                return;
            }

            FabTextJustification layoutJustification =
                (FabTextJustification)justification;

            double tolerance =
                RhinoDoc.ActiveDoc != null
                    ? RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
                    : 0.01;

            /*
             * Temporary fallback slot width.
             *
             * The actual character geometry is measured
             * later by the layout engine.
             */
            double characterWidth =
                textSize * 0.6;

            try
            {
                List<Curve> curves =
                    FabTextBuilder.Build(
                        text,
                        plane,
                        characterWidth,
                        textSize,
                        spacing,
                        layoutJustification,
                        tolerance);

                List<Polyline> polylines =
                    FabTextPolyline.Convert(
                        curves,
                        tolerance);

                DA.SetDataList(
                    0,
                    polylines);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    ex.ToString());
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
                "me-fab-text.png");

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "D4A7C92E-8B31-4F65-A2C8-71E9B53F406D");
            }
        }
    }
}
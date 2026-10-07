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
                "Text containing fabrication tokens.",
                GH_ParamAccess.item);

            pManager.AddNumberParameter(
                "Text Size",
                "S",
                "Height of the fabrication text.",
                GH_ParamAccess.item);

            pManager.AddIntegerParameter(
                "Justification",
                "J",
                "Text justification: 0=Bottom Left, 4=Middle Center, 8=Top Right.",
                GH_ParamAccess.item);

            pManager.AddNumberParameter(
                "Spacing",
                "Sp",
                "Additional spacing between character slots.",
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
                "Curves",
                "C",
                "Fabrication text curves.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            Plane plane = Plane.WorldXY;

            string text = string.Empty;

            // Defaults
            double textSize = 1.0;
            int justification = 4;
            double spacing = 0.0;

            /*
             * Plane
             */
            DA.GetData(
                0,
                ref plane);

            /*
             * Text
             */
            if (!DA.GetData(
                1,
                ref text))
            {
                return;
            }

            /*
             * Text Size
             *
             * Default = 1.0
             */
            DA.GetData(
                2,
                ref textSize);

            /*
             * Justification
             *
             * Default = 4
             * Middle Center
             */
            DA.GetData(
                3,
                ref justification);

            /*
             * Spacing
             *
             * Default = 0.0
             *
             * The layout engine converts this into
             * the minimum/default character gap.
             */
            DA.GetData(
                4,
                ref spacing);

            /*
             * Validation
             */
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (textSize <= 0.0)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "Text Size must be greater than zero.");

                return;
            }

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
                (FabTextJustification)
                justification;

            double tolerance =
                RhinoDoc.ActiveDoc != null
                    ? RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
                    : 0.001;

            /*
             * Kept for compatibility with the current
             * factory API.
             *
             * Normal glyph width now comes from
             * glyphs.json.
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

                DA.SetDataList(
                    0,
                    curves);
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

        protected override Bitmap Icon
        {
            get
            {
                return MeandersIconLoader.Load(
                    "me-fab-text.png");
            }
        }

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
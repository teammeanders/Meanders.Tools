using GH_IO.Serialization;
using Grasshopper.Kernel;
using Meanders.Tools.Core.Fabrication;
using Meanders.Tools.Plugin;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Meanders.Tools.Grasshopper.Components
{
    public class ME_Fab_Text_Component : GH_Component
    {
        private int _justification = 4;

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
                "Text justification. 0=Bottom Left, 1=Bottom Center, 2=Bottom Right, 3=Middle Left, 4=Middle Center, 5=Middle Right, 6=Top Left, 7=Top Center, 8=Top Right.",
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
            Plane plane =
                Plane.WorldXY;

            string text =
                string.Empty;

            double textSize =
                1.0;

            /*
             * Context-menu value is the default.
             *
             * If J is connected, the supplied input
             * value overrides this value.
             */
            int justification =
                _justification;

            double spacing =
                0.0;

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
             */
            DA.GetData(
                2,
                ref textSize);

            /*
             * Justification
             *
             * If the input is connected, use its value.
             * Otherwise use the persistent context-menu value.
             */
            if (DA.GetData(
                3,
                ref justification))
            {
                if (
                    justification < 0 ||
                    justification > 8)
                {
                    AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Error,
                        "Justification must be between 0 and 8.");

                    return;
                }

                /*
                 * Keep the menu state synchronized with
                 * an explicit J input.
                 */
                _justification =
                    justification;
            }

            /*
             * Spacing
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
             * builder API.
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

        protected override void AppendAdditionalComponentMenuItems(
            ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(
                menu);

            ToolStripMenuItem justificationMenu =
                new ToolStripMenuItem(
                    "Justification");

            foreach (
                FabTextJustification justification
                in Enum.GetValues(
                    typeof(FabTextJustification)))
            {
                int value =
                    (int)justification;

                ToolStripMenuItem item =
                    new ToolStripMenuItem(
                        GetJustificationName(
                            justification));

                item.Checked =
                    value == _justification;

                item.Click += (
                    sender,
                    e) =>
                {
                    _justification =
                        value;

                    ExpireSolution(
                        true);
                };

                justificationMenu.DropDownItems.Add(
                    item);
            }

            menu.Items.Add(
                justificationMenu);
        }

        private static string GetJustificationName(
            FabTextJustification justification)
        {
            switch (justification)
            {
                case FabTextJustification.BottomLeft:
                    return "Bottom Left";

                case FabTextJustification.BottomCenter:
                    return "Bottom Center";

                case FabTextJustification.BottomRight:
                    return "Bottom Right";

                case FabTextJustification.MiddleLeft:
                    return "Middle Left";

                case FabTextJustification.MiddleCenter:
                    return "Middle Center";

                case FabTextJustification.MiddleRight:
                    return "Middle Right";

                case FabTextJustification.TopLeft:
                    return "Top Left";

                case FabTextJustification.TopCenter:
                    return "Top Center";

                case FabTextJustification.TopRight:
                    return "Top Right";

                default:
                    return justification.ToString();
            }
        }

        public override bool Write(
            GH_IWriter writer)
        {
            writer.SetInt32(
                "Justification",
                _justification);

            return base.Write(
                writer);
        }

        public override bool Read(
            GH_IReader reader)
        {
            if (reader.ItemExists(
                "Justification"))
            {
                int value =
                    reader.GetInt32(
                        "Justification");

                if (
                    value >= 0 &&
                    value <= 8)
                {
                    _justification =
                        value;
                }
            }

            return base.Read(
                reader);
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
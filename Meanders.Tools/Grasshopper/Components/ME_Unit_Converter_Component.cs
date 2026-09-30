using Grasshopper.Kernel;
using Meanders.Tools.Core;
using System;
using System.Drawing;
using GH_IO.Serialization;

namespace Meanders.Tools.Grasshopper.Components
{
    public class ME_Unit_Converter_Component : GH_Component
    {
        private ME_UnitConverter.LengthUnit _fromUnit =
    ME_UnitConverter.LengthUnit.Millimeter;

        private ME_UnitConverter.LengthUnit _toUnit =
            ME_UnitConverter.LengthUnit.Centimeter;

        private bool _invert = false;


        public ME_Unit_Converter_Component()
     : base(
         "ME Unit Converter",
         "ME Units",
         "Convert between supported length units.",
         "Meanders",
         "Utilities")
        {
            UpdateMessage();
        }

        private string ShortUnit(
    ME_UnitConverter.LengthUnit unit)
        {
            switch (unit)
            {
                case ME_UnitConverter.LengthUnit.Millimeter:
                    return "mm";

                case ME_UnitConverter.LengthUnit.Centimeter:
                    return "cm";

                case ME_UnitConverter.LengthUnit.Meter:
                    return "m";

                case ME_UnitConverter.LengthUnit.Inch:
                    return "in";

                case ME_UnitConverter.LengthUnit.Foot:
                    return "ft";

                default:
                    return unit.ToString();
            }
        }

        private void UpdateMessage()
        {
            string arrow = _invert ? "←" : "→";

            Message =
                $"Length\n{ShortUnit(_fromUnit)} {arrow} {ShortUnit(_toUnit)}";
        }

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter(
                "Value",
                "X",
                "Value to convert.",
                GH_ParamAccess.item);
        }



        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter(
                "Result",
                "Y",
                "Converted value.",
                GH_ParamAccess.item);
        }


        protected override void SolveInstance(
    IGH_DataAccess DA)
        {
            double value = 0;

            if (!DA.GetData(0, ref value))
                return;


            var from = _fromUnit;
            var to = _toUnit;


            if (_invert)
            {
                from = _toUnit;
                to = _fromUnit;
            }


            double result =
                ME_UnitConverter.ConvertLength(
                    value,
                    from,
                    to);


            DA.SetData(0, result);


            UpdateMessage();
        }


        protected override void AppendAdditionalComponentMenuItems(
            System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);


            var fromMenu =
                Menu_UnitMenu(
                    "From Unit",
                    true);

            menu.Items.Add(fromMenu);


            var toMenu =
                Menu_UnitMenu(
                    "To Unit",
                    false);

            menu.Items.Add(toMenu);

            var invert =
    new System.Windows.Forms.ToolStripMenuItem(
        "Invert");

            invert.Checked = _invert;

            invert.Click += (sender, e) =>
            {
                _invert = !_invert;
                UpdateMessage();
                ExpireSolution(true);
            };

            menu.Items.Add(invert);
        }


        private System.Windows.Forms.ToolStripMenuItem Menu_UnitMenu(
            string title,
            bool from)
        {
            var parent =
                new System.Windows.Forms.ToolStripMenuItem(title);


            foreach (
    ME_UnitConverter.LengthUnit unit
    in Enum.GetValues(
        typeof(ME_UnitConverter.LengthUnit)))
            {
                var item =
                    new System.Windows.Forms.ToolStripMenuItem(
                        unit.ToString());

                ME_UnitConverter.LengthUnit currentUnit =
                    from ? _fromUnit : _toUnit;

                item.Checked =
                    unit == currentUnit;

                item.Click += (sender, e) =>
                {
                    if (from)
                        _fromUnit = unit;
                    else
                        _toUnit = unit;

                    UpdateMessage();

                    ExpireSolution(true);
                };

                parent.DropDownItems.Add(item);
            }


            return parent;
        }


        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }


        protected override Bitmap Icon
        {
            get { return null; }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32(
                "FromUnit",
                (int)_fromUnit);

            writer.SetInt32(
                "ToUnit",
                (int)_toUnit);

            writer.SetBoolean(
                "Invert",
                _invert);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("FromUnit"))
            {
                _fromUnit =
                    (ME_UnitConverter.LengthUnit)
                    reader.GetInt32("FromUnit");
            }

            if (reader.ItemExists("ToUnit"))
            {
                _toUnit =
                    (ME_UnitConverter.LengthUnit)
                    reader.GetInt32("ToUnit");
            }

            if (reader.ItemExists("Invert"))
            {
                _invert =
                    reader.GetBoolean("Invert");
            }

            UpdateMessage();

            return base.Read(reader);
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "5A4E9E1D-4B3D-4D0B-8E4B-1C9F9F0D3A71");
            }
        }
    }
}
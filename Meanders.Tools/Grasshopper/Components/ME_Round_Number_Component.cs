using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Meanders.Tools.Plugin;

namespace Meanders.Tools.Grasshopper.Components
{
    public class ME_Round_Number_Component : GH_Component
    {
        public ME_Round_Number_Component()
            : base(
                "ME Round Number",
                "ME Round",
                "Round numbers to a specified number of decimal places.",
                "Meanders",
                "Utilities")
        {
        }

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter(
                "Number",
                "N",
                "Numbers to round.",
                GH_ParamAccess.list);

            pManager.AddIntegerParameter(
                "Decimal Places",
                "D",
                "Number of decimal places to keep.",
                GH_ParamAccess.item,
                2);

            pManager[0].Optional = false;
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter(
                "Rounded",
                "R",
                "Rounded numbers.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            List<double> numbers =
                new List<double>();

            int decimalPlaces = 2;

            if (!DA.GetDataList(
                0,
                numbers))
            {
                return;
            }

            DA.GetData(
                1,
                ref decimalPlaces);

            if (decimalPlaces < 0)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "Decimal Places cannot be negative.");

                return;
            }

            /*
             * Prevent an unreasonable value from
             * reaching Math.Round.
             */
            if (decimalPlaces > 15)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "Decimal Places must be between 0 and 15.");

                return;
            }

            List<double> rounded =
                new List<double>(
                    numbers.Count);

            foreach (
                double number
                in numbers)
            {
                if (double.IsNaN(number) ||
                    double.IsInfinity(number))
                {
                    AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Error,
                        "Number contains an invalid numeric value.");

                    return;
                }

                /*
                 * Use AwayFromZero so that values
                 * exactly halfway between two
                 * decimal values behave predictably.
                 *
                 * Example:
                 *
                 * 1.125 → 1.13
                 */
                double result =
                    Math.Round(
                        number,
                        decimalPlaces,
                        MidpointRounding.AwayFromZero);

                rounded.Add(result);
            }

            DA.SetDataList(
                0,
                rounded);
        }

        protected override Bitmap Icon
        {
            get
            {
                return MeandersIconLoader.Load(
                    "me-round-number.png");
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "7B8D4F21-6C93-4A57-9E18-2F6D83B4A901");
            }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.primary;
            }
        }
    }
}
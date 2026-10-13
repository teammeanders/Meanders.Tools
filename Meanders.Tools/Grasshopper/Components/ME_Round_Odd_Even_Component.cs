using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Meanders.Tools.Plugin;

namespace Meanders.Tools.Grasshopper.Components
{
    public class ME_Round_Odd_Even_Component : GH_Component
    {
        public ME_Round_Odd_Even_Component()
            : base(
                "ME Round to Odd/Even",
                "ME Odd/Even",
                "Round numbers to the nearest integer with a specified odd or even result.",
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
                "Mode",
                "M",
                "Rounding mode. 0 = Even, 1 = Odd.",
                GH_ParamAccess.list,
                0);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter(
                "Rounded",
                "R",
                "Numbers rounded to the nearest integer with the requested parity.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            List<double> numbers =
                new List<double>();

            List<int> modes =
                new List<int>();

            if (!DA.GetDataList(
                0,
                numbers))
            {
                return;
            }

            /*
             * Mode is optional.
             *
             * Default:
             * 0 = Even
             */
            if (!DA.GetDataList(
                1,
                modes))
            {
                modes.Add(0);
            }

            /*
             * Validate modes.
             */
            foreach (int mode in modes)
            {
                if (mode != 0 && mode != 1)
                {
                    AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Error,
                        "Mode must be 0 (Even) or 1 (Odd).");

                    return;
                }
            }

            /*
             * Match the input lists using
             * Grasshopper's standard
             * Repeat Last behavior.
             *
             * Do not create a custom
             * matching system here.
             */
            int count =
    numbers.Count;

            if (count == 0)
            {
                return;
            }

            List<double> rounded =
                new List<double>(count);

            for (int i = 0; i < count; i++)
            {
                /*
                 * Repeat Last behavior.
                 */
                double number =
    numbers[i];

                int mode =
                    modes[
                        Math.Min(
                            i,
                            modes.Count - 1)];

                if (
                    double.IsNaN(number) ||
                    double.IsInfinity(number))
                {
                    AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Error,
                        "Number contains an invalid numeric value.");

                    return;
                }

                double result =
                    RoundToParity(
                        number,
                        mode);

                rounded.Add(result);
            }

            DA.SetDataList(
                0,
                rounded);
        }

        private static double RoundToParity(
            double number,
            int mode)
        {
            /*
             * Find the two nearest integers.
             */
            double lower =
                Math.Floor(number);

            double upper =
                Math.Ceiling(number);

            /*
             * Exact integer.
             */
            if (lower == upper)
            {
                int integer =
                    (int)lower;

                bool isEven =
                    integer % 2 == 0;

                bool wantsEven =
                    mode == 0;

                if (
                    isEven == wantsEven)
                {
                    return integer;
                }

                /*
                 * The number is already an integer,
                 * but has the wrong parity.
                 *
                 * Choose the adjacent integer.
                 */
                double down =
                    integer - 1;

                double up =
                    integer + 1;

                /*
                 * Both are equally distant.
                 * Prefer the smaller integer.
                 */
                return down;
            }

            /*
             * Distances to lower/upper integers.
             */
            double lowerDistance =
                Math.Abs(
                    number - lower);

            double upperDistance =
                Math.Abs(
                    upper - number);

            /*
             * Check parity.
             */
            bool lowerMatches =
                IsRequestedParity(
                    lower,
                    mode);

            bool upperMatches =
                IsRequestedParity(
                    upper,
                    mode);

            /*
             * Only lower has requested parity.
             */
            if (
                lowerMatches &&
                !upperMatches)
            {
                return lower;
            }

            /*
             * Only upper has requested parity.
             */
            if (
                !lowerMatches &&
                upperMatches)
            {
                return upper;
            }

            /*
             * Both should normally never happen
             * for adjacent integers, but keep the
             * fallback deterministic.
             */
            if (
                lowerMatches &&
                upperMatches)
            {
                return
                    lowerDistance <=
                    upperDistance
                        ? lower
                        : upper;
            }

            /*
             * Neither integer has requested parity.
             *
             * This should not occur for adjacent
             * integers, because one integer is always
             * even and the other is always odd.
             */
            return
                lowerDistance <=
                upperDistance
                    ? lower
                    : upper;
        }

        private static bool IsRequestedParity(
            double value,
            int mode)
        {
            int integer =
                (int)value;

            bool isEven =
                integer % 2 == 0;

            return mode == 0
                ? isEven
                : !isEven;
        }

        protected override Bitmap Icon
        {
            get
            {
                return MeandersIconLoader.Load(
                    "me-round-odd-even.png");
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "A63F2D91-4C87-4B25-9E16-73D8C5A204BF");
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
using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Meanders.Tools.Core.Fabrication
{
    public static class FabTextBold
    {
        /*
         * Amount of geometric expansion relative to
         * the text height.
         *
         * Example:
         * 100 mm text -> 8 mm total expansion
         * around the glyph boundary.
         */
        private const double BoldRatio = 0.08;

        public static List<Curve> Apply(
            IEnumerable<Curve> curves,
            double textHeight,
            Plane plane,
            double tolerance)
        {
            if (curves == null)
                throw new ArgumentNullException(
                    nameof(curves));

            if (textHeight <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(textHeight));

            if (tolerance <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));

            double distance =
                textHeight * BoldRatio;

            var result =
                new List<Curve>();

            foreach (Curve curve in curves)
            {
                if (curve == null)
                    continue;

                /*
                 * Open curves are kept unchanged.
                 *
                 * This is important for fabrication tokens
                 * such as arrows and edge markers.
                 */
                if (!curve.IsClosed)
                {
                    result.Add(
                        curve.DuplicateCurve());

                    continue;
                }

                CurveOrientation orientation =
                    curve.ClosedCurveOrientation(
                        plane);

                /*
                 * Outer and inner contours need opposite
                 * offset directions.
                 *
                 * Counter-clockwise:
                 *   negative = expand outward
                 *
                 * Clockwise:
                 *   positive = expand outward for the
                 *   corresponding inner contour.
                 */
                double offsetDistance;

                if (
                    orientation ==
                    CurveOrientation.CounterClockwise)
                {
                    offsetDistance =
                        -distance;
                }
                else if (
                    orientation ==
                    CurveOrientation.Clockwise)
                {
                    offsetDistance =
                        distance;
                }
                else
                {
                    /*
                     * If Rhino cannot determine the
                     * orientation, keep the original curve
                     * instead of risking invalid geometry.
                     */
                    result.Add(
                        curve.DuplicateCurve());

                    continue;
                }

                Curve[] offsets =
                    curve.Offset(
                        plane,
                        offsetDistance,
                        tolerance,
                        CurveOffsetCornerStyle.Round);

                if (
                    offsets == null ||
                    offsets.Length == 0)
                {
                    /*
                     * Safe fallback:
                     * never lose the original glyph.
                     */
                    result.Add(
                        curve.DuplicateCurve());

                    continue;
                }

                foreach (Curve offset in offsets)
                {
                    if (offset != null)
                    {
                        result.Add(offset);
                    }
                }
            }

            return result;
        }
    }
}
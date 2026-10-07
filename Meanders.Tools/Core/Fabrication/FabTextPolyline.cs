using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;

namespace Meanders.Tools.Core.Fabrication
{
    public static class FabTextPolyline
    {
        public static List<Polyline> Convert(
            IEnumerable<Curve> curves,
            double tolerance)
        {
            if (curves == null)
                throw new ArgumentNullException(
                    nameof(curves));

            if (tolerance <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));

            var result = new List<Polyline>();

            foreach (Curve curve in curves)
            {
                if (curve == null)
                    continue;

                if (TryConvert(
                    curve,
                    tolerance,
                    out Polyline polyline))
                {
                    result.Add(polyline);
                }
            }

            return result;
        }

        private static bool TryConvert(
            Curve curve,
            double tolerance,
            out Polyline polyline)
        {
            polyline = new Polyline();

            /*
             * If the curve is already representable
             * as a polyline, keep it exactly as it is.
             */
            if (curve.TryGetPolyline(
                out polyline))
            {
                return polyline.Count >= 2;
            }

            /*
             * Otherwise approximate the curve.
             *
             * Use the document-independent overload:
             *
             * tolerance
             * angle tolerance
             * minimum segment length
             * maximum segment length
             */
            PolylineCurve polylineCurve =
                curve.ToPolyline(
                    tolerance,
                    RhinoMath.ToRadians(1.0),
                    0.0,
                    0.0);

            if (polylineCurve == null)
                return false;

            return polylineCurve.TryGetPolyline(
                out polyline)
                && polyline.Count >= 2;
        }
    }
}
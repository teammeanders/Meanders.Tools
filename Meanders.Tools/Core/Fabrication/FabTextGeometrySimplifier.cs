using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;

namespace Meanders.Tools.Core.Fabrication
{
    public static class FabTextGeometrySimplifier
    {
        public static List<Curve> Simplify(
            IEnumerable<Curve> curves,
            double tolerance)
        {
            if (curves == null)
                throw new ArgumentNullException(
                    nameof(curves));

            if (tolerance <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));

            var result =
                new List<Curve>();

            /*
             * Use a slightly relaxed tolerance for
             * fabrication geometry.
             *
             * The goal is not mathematical overkill.
             * The goal is lightweight, clean geometry.
             */
            double simplifyTolerance =
                Math.Max(
                    tolerance,
                    0.0001);

            double angleTolerance =
                RhinoMath.ToRadians(3.0);

            foreach (Curve curve in curves)
            {
                if (curve == null)
                    continue;

                /*
                 * --------------------------------------------------
                 * 1. Keep real lines as lines.
                 * --------------------------------------------------
                 */
                if (curve is LineCurve)
                {
                    result.Add(
                        curve.DuplicateCurve());

                    continue;
                }

                /*
                 * --------------------------------------------------
                 * 2. Detect circles BEFORE polyline conversion.
                 *
                 * This is important for glyphs like O.
                 * --------------------------------------------------
                 */
                if (curve.TryGetCircle(
                    out Circle circle,
                    simplifyTolerance))
                {
                    result.Add(
                        circle.ToNurbsCurve());

                    continue;
                }

                /*
                 * --------------------------------------------------
                 * 3. Detect arcs.
                 * --------------------------------------------------
                 */
                if (curve.TryGetArc(
                    out Arc arc,
                    simplifyTolerance))
                {
                    result.Add(
                        arc.ToNurbsCurve());

                    continue;
                }

                /*
                 * --------------------------------------------------
                 * 4. Let Rhino simplify the curve.
                 *
                 * This can turn:
                 *
                 * NURBS → lines
                 * NURBS → arcs
                 * adjacent lines → polyline
                 * adjacent arcs → arcs
                 *
                 * and removes unnecessary complexity.
                 * --------------------------------------------------
                 */
                Curve simplified =
                    curve.Simplify(
                        CurveSimplifyOptions.All,
                        simplifyTolerance,
                        angleTolerance);

                if (simplified == null)
                    simplified =
                        curve.DuplicateCurve();

                /*
                 * --------------------------------------------------
                 * 5. Re-check for circle after simplification.
                 * --------------------------------------------------
                 */
                if (simplified.TryGetCircle(
                    out circle,
                    simplifyTolerance))
                {
                    result.Add(
                        circle.ToNurbsCurve());

                    continue;
                }

                /*
                 * --------------------------------------------------
                 * 6. If it is already a polyline, preserve it.
                 * --------------------------------------------------
                 */
                if (simplified.TryGetPolyline(
                    out Polyline polyline))
                {
                    if (polyline.Count >= 2)
                    {
                        result.Add(
                            new PolylineCurve(
                                polyline));
                    }

                    continue;
                }

                /*
                 * --------------------------------------------------
                 * 7. Last resort:
                 * convert to a lightweight polyline.
                 * --------------------------------------------------
                 */
                PolylineCurve approximation =
                    simplified.ToPolyline(
                        simplifyTolerance,
                        angleTolerance,
                        0.0,
                        0.0);

                if (approximation == null)
                    continue;

                if (
                    approximation.TryGetPolyline(
                        out Polyline outputPolyline)
                    &&
                    outputPolyline.Count >= 2)
                {
                    result.Add(
                        new PolylineCurve(
                            outputPolyline));
                }
            }

            return result;
        }
    }
}
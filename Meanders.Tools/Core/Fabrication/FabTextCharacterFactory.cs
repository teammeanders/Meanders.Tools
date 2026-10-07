using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Meanders.Tools.Core.Fabrication
{
    public static class FabTextCharacterFactory
    {
        public static FabTextCharacter Create(
            FabTextToken token,
            double characterWidth,
            double characterHeight,
            double tolerance)
        {
            if (token == null)
                throw new ArgumentNullException(
                    nameof(token));

            if (characterWidth <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(characterWidth));

            if (characterHeight <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(characterHeight));

            if (tolerance <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));

            switch (token.Type)
            {
                case FabTextTokenType.Text:
                    return CreateText(
                        token,
                        characterWidth,
                        characterHeight,
                        tolerance);

                case FabTextTokenType.Arrow:
                    return CreateArrow(
                        token,
                        characterWidth,
                        characterHeight,
                        tolerance);

                case FabTextTokenType.EdgeMarker:
                    return CreateEdgeMarker(
                        token,
                        characterWidth,
                        characterHeight,
                        tolerance);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(token.Type));
            }
        }

        private static FabTextCharacter CreateText(
    FabTextToken token,
    double fallbackWidth,
    double height,
    double tolerance)
        {
            Curve[] curves =
                FabTextGeometry.CreateTextOutlines(
                    token.Text,
                    Plane.WorldXY,
                    height,
                    false,
                    tolerance);

            double width = fallbackWidth;

            if (curves.Length > 0)
            {
                BoundingBox bounds =
                    BoundingBox.Unset;

                foreach (Curve curve in curves)
                {
                    if (curve == null)
                        continue;

                    BoundingBox curveBounds =
                        curve.GetBoundingBox(true);

                    if (!bounds.IsValid)
                    {
                        bounds = curveBounds;
                    }
                    else
                    {
                        bounds.Union(curveBounds);
                    }
                }

                if (bounds.IsValid)
                {
                    double measuredWidth =
                        bounds.Max.X - bounds.Min.X;

                    if (measuredWidth > tolerance)
                    {
                        width = measuredWidth;
                    }
                }
            }

            return new FabTextCharacter(
                token,
                width,
                height,
                curves);
        }

        private static FabTextCharacter CreateArrow(
     FabTextToken token,
     double width,
     double height,
     double tolerance)
        {
            /*
             * Every Arrow is a square character slot.
             *
             * Example:
             * Text Size = 1
             * Arrow slot = 1 x 1
             */

            double size = height;

            double margin = size * 0.08;

            double min = -size * 0.5 + margin;
            double max = size * 0.5 - margin;

            double shaftLength = max - min;

            double headLength = size * 0.28;
            double headWidth = size * 0.22;

            Point3d start =
                new Point3d(
                    min,
                    0.0,
                    0.0);

            Point3d end =
                new Point3d(
                    max,
                    0.0,
                    0.0);

            Vector3d direction =
                end - start;

            direction.Unitize();

            Vector3d perpendicular =
                Vector3d.CrossProduct(
                    direction,
                    Vector3d.ZAxis);

            perpendicular.Unitize();

            Point3d headBase =
                end -
                direction * headLength;

            Point3d left =
                headBase +
                perpendicular * headWidth;

            Point3d right =
                headBase -
                perpendicular * headWidth;

            var polyline =
                new Polyline(
                    new[]
                    {
                start,
                end,
                left,
                end,
                right
                    });

            Curve curve =
                polyline.ToPolylineCurve();

            double angleRadians =
                RhinoMath.ToRadians(
                    token.Angle);

            Transform rotation =
                Transform.Rotation(
                    angleRadians,
                    Point3d.Origin);

            curve.Transform(rotation);

            return new FabTextCharacter(
                token,
                size,
                size,
                new[] { curve });
        }

        private static FabTextCharacter CreateEdgeMarker(
    FabTextToken token,
    double width,
    double height,
    double tolerance)
        {
            /*
             * Edge marker is always a square character.
             */
            double size = height;

            double half =
                size * 0.5;

            Point3d bottomLeft =
                new Point3d(
                    -half,
                    -half,
                    0.0);

            Point3d bottomRight =
                new Point3d(
                    half,
                    -half,
                    0.0);

            Point3d topRight =
                new Point3d(
                    half,
                    half,
                    0.0);

            Point3d topLeft =
                new Point3d(
                    -half,
                    half,
                    0.0);

            var curves =
                new List<Curve>();

            AddEdge(
                curves,
                token.Edge,
                "bottom",
                bottomLeft,
                bottomRight);

            AddEdge(
                curves,
                token.Edge,
                "right",
                bottomRight,
                topRight);

            AddEdge(
                curves,
                token.Edge,
                "top",
                topRight,
                topLeft);

            AddEdge(
                curves,
                token.Edge,
                "left",
                topLeft,
                bottomLeft);

            return new FabTextCharacter(
                token,
                size,
                size,
                curves.ToArray());
        }

        private static void AddEdge(
     System.Collections.Generic.List<Curve> curves,
     FabTextEdge mode,
     string edge,
     Point3d start,
     Point3d end)
        {
            if (ShouldDrawEdge(mode, edge))
            {
                curves.Add(
                    new LineCurve(
                        start,
                        end));
            }
        }

        private static bool ShouldDrawEdge(
            FabTextEdge mode,
            string edge)
        {
            switch (mode)
            {
                case FabTextEdge.Full:
                    return true;

                case FabTextEdge.BottomRight:
                    return edge == "bottom" ||
                           edge == "right";

                case FabTextEdge.TopRight:
                    return edge == "top" ||
                           edge == "right";

                case FabTextEdge.TopLeft:
                    return edge == "top" ||
                           edge == "left";

                case FabTextEdge.BottomLeft:
                    return edge == "bottom" ||
                           edge == "left";

                case FabTextEdge.NoBottom:
                    return edge != "bottom";

                case FabTextEdge.NoRight:
                    return edge != "right";

                case FabTextEdge.NoTop:
                    return edge != "top";

                case FabTextEdge.NoLeft:
                    return edge != "left";

                default:
                    return false;
            }
        }
    }
}
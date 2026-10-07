using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Meanders.Tools.Core.Fabrication
{
    public static class FabTextCharacterFactory
    {
        private static FabTextGlyphLibrary _glyphLibrary;

        private static FabTextGlyphLibrary GlyphLibrary
        {
            get
            {
                if (_glyphLibrary == null)
                {
                    _glyphLibrary =
                        FabTextGlyphLibrary.Load();
                }

                return _glyphLibrary;
            }
        }

        public static FabTextCharacter Create(
            FabTextToken token,
            double characterWidth,
            double characterHeight,
            double tolerance)
        {
            if (token == null)
                throw new ArgumentNullException(
                    nameof(token));

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
                        characterHeight);

                case FabTextTokenType.Arrow:
                    return CreateArrow(
                        token,
                        characterHeight);

                case FabTextTokenType.EdgeMarker:
                    return CreateEdgeMarker(
                        token,
                        characterHeight);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(token.Type));
            }
        }

        private static FabTextCharacter CreateText(
            FabTextToken token,
            double height)
        {
            if (!GlyphLibrary.Contains(
                token.Text))
            {
                throw new InvalidOperationException(
                    "FabText glyph is not supported: " +
                    token.Text);
            }

            List<Curve> sourceCurves =
                GlyphLibrary.CreateCurves(
                    token.Text);

            var curves =
                new List<Curve>(
                    sourceCurves.Count);

            /*
             * The glyph library is authored at Text Size = 1.
             *
             * Scale it uniformly to the requested text size.
             */
            double scale =
                height;

            Transform scaleTransform =
                Transform.Scale(
                    Point3d.Origin,
                    scale);

            BoundingBox bounds =
                BoundingBox.Unset;

            foreach (Curve source in sourceCurves)
            {
                if (source == null)
                    continue;

                Curve curve =
                    source.DuplicateCurve();

                curve.Transform(
                    scaleTransform);

                curves.Add(curve);

                BoundingBox curveBounds =
                    curve.GetBoundingBox(true);

                if (!curveBounds.IsValid)
                    continue;

                if (!bounds.IsValid)
                {
                    bounds =
                        curveBounds;
                }
                else
                {
                    bounds.Union(
                        curveBounds);
                }
            }

            double width =
                bounds.IsValid
                    ? bounds.Max.X - bounds.Min.X
                    : height;

            return new FabTextCharacter(
                token,
                width,
                height,
                curves.ToArray());
        }

        private static FabTextCharacter CreateArrow(
            FabTextToken token,
            double height)
        {
            /*
             * One square character slot.
             */
            double size =
                height;

            double margin =
                size * 0.08;

            double min =
                -size * 0.5 + margin;

            double max =
                size * 0.5 - margin;

            double headLength =
                size * 0.28;

            double headWidth =
                size * 0.22;

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

            /*
             * Three independent segments.
             *
             * No duplicated/retraced shaft.
             */
            var curves =
                new List<Curve>
                {
                    new LineCurve(
                        start,
                        end),

                    new LineCurve(
                        end,
                        left),

                    new LineCurve(
                        end,
                        right)
                };

            double angle =
                RhinoMath.ToRadians(
                    token.Angle);

            Transform rotation =
                Transform.Rotation(
                    angle,
                    Point3d.Origin);

            foreach (Curve curve in curves)
            {
                curve.Transform(
                    rotation);
            }

            return new FabTextCharacter(
                token,
                size,
                size,
                curves.ToArray());
        }

        private static FabTextCharacter CreateEdgeMarker(
            FabTextToken token,
            double height)
        {
            /*
             * EG is always a square character slot.
             */
            double size =
                height;

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
            List<Curve> curves,
            FabTextEdge mode,
            string edge,
            Point3d start,
            Point3d end)
        {
            if (!ShouldDrawEdge(
                mode,
                edge))
            {
                return;
            }

            curves.Add(
                new LineCurve(
                    start,
                    end));
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
using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;

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
            {
                throw new ArgumentNullException(
                    nameof(token));
            }

            if (characterHeight <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(characterHeight));
            }

            if (tolerance <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));
            }

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
                    throw new InvalidOperationException(
                        "Unknown FabText token type: " +
                        token.Type);
            }
        }

        public static bool Supports(
            FabTextToken token)
        {
            if (token == null)
            {
                return false;
            }

            switch (token.Type)
            {
                case FabTextTokenType.Text:
                    return GlyphLibrary.Contains(
                        token.Text);

                case FabTextTokenType.Arrow:
                    return true;

                case FabTextTokenType.EdgeMarker:
                    return true;

                default:
                    return false;
            }
        }

        public static string GetDescription(
            FabTextToken token)
        {
            if (token == null)
            {
                return "Unknown fabrication token.";
            }

            switch (token.Type)
            {
                case FabTextTokenType.Text:
                    return
                        "Character '" +
                        token.Text +
                        "'";

                case FabTextTokenType.Arrow:
                    return
                        "<AR-" +
                        token.Angle.ToString("0.###") +
                        ">";

                case FabTextTokenType.EdgeMarker:
                    return
                        "<EG-" +
                        GetEdgeCode(token.Edge) +
                        ">";

                default:
                    return
                        "Unknown fabrication token.";
            }
        }

        private static string GetEdgeCode(
            FabTextEdge edge)
        {
            switch (edge)
            {
                case FabTextEdge.Full:
                    return "F";

                case FabTextEdge.BottomRight:
                    return "BR";

                case FabTextEdge.TopRight:
                    return "TR";

                case FabTextEdge.TopLeft:
                    return "TL";

                case FabTextEdge.BottomLeft:
                    return "BL";

                case FabTextEdge.NoBottom:
                    return "NB";

                case FabTextEdge.NoRight:
                    return "NR";

                case FabTextEdge.NoTop:
                    return "NT";

                case FabTextEdge.NoLeft:
                    return "NL";

                default:
                    return edge.ToString();
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
                    "Unsupported fabrication character: '" +
                    token.Text +
                    "'.");
            }

            List<Curve> sourceCurves =
                GlyphLibrary.CreateCurves(
                    token.Text);

            var curves =
                new List<Curve>(
                    sourceCurves.Count);

            /*
             * glyphs.json is authored at Text Size = 1.
             *
             * Scale the complete glyph uniformly to
             * the requested Text Size.
             */
            Transform scale =
                Transform.Scale(
                    Point3d.Origin,
                    height);

            BoundingBox bounds =
                BoundingBox.Unset;

            foreach (Curve source in sourceCurves)
            {
                if (source == null)
                {
                    continue;
                }

                Curve curve =
                    source.DuplicateCurve();

                curve.Transform(scale);

                curves.Add(curve);

                BoundingBox curveBounds =
                    curve.GetBoundingBox(true);

                if (!curveBounds.IsValid)
                {
                    continue;
                }

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
                    ? bounds.Max.X -
                      bounds.Min.X
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
             * Arrow occupies one square character slot.
             */
            double size =
                height;

            double margin =
                size * 0.08;

            double min =
                -size * 0.5 +
                margin;

            double max =
                size * 0.5 -
                margin;

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
                direction *
                headLength;

            Point3d left =
                headBase +
                perpendicular *
                headWidth;

            Point3d right =
                headBase -
                perpendicular *
                headWidth;

            /*
             * Three independent segments.
             *
             * This prevents the shaft from being traced
             * twice.
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
                    return
                        edge == "bottom" ||
                        edge == "right";

                case FabTextEdge.TopRight:
                    return
                        edge == "top" ||
                        edge == "right";

                case FabTextEdge.TopLeft:
                    return
                        edge == "top" ||
                        edge == "left";

                case FabTextEdge.BottomLeft:
                    return
                        edge == "bottom" ||
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
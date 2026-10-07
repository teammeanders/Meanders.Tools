using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Meanders.Tools.Core.Fabrication
{
    public static class FabTextBuilder
    {
        public static List<Curve> Build(
            string text,
            Plane plane,
            double characterWidth,
            double textHeight,
            double spacing,
            FabTextJustification justification,
            double tolerance)
        {
            if (string.IsNullOrEmpty(text))
                return new List<Curve>();

            if (textHeight <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(textHeight));

            if (tolerance <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));

            List<FabTextToken> tokens =
                FabTextParser.Parse(text);

            var characters =
                new List<FabTextCharacter>(
                    tokens.Count);

            foreach (
                FabTextToken token
                in tokens)
            {
                FabTextCharacter character =
                    FabTextCharacterFactory.Create(
                        token,
                        characterWidth,
                        textHeight,
                        tolerance);

                characters.Add(
                    character);
            }

            FabTextLayout layout =
                FabTextLayoutEngine.Layout(
                    characters,
                    textHeight,
                    spacing,
                    justification);

            var result =
                new List<Curve>();

            foreach (
                FabTextLayoutItem item
                in layout.Items)
            {
                if (item.Character == null ||
                    item.Character.Curves == null)
                {
                    continue;
                }

                foreach (
                    Curve source
                    in item.Character.Curves)
                {
                    if (source == null)
                        continue;

                    Curve curve =
                        source.DuplicateCurve();

                    Transform translation =
                        Transform.Translation(
                            item.X,
                            item.Y,
                            0.0);

                    curve.Transform(
                        translation);

                    result.Add(
                        curve);
                }
            }

            /*
             * The glyph library already contains the
             * fabrication-ready PWK representation.
             *
             * Do NOT simplify, rebuild or convert to
             * polylines here.
             */
            Transform planeTransform =
                Transform.PlaneToPlane(
                    Plane.WorldXY,
                    plane);

            foreach (Curve curve in result)
            {
                curve.Transform(
                    planeTransform);
            }

            return result;
        }
    }
}
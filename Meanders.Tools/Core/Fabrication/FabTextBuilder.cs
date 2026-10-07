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
            {
                return new List<Curve>();
            }

            if (textHeight <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(textHeight));
            }

            if (spacing < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(spacing));
            }

            if (tolerance <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));
            }

            /*
             * 1. Parse the complete fabrication string.
             */
            List<FabTextToken> tokens =
                FabTextParser.Parse(text);

            /*
             * 2. Validate EVERYTHING before creating
             *    any geometry.
             *
             * This guarantees that an unsupported character
             * never results in partially generated geometry.
             */
            for (
                int i = 0;
                i < tokens.Count;
                i++)
            {
                FabTextToken token =
                    tokens[i];

                if (FabTextCharacterFactory.Supports(
                    token))
                {
                    continue;
                }

                string description =
                    FabTextCharacterFactory
                        .GetDescription(token);

                throw new InvalidOperationException(
                    "Unsupported fabrication character " +
                    description +
                    " at text position " +
                    i +
                    ".");
            }

            /*
             * 3. Create one character object for every
             *    normal character / AR / EG token.
             */
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

            /*
             * 4. Layout.
             *
             * Every normal glyph, Arrow and Edge Marker
             * participates as one character slot.
             */
            FabTextLayout layout =
                FabTextLayoutEngine.Layout(
                    characters,
                    textHeight,
                    spacing,
                    justification);

            var result =
                new List<Curve>();

            /*
             * 5. Apply character positions.
             */
            foreach (
                FabTextLayoutItem item
                in layout.Items)
            {
                if (item.Character == null)
                {
                    continue;
                }

                if (item.Character.Curves == null)
                {
                    continue;
                }

                foreach (
                    Curve source
                    in item.Character.Curves)
                {
                    if (source == null)
                    {
                        continue;
                    }

                    Curve curve =
                        source.DuplicateCurve();

                    Transform translation =
                        Transform.Translation(
                            item.X,
                            item.Y,
                            0.0);

                    curve.Transform(
                        translation);

                    result.Add(curve);
                }
            }

            /*
             * 6. Transform the complete local layout
             *    onto the requested Plane.
             *
             * Normal glyphs remain exactly the PWK curves
             * from glyphs.json.
             */
            Transform planeTransform =
                Transform.PlaneToPlane(
                    Plane.WorldXY,
                    plane);

            foreach (Curve curve in result)
            {
                if (curve == null)
                {
                    continue;
                }

                curve.Transform(
                    planeTransform);
            }

            return result;
        }
    }
}
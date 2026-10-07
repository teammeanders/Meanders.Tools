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

            if (characterWidth <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(characterWidth));

            if (textHeight <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(textHeight));

            if (tolerance <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));

            /*
             * 1. Parse the fabrication text.
             */
            List<FabTextToken> tokens =
                FabTextParser.Parse(text);

            /*
             * 2. Create one character object for
             *    every text character / fabrication token.
             */
            var characters =
                new List<FabTextCharacter>(
                    tokens.Count);

            foreach (FabTextToken token in tokens)
            {
                FabTextCharacter character =
                    FabTextCharacterFactory.Create(
                        token,
                        characterWidth,
                        textHeight,
                        tolerance);

                characters.Add(character);
            }

            /*
             * 3. Build the complete character layout.
             *
             * IMPORTANT:
             * The layout is based on the real bounding
             * box of every character/token.
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
             * 4. Place every character according to
             *    the calculated layout.
             */
            foreach (
                FabTextLayoutItem item
                in layout.Items)
            {
                if (item.Character == null)
                    continue;

                if (item.Character.Curves == null)
                    continue;

                foreach (
                    Curve source
                    in item.Character.Curves)
                {
                    if (source == null)
                        continue;

                    Curve curve =
                        source.DuplicateCurve();

                    /*
                     * Move the character from its local
                     * origin into its layout position.
                     */
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


            result =
                FabTextGeometrySimplifier.Simplify(
                    result,
                    tolerance);


            /*
             * 6. Finally transform the complete local
             * fabrication layout onto the requested Plane.
             */
            Transform planeTransform =
                Transform.PlaneToPlane(
                    Plane.WorldXY,
                    plane);

            foreach (Curve curve in result)
            {
                if (curve == null)
                    continue;

                curve.Transform(
                    planeTransform);
            }

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Meanders.Tools.Core.Fabrication
{
    public enum FabTextJustification
    {
        BottomLeft = 0,
        BottomCenter = 1,
        BottomRight = 2,

        MiddleLeft = 3,
        MiddleCenter = 4,
        MiddleRight = 5,

        TopLeft = 6,
        TopCenter = 7,
        TopRight = 8
    }

    public sealed class FabTextLayoutItem
    {
        public FabTextCharacter Character { get; }

        public BoundingBox Bounds { get; }

        public double X { get; }

        public double Y { get; }

        public double Width { get; }

        public double Height { get; }

        public FabTextLayoutItem(
            FabTextCharacter character,
            BoundingBox bounds,
            double x,
            double y)
        {
            Character =
                character
                ?? throw new ArgumentNullException(
                    nameof(character));

            Bounds = bounds;

            X = x;
            Y = y;

            Width = bounds.IsValid
                ? bounds.Max.X - bounds.Min.X
                : character.Width;

            Height = bounds.IsValid
                ? bounds.Max.Y - bounds.Min.Y
                : character.Height;
        }
    }

    public sealed class FabTextLayout
    {
        public IReadOnlyList<FabTextLayoutItem> Items { get; }

        public BoundingBox Bounds { get; }

        public double Width =>
            Bounds.IsValid
                ? Bounds.Max.X - Bounds.Min.X
                : 0.0;

        public double Height =>
            Bounds.IsValid
                ? Bounds.Max.Y - Bounds.Min.Y
                : 0.0;

        public FabTextLayout(
            IReadOnlyList<FabTextLayoutItem> items,
            BoundingBox bounds)
        {
            Items = items;
            Bounds = bounds;
        }
    }

    public static class FabTextLayoutEngine
    {
        private const double DefaultSpacingRatio = 0.05;

        public static FabTextLayout Layout(
            IReadOnlyList<FabTextCharacter> characters,
            double textHeight,
            double spacing,
            FabTextJustification justification)
        {
            if (characters == null)
                throw new ArgumentNullException(
                    nameof(characters));

            if (textHeight <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(textHeight));

            if (characters.Count == 0)
            {
                return new FabTextLayout(
                    Array.Empty<FabTextLayoutItem>(),
                    BoundingBox.Unset);
            }

            /*
             * User spacing is an additional gap between
             * character bounding boxes.
             *
             * It can never become smaller than the
             * minimum default gap.
             */
            double defaultSpacing =
                textHeight *
                DefaultSpacingRatio;

            double effectiveSpacing =
                Math.Max(
                    spacing,
                    defaultSpacing);

            var prepared =
                new List<PreparedCharacter>(
                    characters.Count);

            foreach (
                FabTextCharacter character
                in characters)
            {
                BoundingBox bounds =
                    GetCharacterBounds(
                        character);

                if (!bounds.IsValid)
                    continue;

                prepared.Add(
                    new PreparedCharacter(
                        character,
                        bounds));
            }

            if (prepared.Count == 0)
            {
                return new FabTextLayout(
                    Array.Empty<FabTextLayoutItem>(),
                    BoundingBox.Unset);
            }

            /*
             * Every character is centered on the same
             * horizontal layout line.
             *
             * The character's own vertical center is
             * used so that normal text, arrows and edge
             * markers share the same center line.
             */
            double centerY = 0.0;

            foreach (
                PreparedCharacter character
                in prepared)
            {
                centerY +=
                    character.Bounds.Center.Y;
            }

            centerY /=
                prepared.Count;

            /*
             * First calculate the un-justified sequence.
             */
            double cursorX = 0.0;

            var positioned =
                new List<PositionedCharacter>(
                    prepared.Count);

            foreach (
                PreparedCharacter character
                in prepared)
            {
                BoundingBox bounds =
                    character.Bounds;

                double width =
                    bounds.Max.X -
                    bounds.Min.X;

                double localCenterY =
                    bounds.Center.Y;

                double yOffset =
                    centerY -
                    localCenterY;

                double xOffset =
                    cursorX -
                    bounds.Min.X;

                positioned.Add(
                    new PositionedCharacter(
                        character.Character,
                        bounds,
                        xOffset,
                        yOffset));

                cursorX +=
                    width +
                    effectiveSpacing;
            }

            /*
             * Remove the final gap.
             */
            double totalWidth =
                cursorX -
                effectiveSpacing;

            /*
             * Calculate total bounds before
             * justification.
             */
            BoundingBox totalBounds =
                BoundingBox.Unset;

            foreach (
                PositionedCharacter character
                in positioned)
            {
                BoundingBox translated =
                    TranslateBounds(
                        character.Bounds,
                        character.X,
                        character.Y);

                totalBounds.Union(
                    translated);
            }

            /*
             * Apply justification to the complete
             * text bounds, not individual characters.
             */
            GetJustificationOffset(
                justification,
                totalBounds,
                out double justificationX,
                out double justificationY);

            var items =
                new List<FabTextLayoutItem>(
                    positioned.Count);

            BoundingBox finalBounds =
                BoundingBox.Unset;

            foreach (
                PositionedCharacter character
                in positioned)
            {
                double finalX =
                    character.X +
                    justificationX;

                double finalY =
                    character.Y +
                    justificationY;

                BoundingBox finalCharacterBounds =
                    TranslateBounds(
                        character.Bounds,
                        finalX,
                        finalY);

                finalBounds.Union(
                    finalCharacterBounds);

                items.Add(
                    new FabTextLayoutItem(
                        character.Character,
                        finalCharacterBounds,
                        finalX,
                        finalY));
            }

            return new FabTextLayout(
                items,
                finalBounds);
        }

        private static BoundingBox GetCharacterBounds(
            FabTextCharacter character)
        {
            BoundingBox bounds =
                BoundingBox.Unset;

            if (character.Curves != null)
            {
                foreach (
                    Curve curve
                    in character.Curves)
                {
                    if (curve == null)
                        continue;

                    BoundingBox curveBounds =
                        curve.GetBoundingBox(true);

                    if (!curveBounds.IsValid)
                        continue;

                    bounds.Union(
                        curveBounds);
                }
            }

            /*
             * Special characters should always have
             * a usable slot even if their geometry
             * happens to be empty.
             */
            if (!bounds.IsValid)
            {
                double width =
                    Math.Max(
                        character.Width,
                        0.001);

                double height =
                    Math.Max(
                        character.Height,
                        0.001);

                bounds =
                    new BoundingBox(
                        new Point3d(
                            -width * 0.5,
                            -height * 0.5,
                            0.0),
                        new Point3d(
                            width * 0.5,
                            height * 0.5,
                            0.0));
            }

            return bounds;
        }

        private static BoundingBox TranslateBounds(
            BoundingBox bounds,
            double x,
            double y)
        {
            BoundingBox result =
                bounds;

            result.Transform(
                Transform.Translation(
                    x,
                    y,
                    0.0));

            return result;
        }

        private static void GetJustificationOffset(
            FabTextJustification justification,
            BoundingBox bounds,
            out double x,
            out double y)
        {
            x = 0.0;
            y = 0.0;

            if (!bounds.IsValid)
                return;

            double centerX =
                bounds.Center.X;

            double centerY =
                bounds.Center.Y;

            switch (justification)
            {
                case FabTextJustification.BottomLeft:
                    x = -bounds.Min.X;
                    y = -bounds.Min.Y;
                    break;

                case FabTextJustification.BottomCenter:
                    x = -centerX;
                    y = -bounds.Min.Y;
                    break;

                case FabTextJustification.BottomRight:
                    x = -bounds.Max.X;
                    y = -bounds.Min.Y;
                    break;

                case FabTextJustification.MiddleLeft:
                    x = -bounds.Min.X;
                    y = -centerY;
                    break;

                case FabTextJustification.MiddleCenter:
                    x = -centerX;
                    y = -centerY;
                    break;

                case FabTextJustification.MiddleRight:
                    x = -bounds.Max.X;
                    y = -centerY;
                    break;

                case FabTextJustification.TopLeft:
                    x = -bounds.Min.X;
                    y = -bounds.Max.Y;
                    break;

                case FabTextJustification.TopCenter:
                    x = -centerX;
                    y = -bounds.Max.Y;
                    break;

                case FabTextJustification.TopRight:
                    x = -bounds.Max.X;
                    y = -bounds.Max.Y;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(justification));
            }
        }

        private sealed class PreparedCharacter
        {
            public FabTextCharacter Character { get; }

            public BoundingBox Bounds { get; }

            public PreparedCharacter(
                FabTextCharacter character,
                BoundingBox bounds)
            {
                Character = character;
                Bounds = bounds;
            }
        }

        private sealed class PositionedCharacter
        {
            public FabTextCharacter Character { get; }

            public BoundingBox Bounds { get; }

            public double X { get; }

            public double Y { get; }

            public PositionedCharacter(
                FabTextCharacter character,
                BoundingBox bounds,
                double x,
                double y)
            {
                Character = character;
                Bounds = bounds;
                X = x;
                Y = y;
            }
        }
    }
}
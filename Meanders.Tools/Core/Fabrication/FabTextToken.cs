using System;

namespace Meanders.Tools.Core.Fabrication
{
    public enum FabTextTokenType
    {
        Text,
        Arrow,
        EdgeMarker
    }

    public enum FabTextEdge
    {
        Full,

        BottomRight,
        TopRight,
        TopLeft,
        BottomLeft,

        NoBottom,
        NoRight,
        NoTop,
        NoLeft
    }

    public sealed class FabTextToken
    {
        public FabTextTokenType Type { get; }

        /*
         * For normal text characters.
         */
        public string Text { get; }

        /*
         * For Arrow tokens.
         *
         * Angle is expressed in degrees
         * relative to the local X axis of
         * the fabrication text Plane.
         */
        public double Angle { get; }

        /*
         * For EdgeMarker tokens.
         */
        public FabTextEdge Edge { get; }

        private FabTextToken(
            FabTextTokenType type,
            string text = null,
            double angle = 0.0,
            FabTextEdge edge = FabTextEdge.Full)
        {
            Type = type;
            Text = text;
            Angle = angle;
            Edge = edge;
        }

        public static FabTextToken TextCharacter(
            string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException(
                    "Text token cannot be empty.",
                    nameof(text));

            return new FabTextToken(
                FabTextTokenType.Text,
                text: text);
        }

        public static FabTextToken Arrow(
            double angle)
        {
            return new FabTextToken(
                FabTextTokenType.Arrow,
                angle: angle);
        }

        public static FabTextToken EdgeMarker(
            FabTextEdge edge)
        {
            return new FabTextToken(
                FabTextTokenType.EdgeMarker,
                edge: edge);
        }

        public override string ToString()
        {
            switch (Type)
            {
                case FabTextTokenType.Text:
                    return $"Text({Text})";

                case FabTextTokenType.Arrow:
                    return $"Arrow({Angle}°)";

                case FabTextTokenType.EdgeMarker:
                    return $"EdgeMarker({Edge})";

                default:
                    return Type.ToString();
            }
        }
    }
}
using System;
using Rhino.Geometry;

namespace Meanders.Tools.Core.Fabrication
{
    public sealed class FabTextCharacter
    {
        public FabTextToken Token { get; }

        public double Width { get; }

        public double Height { get; }

        public Curve[] Curves { get; }

        public FabTextCharacter(
            FabTextToken token,
            double width,
            double height,
            Curve[] curves)
        {
            Token = token
                ?? throw new ArgumentNullException(
                    nameof(token));

            Width = width;
            Height = height;

            Curves =
                curves ?? Array.Empty<Curve>();
        }
    }
}
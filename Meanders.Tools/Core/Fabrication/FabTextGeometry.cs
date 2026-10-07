using System;
using Rhino.Geometry;

namespace Meanders.Tools.Core.Fabrication
{
    public static class FabTextGeometry
    {
        /*
         * Rhino 7 single-stroke / engraving font.
         *
         * This is intentionally kept in one place so the
         * fabrication font can be changed later without
         * touching the parser or layout system.
         */
        public const string FontName =
            "SLF-RHN Architect";

        public static Curve[] CreateTextOutlines(
            string text,
            Plane plane,
            double height,
            double tolerance)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<Curve>();

            if (height <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(height));

            if (tolerance <= 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(tolerance));

            /*
             * IMPORTANT:
             *
             * We deliberately do not use Bold here.
             * The fabrication output is based on the
             * single-stroke font geometry.
             */
            Curve[] curves =
                Curve.CreateTextOutlines(
                    text,
                    FontName,
                    height,
                    0,
                    true,
                    plane,
                    0.0,
                    tolerance);

            return curves ??
                   Array.Empty<Curve>();
        }
    }
}
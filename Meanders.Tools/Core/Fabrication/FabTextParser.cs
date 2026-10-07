using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Meanders.Tools.Core.Fabrication
{
    public static class FabTextParser
    {
        private static readonly Regex ArrowRegex =
            new Regex(
                @"^<AR-(?<angle>[+-]?(?:\d+(?:\.\d*)?|\.\d+))>$",
                RegexOptions.Compiled |
                RegexOptions.CultureInvariant |
                RegexOptions.IgnoreCase);

        private static readonly Regex EdgeRegex =
            new Regex(
                @"^<EG-(?<flag>F|BR|TR|TL|BL|NB|NR|NT|NL)>$",
                RegexOptions.Compiled |
                RegexOptions.CultureInvariant |
                RegexOptions.IgnoreCase);

        public static List<FabTextToken> Parse(
            string text)
        {
            var tokens =
                new List<FabTextToken>();

            if (string.IsNullOrEmpty(text))
                return tokens;

            int index = 0;

            while (index < text.Length)
            {
                if (text[index] == '<')
                {
                    int closeIndex =
                        text.IndexOf(
                            '>',
                            index);

                    if (closeIndex >= 0)
                    {
                        string candidate =
                            text.Substring(
                                index,
                                closeIndex - index + 1);

                        if (TryParseToken(
                            candidate,
                            out FabTextToken token))
                        {
                            tokens.Add(token);

                            index =
                                closeIndex + 1;

                            continue;
                        }

                        /*
                         * Unknown <...> content is treated
                         * as normal text.
                         *
                         * This prevents user text from being
                         * accidentally destroyed.
                         */
                    }
                }

                /*
                 * Normal text is represented character-by-character
                 * because every character participates in the same
                 * fabrication layout.
                 */
                tokens.Add(
                    FabTextToken.TextCharacter(
                        text[index].ToString()));

                index++;
            }

            return tokens;
        }

        private static bool TryParseToken(
            string value,
            out FabTextToken token)
        {
            token = null;

            Match arrow =
                ArrowRegex.Match(value);

            if (arrow.Success)
            {
                string angleText =
                    arrow.Groups["angle"].Value;

                if (!double.TryParse(
                    angleText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double angle))
                {
                    return false;
                }

                token =
                    FabTextToken.Arrow(
                        NormalizeAngle(angle));

                return true;
            }

            Match edge =
                EdgeRegex.Match(value);

            if (edge.Success)
            {
                string flag =
                    edge.Groups["flag"]
                        .Value
                        .ToUpperInvariant();

                if (!TryParseEdge(
                    flag,
                    out FabTextEdge edgeType))
                {
                    return false;
                }

                token =
                    FabTextToken.EdgeMarker(
                        edgeType);

                return true;
            }

            return false;
        }

        private static bool TryParseEdge(
            string flag,
            out FabTextEdge edge)
        {
            switch (flag)
            {
                case "F":
                    edge = FabTextEdge.Full;
                    return true;

                case "BR":
                    edge = FabTextEdge.BottomRight;
                    return true;

                case "TR":
                    edge = FabTextEdge.TopRight;
                    return true;

                case "TL":
                    edge = FabTextEdge.TopLeft;
                    return true;

                case "BL":
                    edge = FabTextEdge.BottomLeft;
                    return true;

                case "NB":
                    edge = FabTextEdge.NoBottom;
                    return true;

                case "NR":
                    edge = FabTextEdge.NoRight;
                    return true;

                case "NT":
                    edge = FabTextEdge.NoTop;
                    return true;

                case "NL":
                    edge = FabTextEdge.NoLeft;
                    return true;

                default:
                    edge = FabTextEdge.Full;
                    return false;
            }
        }

        private static double NormalizeAngle(
            double angle)
        {
            double normalized =
                angle % 360.0;

            if (normalized < 0.0)
                normalized += 360.0;

            return normalized;
        }
    }
}
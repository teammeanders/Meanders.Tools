using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using Rhino.Geometry;

namespace Meanders.Tools.Core.Fabrication
{
    public sealed class FabTextGlyphLibrary
    {
        private const string ResourceName =
            "Meanders.Tools.Resources.FabText.glyphs.json";

        private readonly Dictionary<
            string,
            FabTextGlyphDefinition> _glyphs;

        private FabTextGlyphLibrary(
            Dictionary<string, FabTextGlyphDefinition> glyphs)
        {
            _glyphs = glyphs;
        }

        public static FabTextGlyphLibrary Load()
        {
            Assembly assembly =
                typeof(FabTextGlyphLibrary).Assembly;

            Stream stream =
                assembly.GetManifestResourceStream(
                    ResourceName);

            if (stream == null)
            {
                throw new InvalidOperationException(
                    "Embedded resource was not found: " +
                    ResourceName);
            }

            string json;

            using (StreamReader reader =
                   new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            var serializer =
                new JavaScriptSerializer();

            FabTextGlyphFile file =
                serializer.Deserialize<FabTextGlyphFile>(
                    json);

            if (file == null ||
                file.Glyphs == null)
            {
                throw new InvalidOperationException(
                    "FabText glyph library is empty or invalid.");
            }

            return new FabTextGlyphLibrary(
                file.Glyphs);
        }

        public bool Contains(
            string character)
        {
            return
                !string.IsNullOrEmpty(character) &&
                _glyphs.ContainsKey(character);
        }

        public List<Curve> CreateCurves(
            string character)
        {
            if (!Contains(character))
            {
                throw new KeyNotFoundException(
                    "FabText glyph was not found: " +
                    character);
            }

            FabTextGlyphDefinition glyph =
                _glyphs[character];

            var curves =
                new List<Curve>();

            if (glyph.Curves == null)
                return curves;

            foreach (
                FabTextGlyphCurveDefinition definition
                in glyph.Curves)
            {
                curves.Add(
                    CreateNurbsCurve(
                        definition,
                        character));
            }

            return curves;
        }

        private static NurbsCurve CreateNurbsCurve(
            FabTextGlyphCurveDefinition definition,
            string character)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (definition.Points == null ||
                definition.Points.Count == 0)
            {
                throw new InvalidOperationException(
                    "FabText glyph curve has no control points. " +
                    "Character: " +
                    character);
            }

            if (definition.Weights == null ||
                definition.Weights.Count !=
                definition.Points.Count)
            {
                throw new InvalidOperationException(
                    "FabText glyph curve has an invalid " +
                    "weight count. Character: " +
                    character);
            }

            if (definition.Knots == null)
            {
                throw new InvalidOperationException(
                    "FabText glyph curve has no knots. " +
                    "Character: " +
                    character);
            }

            int degree =
                definition.Degree;

            if (degree < 1)
            {
                throw new InvalidOperationException(
                    "FabText glyph curve has an invalid degree. " +
                    "Character: " +
                    character);
            }

            int pointCount =
                definition.Points.Count;

            int order =
                degree + 1;

            if (pointCount < order)
            {
                throw new InvalidOperationException(
                    "FabText glyph has fewer control points " +
                    "than its NURBS order. Character: " +
                    character);
            }

            int expectedKnotCount =
                pointCount + degree - 1;

            if (definition.Knots.Count !=
                expectedKnotCount)
            {
                throw new InvalidOperationException(
                    "Invalid knot count. " +
                    "Character: " +
                    character +
                    ", expected: " +
                    expectedKnotCount +
                    ", actual: " +
                    definition.Knots.Count);
            }

            /*
             * RhinoCommon constructor:
             *
             * dimension
             * rational
             * order
             * pointCount
             *
             * The exported BB Text glyphs contain weights,
             * so these curves are reconstructed as rational
             * NURBS.
             */
            var curve =
                new NurbsCurve(
                    3,
                    true,
                    order,
                    pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                FabTextPoint point =
                    definition.Points[i];

                double weight =
                    definition.Weights[i];

                curve.Points.SetPoint(
                    i,
                    new Point3d(
                        point.X,
                        point.Y,
                        point.Z),
                    weight);
            }

            for (
                int i = 0;
                i < definition.Knots.Count;
                i++)
            {
                curve.Knots[i] =
                    definition.Knots[i];
            }

            if (!curve.IsValid)
            {
                throw new InvalidOperationException(
                    "Generated FabText NURBS curve is invalid. " +
                    "Character: " +
                    character);
            }

            return curve;
        }
    }

    public sealed class FabTextGlyphFile
    {
        public string Schema { get; set; }

        public int Version { get; set; }

        public string Source { get; set; }

        public int CharacterCount { get; set; }

        public List<string> Characters { get; set; }

        public Dictionary<
            string,
            FabTextGlyphDefinition> Glyphs
        { get; set; }
    }

    public sealed class FabTextGlyphDefinition
    {
        public string Character { get; set; }

        public int CurveCount { get; set; }

        public List<
            FabTextGlyphCurveDefinition> Curves
        { get; set; }
    }

    public sealed class FabTextGlyphCurveDefinition
    {
        public int Degree { get; set; }

        public List<FabTextPoint> Points { get; set; }

        public List<double> Weights { get; set; }

        public List<double> Knots { get; set; }
    }

    public sealed class FabTextPoint
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
    }
}
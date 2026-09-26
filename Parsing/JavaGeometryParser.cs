using CeacModelConverter.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CeacModelConverter.Parsing
{
    internal sealed class JavaGeometryParser
    {
        private static readonly Regex FaceStartRegex = new(
            @"ModelHelper\.(quad|triangle)\s*\(",
            RegexOptions.Compiled
        );

        private static readonly Regex VertexStartRegex = new(
            @"ModelHelper\.vertex\s*\(",
            RegexOptions.Compiled
        );

        private static readonly Regex ClassRegex = new(
            @"\bclass\s+([A-Za-z_$][A-Za-z0-9_$]*)",
            RegexOptions.Compiled
        );



        public GeometryModel Parse(
            string source,
            string fallbackName
        )
        {
            string name = FindClassName(source) ?? fallbackName;
            var faces = new List<GeometryFace>();
            MatchCollection faceMatches = FaceStartRegex.Matches(source);

            foreach (Match faceMatch in faceMatches)
            {
                string faceTypeName = faceMatch.Groups[1].Value;
                int openingParenthesis = source.IndexOf('(', faceMatch.Index + faceMatch.Length - 1);
                int closingParenthesis = FindMatchingParenthesis(source, openingParenthesis);
                string faceBody = source.Substring(
                    openingParenthesis + 1,
                    closingParenthesis - openingParenthesis - 1
                );

                List<GeometryVertex> vertices = ParseVertices(faceBody);
                FaceType faceType = faceTypeName.Equals("quad", StringComparison.Ordinal)
                    ? FaceType.Quad
                    : FaceType.Triangle;
                int expectedVertexCount = faceType == FaceType.Quad ? 4 : 3;

                if (vertices.Count != expectedVertexCount)
                    throw new FormatException(
                        $"A {faceTypeName} contains " +
                        $"{vertices.Count} ModelHelper.vertex calls; " +
                        $"expected {expectedVertexCount}."
                    );

                string? comment = FindFaceComment(source, faceMatch.Index);

                faces.Add(new GeometryFace(faceType, vertices, comment));
            }

            if (faces.Count == 0)
                throw new FormatException("No ModelHelper.quad(...) or ModelHelper.triangle(...) calls were found.");

            return new GeometryModel(name, faces);
        }

        private static List<GeometryVertex> ParseVertices(string faceBody)
        {
            var vertices = new List<GeometryVertex>();
            MatchCollection matches = VertexStartRegex.Matches(faceBody);

            foreach (Match match in matches)
            {
                int openingParenthesis = faceBody.IndexOf('(', match.Index + match.Length - 1);
                int closingParenthesis = FindMatchingParenthesis(faceBody, openingParenthesis);

                string arguments = faceBody.Substring(
                    openingParenthesis + 1,
                    closingParenthesis - openingParenthesis - 1
                );

                vertices.Add(ParseVertex(arguments));
            }

            return vertices;
        }

        private static GeometryVertex ParseVertex(string arguments)
        {
            string[] parts = arguments.Split(',');
            if (parts.Length != 5)
                throw new FormatException("ModelHelper.vertex(...) must contain exactly fine arguments.");

            return new GeometryVertex(
                ParseNumber(parts[0], "X"),
                ParseNumber(parts[1], "Y"),
                ParseNumber(parts[2], "Z"),
                ParseNumber(parts[3], "U"),
                ParseNumber(parts[4], "V")
            );
        }

        private static double ParseNumber(
            string text,
            string name
        )
        {
            string value = text.Trim();

            if (
                value.EndsWith("f", StringComparison.OrdinalIgnoreCase) ||
                value.EndsWith("d", StringComparison.OrdinalIgnoreCase)
            )
                value = value.Substring(0, value.Length - 1);

            if (!double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double result
            ))
                throw new FormatException($"Could not parse {name} value '{text}'.");

            return result;
        }

        private string? FindClassName(string source)
        {
            Match match = ClassRegex.Match(source);
            return match.Success
                ? match.Groups[1].Value
                : null;
        }

        private static string? FindFaceComment(
            string source,
            int faceIndex
        )
        {
            int builderIndex = source.LastIndexOf(
                "builder.addUnculledFace",
                faceIndex,
                StringComparison.Ordinal
            );
            if (builderIndex < 0)
                return null;

            int lineStart = source.LastIndexOf('\n', builderIndex);
            if (lineStart < 0)
                lineStart = 0;
            else
                lineStart++;

            string beforeBuilder = source.Substring(0, lineStart);
            string[] lines = beforeBuilder.Split('\n');

            for (int i = lines.Length - 1; i >= 0; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (line.StartsWith("//"))
                    return line.Substring(2).Trim();
                break;
            }

            return null;
        }

        private static int FindMatchingParenthesis(
            string text,
            int openingIndex
        )
        {
            int depth = 0;

            for (int i = openingIndex; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '(':
                        depth++;
                        break;
                    case ')':
                        depth--;
                        if (depth == 0)
                            return i;
                        break;
                }
            }

            throw new FormatException("Unterminated parenthesis expression.");
        }
    }
}

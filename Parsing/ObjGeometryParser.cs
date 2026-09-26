using CeacModelConverter.Model;
using System.Globalization;

namespace CeacModelConverter.Parsing
{
    internal sealed class ObjGeometryParser
    {
        public GeometryModel Parse(
            string source,
            string fallbackName
        )
        {
            var positions = new List<(double X, double Y, double Z)>();
            var uvs = new List<(double U, double V)>();
            var faces = new List<GeometryFace>();

            string name = fallbackName;
            string? pendingComment = null;

            string[] lines = source.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (line.StartsWith("#"))
                {
                    pendingComment = line.Substring(1).Trim();
                    continue;
                }

                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;

                switch (parts[0])
                {
                    case "o":
                        if (parts.Length > 1)
                            name = string.Join(" ", parts.Skip(1));
                        pendingComment = null;
                        break;

                    case "v":
                        if (parts.Length < 4)
                            throw new FormatException($"Invalid OBJ vertex: '{line}'.");
                        positions.Add((
                            ParseNumber(parts[1], "X"),
                            ParseNumber(parts[2], "Y"),
                            ParseNumber(parts[3], "Z")
                        ));
                        pendingComment = null;
                        break;

                    case "vt":
                        if (parts.Length < 3)
                            throw new FormatException($"Invalid OBJ texture coordinate: '{line}'.");
                        uvs.Add((
                            ParseNumber(parts[1], "U"),
                            ParseNumber(parts[2], "V")
                        ));
                        pendingComment = null;
                        break;

                    case "f":
                        if (parts.Length < 4)
                            throw new FormatException($"OBJ face has fewer than three vertices: '{line}'.");
                        List<GeometryVertex> vertices = ParseFace(parts, positions, uvs);
                        FaceType faceType = vertices.Count == 3 ? FaceType.Triangle : FaceType.Quad;
                        if (vertices.Count > 4)
                            throw new FormatException($"OBJ n-gons are not supported: '{line}'.");
                        faces.Add(new GeometryFace(faceType, vertices, pendingComment));
                        pendingComment = null;
                        break;

                    default:
                        pendingComment = null;
                        break;
                }
            }

            if (faces.Count == 0)
                throw new FormatException($"No OBJ faces were found.");

            return new GeometryModel(name, faces);
        }

        private static List<GeometryVertex> ParseFace(
            string[] parts,
            List<(double X, double Y, double Z)> positions,
            List<(double U, double V)> uvs
        )
        {
            var vertices = new List<GeometryVertex>();

            for (int i = 1; i < parts.Length; i++)
            {
                string[] indices = parts[i].Split('/');
                if (indices.Length < 1 || string.IsNullOrWhiteSpace(indices[0]))
                    throw new FormatException($"Invalid OBJ face vertex '{parts[i]}'.");

                int positionIndex = ParseIndex(indices[0], positions.Count);

                double u = 0.0;
                double v = 0.0;

                if (indices.Length > 1 && !string.IsNullOrWhiteSpace(indices[1]))
                {
                    int uvIndex = ParseIndex(indices[1], uvs.Count);
                    (u, v) = uvs[uvIndex];
                }

                var position = positions[positionIndex];

                vertices.Add(new GeometryVertex(
                    position.X,
                    position.Y,
                    position.Z,
                    u,
                    v
                ));
            }

            return vertices;
        }

        private static int ParseIndex(
            string text,
            int count
        )
        {
            if (!int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int index
            ))
                throw new FormatException($"Invalid OBJ index '{text}'.");
            if (index == 0)
                throw new FormatException($"OBJ indeces cannot be zero.");

            int zeroBased = index > 0
                ? index - 1
                : count + index;

            if (zeroBased < 0 || zeroBased >= count)
                throw new FormatException($"OBJ index '{text}' is outside the available range.");

            return zeroBased;
        }

        private static double ParseNumber(
            string text,
            string name
        )
        {
            if (!double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double result
            ))
                throw new FormatException($"Could not parse {name} value '{text}'.");

            return result;
        }
    }
}

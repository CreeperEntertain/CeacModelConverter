using CeacModelConverter.Coordinates;
using CeacModelConverter.Model;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CeacModelConverter.WavefrontObj
{
    internal sealed class ObjWriter
    {
        public void Write(
            GeometryModel geometry,
            string outputPath
        )
        {
            var positions = new List<Vector3>();
            var uvs = new List<(double u, double v)>();
            var indeces = new Dictionary<ObjVertexKey, int>();
            var faces = new List<ObjFace>();

            foreach (GeometryFace face in geometry.Faces)
            {
                var faceIndeces = new List<int>();

                foreach (GeometryVertex vertex in face.Vertices)
                {
                    var key = new ObjVertexKey(
                        vertex.X,
                        vertex.Y,
                        vertex.Z,
                        vertex.U,
                        vertex.V
                    );

                    if (!indeces.TryGetValue(key, out int index))
                    {
                        Vector3 position = CoordinateConverter.JavaToObj(vertex);

                        positions.Add(position);
                        uvs.Add((vertex.U, vertex.V));

                        index = positions.Count;

                        indeces.Add(key, index);
                    }

                    faceIndeces.Add(index);
                }

                faces.Add(new ObjFace(face.Type, faceIndeces, face.Comment));
            }

            string contents = BuildObj(geometry.Name, positions, uvs, faces);

            File.WriteAllText(outputPath, contents, new UTF8Encoding(false));
        }

        private static string BuildObj(
            string name,
            IReadOnlyList<Vector3> positions,
            IReadOnlyList<(double u, double v)> uvs,
            IReadOnlyList<ObjFace> faces
        )
        {
            var output = new StringBuilder();

            output.AppendLine("# Generated from CEAC Java geometry.");
            output.AppendLine($"# Geometry: {name}");
            output.AppendLine($"o {name}");
            output.AppendLine();

            foreach (Vector3 position in positions)
                output.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "v {0} {1} {2}",
                    position.X,
                    position.Y,
                    position.Z
                ));

            output.AppendLine();

            foreach ((double u, double v) in uvs)
                output.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "vt {0} {1}",
                    u,
                    v
                ));

            output.AppendLine();

            foreach (ObjFace face in faces)
            {
                if (!string.IsNullOrWhiteSpace(face.Comment))
                    output.AppendLine($"# {face.Comment}");
                var faceParts = new List<string>();
                foreach (int index in face.Indeces)
                    faceParts.Add($"{index}/{index}");
                output.AppendLine($"f {string.Join(" ", faceParts)}");
                output.AppendLine();
            }

            return output.ToString();
        }

        private readonly struct ObjVertexKey : IEquatable<ObjVertexKey>
        {
            private readonly double x;
            private readonly double y;
            private readonly double z;
            private readonly double u;
            private readonly double v;

            public ObjVertexKey(
                double x,
                double y,
                double z,
                double u,
                double v
            )
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.u = u;
                this.v = v;
            }

            public bool Equals(ObjVertexKey other)
                =>  x == other.x &&
                    y == other.y &&
                    z == other.z &&
                    u == other.u &&
                    v == other.v;

            public override bool Equals(object? obj)
                => obj is ObjVertexKey other && Equals(other);

            public override int GetHashCode()
                => HashCode.Combine(x, y, z, u, v);
        }

        private sealed class ObjFace
        {
            public FaceType Type { get; }
            public IReadOnlyList<int> Indeces { get; }
            public string? Comment { get; }

            public ObjFace(
                FaceType type,
                IReadOnlyList<int> indeces,
                string? comment
            )
            {
                Type = type;
                Indeces = indeces;
                Comment = comment;
            }
        }
    }
}

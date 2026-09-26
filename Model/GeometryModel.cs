namespace CeacModelConverter.Model
{
    internal sealed class GeometryModel
    {
        public string Name { get; }
        public IReadOnlyList<GeometryFace> Faces { get; }

        public GeometryModel(
            string name,
            IReadOnlyList<GeometryFace> faces
        )
        {
            Name = name;
            Faces = faces;
        }
    }

    internal sealed class GeometryFace
    {
        public FaceType Type { get; }
        public IReadOnlyList<GeometryVertex> Vertices { get; }
        public string? Comment { get; }

        public GeometryFace(
            FaceType type,
            IReadOnlyList<GeometryVertex> vertices,
            string? comment
        )
        {
            Type = type;
            Vertices = vertices;
            Comment = comment;
        }
    }

    internal enum FaceType
    {
        Triangle,
        Quad
    }

    internal readonly struct GeometryVertex
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public double U { get; }
        public double V { get; }

        public GeometryVertex(
            double x,
            double y,
            double z,
            double u,
            double v
        )
        {
            X = x;
            Y = y;
            Z = z;
            U = u;
            V = v;
        }
    }
}

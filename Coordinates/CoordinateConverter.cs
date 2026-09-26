using CeacModelConverter.Model;
using System.Numerics;

namespace CeacModelConverter.Coordinates
{
    internal static class CoordinateConverter
    {
        public static Vector3 JavaToObj(GeometryVertex vertex)
        {
            return new Vector3(
                (float)(vertex.X - 0.5),
                (float)(-(vertex.Z - 0.5)),
                (float)(vertex.Y - 0.5)
            );
        }
    }
}

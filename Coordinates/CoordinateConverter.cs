using CeacModelConverter.Model;
using System.Numerics;

namespace CeacModelConverter.Coordinates
{
    internal static class CoordinateConverter
    {
        public static Vector3 JavaToObj(GeometryVertex vertex)
        {
            return new Vector3(
                (float)(vertex.X),
                (float)(vertex.Y),
                (float)(vertex.Z)
            );
        }
    }
}

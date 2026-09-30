using System.Numerics;
using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CollisionFactories
{
    public class BoundingRenderEntity : CollisionDefinition
    {
        public DetectionType DetectionType { get; set; }
        public ResponseType ResponseType { get; set; }
        public bool UserDefinedSphereSize { get; set; }
        public Vector3 LocalPosition { get; set; }
        public float Radius { get; set; }
        public bool UserDefinedBoundingBoxExtents { get; set; }
        public Vector3 MinExtents { get; set; }
        public Vector3 MaxExtents { get; set; }
        public BoundingRenderEntity(string type) : base(type)
        {
        }
    }
}

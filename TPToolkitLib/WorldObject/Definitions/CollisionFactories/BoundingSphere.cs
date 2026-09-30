using System.Numerics;
using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CollisionFactories
{
    public class BoundingSphere : CollisionDefinition
    {
        public DetectionType DetectionType { get; set; }
        public ResponseType ResponseType { get; set; }
        public bool UserDefinedSphereSize { get; set; }
        public Vector3 LocalPosition { get; set; }
        public float Radius { get; set; }
        public BoundingSphere(string type) : base(type)
        {
        }
    }
}

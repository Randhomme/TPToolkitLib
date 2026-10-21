using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CollisionFactories
{
    public class Point : CollisionDefinition
    {
        public DetectionType DetectionType { get; set; }
        public ResponseType ResponseType { get; set; }
        public Point(string type) : base(type)
        {
        }
    }
}

using System.Numerics;

namespace TPToolkitLib.WorldObject.Definitions.PhysicsFactories
{
    public class SpaceObjectPhysics : PhysicsDefinition
    {
        public Vector3 CenterOfMass { get; set; }
        public float Mass { get; set; }
        public float Acceleration { get; set; }
        public SpaceObjectPhysics(string type) : base(type)
        {
            
        }
    }
}

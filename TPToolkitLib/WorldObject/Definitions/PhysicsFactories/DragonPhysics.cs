using System.Numerics;

namespace TPToolkitLib.WorldObject.Definitions.PhysicsFactories
{
    public class DragonPhysics : PhysicsDefinition
    {
        public Vector3 CenterOfMass { get; set; }
        public float Mass { get; set; }
        public float MaxThrust { get; set; }
        public float MaxSpeed { get; set; }
        public float RotationalFriction { get; set; }
        public float MaxAngularAcceleration { get; set; }
        public DragonPhysics(string type) : base(type)
        {
            
        }
    }
}

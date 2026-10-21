using System.Numerics;

namespace TPToolkitLib.WorldObject.Definitions.PhysicsFactories
{
    public class WhalePhysics : PhysicsDefinition
    {
        public Vector3 CenterOfMass { get; set; }
        public float Mass { get; set; }
        public float MaxThrust { get; set; }
        public float MaxSpeed { get; set; }
        public float RotationalFriction { get; set; }
        public float MaxAngularAcceleration { get; set; }
        public float MaxDivePitch { get; set; }
        public float MaxClimbPitch { get; set; }
        public float DiveTime { get; set; }
        public WhalePhysics(string type) : base(type)
        {
            
        }
    }
}

using System.Numerics;

namespace TPToolkitLib.WorldObject.Definitions.PhysicsFactories
{
    public class MinePhysics : PhysicsDefinition
    {
        public Vector3 CenterOfMass { get; set; }
        public float Mass { get; set; }
        public float MaximumSpeed { get; set; }
        public MinePhysics(string type) : base(type)
        {
            
        }
    }
}

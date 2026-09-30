using System.Numerics;

namespace TPToolkitLib.WorldObject.Definitions.PhysicsFactories
{
    public class ProjectilePhysics : PhysicsDefinition
    {
        public Vector3 CenterOfMass { get; set; }
        public float Mass { get; set; }
        public ProjectilePhysics(string type) : base(type)
        {
            
        }
    }
}

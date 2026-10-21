namespace TPToolkitLib.WorldObject.Definitions
{
    public abstract class PhysicsDefinition
    {
        public string Type { get; set; }

        public PhysicsDefinition(string type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type;
        }
    }
}

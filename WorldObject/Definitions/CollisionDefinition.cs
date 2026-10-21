namespace TPToolkitLib.WorldObject.Definitions
{
    public abstract class CollisionDefinition
    {
        public string Type { get; set; }

        public CollisionDefinition(string type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type;
        }
    }
}

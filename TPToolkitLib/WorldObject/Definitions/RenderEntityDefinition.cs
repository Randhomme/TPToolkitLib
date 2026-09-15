namespace TPToolkitLib.WorldObject.Definitions
{
    public abstract class RenderEntityDefinition
    {
        public string Type { get; set; }

        public RenderEntityDefinition(string type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type;
        }
    }
}

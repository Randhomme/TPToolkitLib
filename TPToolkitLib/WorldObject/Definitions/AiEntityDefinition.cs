namespace TPToolkitLib.WorldObject.Definitions
{
    public abstract class AiEntityDefinition
    {
        public string Type { get; set; }

        public AiEntityDefinition(string type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type;
        }
    }
}

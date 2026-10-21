namespace TPToolkitLib.WorldObject.Definitions
{
    public abstract class CustomInfoDefinition
    {
        public string Type { get; set; }

        public CustomInfoDefinition(string type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type;
        }
    }
}

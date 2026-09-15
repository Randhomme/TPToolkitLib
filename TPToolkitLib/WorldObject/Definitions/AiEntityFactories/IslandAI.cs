namespace TPToolkitLib.WorldObject.Definitions.AiEntityFactories
{
    public class IslandAI : AiEntityDefinition
    {
        public float DefaultSightRange { get; set; }
        public IslandAI(string type) : base(type) { }
    }
}

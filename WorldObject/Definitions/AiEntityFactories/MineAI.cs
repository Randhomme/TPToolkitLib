namespace TPToolkitLib.WorldObject.Definitions.AiEntityFactories
{
    public class MineAI : AiEntityDefinition
    {
        public float DefaultSightRange { get; set; }
        public MineAI(string type) : base(type) { }
    }
}

namespace TPToolkitLib.WorldObject.Definitions.AiEntityFactories
{
    public class DragonAI : AiEntityDefinition
    {
        public float DefaultSightRange { get; set; }
        public DragonAI(string type) : base(type) { }
    }
}

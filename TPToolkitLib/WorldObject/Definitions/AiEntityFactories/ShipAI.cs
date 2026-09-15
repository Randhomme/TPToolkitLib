namespace TPToolkitLib.WorldObject.Definitions.AiEntityFactories
{
    public class ShipAI : AiEntityDefinition
    {
        public float DefaultSightRange { get; set; }
        public ShipAI(string type) : base(type) { }
    }
}

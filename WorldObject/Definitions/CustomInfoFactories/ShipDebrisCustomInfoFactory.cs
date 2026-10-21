namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class ShipDebrisCustomInfoFactory : CustomInfoDefinition
    {
        public float LifeTimeMin { get; set; }
        public float LifeTimeMax { get; set; }
        public ShipDebrisCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

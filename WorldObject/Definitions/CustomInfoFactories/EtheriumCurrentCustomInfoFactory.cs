namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class EtheriumCurrentCustomInfoFactory : CustomInfoDefinition
    {
        public float Magnitude { get; set; }
        public float Radius { get; set; }
        public EtheriumCurrentCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

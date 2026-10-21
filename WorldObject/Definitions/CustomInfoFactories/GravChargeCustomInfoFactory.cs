namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class GravChargeCustomInfoFactory : BulletCustomInfoFactory
    {
        public float Magnitude { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }
        public GravChargeCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class EnergyNetCustomInfoFactory : BulletCustomInfoFactory
    {
        public float Duration { get; set; }
        public float PercentageOfEnergyDissipated { get; set; }
        public EnergyNetCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

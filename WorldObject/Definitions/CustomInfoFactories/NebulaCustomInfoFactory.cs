using TPToolkitLib.WorldObject.Definitions.CustomInfoFactories.Classes;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class NebulaCustomInfoFactory : CustomInfoDefinition
    {
        public float MinimumLightningSpawnDistanceFromShips { get; set; }
        public float MinimumLightningBoltLength { get; set; }
        public float MaximumLightningSpawnPointZAmplitude { get; set; }
        public float MeteorHitChanceModifier { get; set; }
        public DamagePotential LightningDamagePotential { get; } = new();
        public DamagePotential WindDamagePotential { get; } = new();
        public DamagePotential MeteorDamagePotential { get; } = new();
        public string NebulaSoundName { get; set; } = string.Empty; // not case sensitive
        public string SolarStormSoundName { get; set; } = string.Empty; // not case sensitive
        public string MeteorShowerSoundName { get; set; } = string.Empty; // not case sensitive
        public NebulaCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

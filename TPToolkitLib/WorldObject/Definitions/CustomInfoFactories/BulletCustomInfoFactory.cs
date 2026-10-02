using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class BulletCustomInfoFactory : CustomInfoDefinition
    {
        public string HitEffect { get; set; } = string.Empty;
        public string BulletEffect { get; set; } = string.Empty;
        public int DamageHitPoints { get; set; }
        public DecalDamageSize DecalDamageSize { get; set; }
        public float ChanceOfCriticalDamageWood { get; set; }
        public float ChanceOfFireWood { get; set; }
        public float InitialFireStrengthWood { get; set; }
        public float DamageLowerBoundWood { get; set; }
        public float DamageUpperBoundWood { get; set; }
        public float ChanceOfCriticalDamageReinforced { get; set; }
        public float ChanceOfFireReinforced { get; set; }
        public float InitialFireStrengthReinforced { get; set; }
        public float DamageLowerBoundReinforced { get; set; }
        public float DamageUpperBoundReinforced { get; set; }
        public float ChanceOfCriticalDamageIron { get; set; }
        public float ChanceOfFireIron { get; set; }
        public float InitialFireStrengthIron { get; set; }
        public float DamageLowerBoundIron { get; set; }
        public float DamageUpperBoundIron { get; set; }
        public float ChanceOfCriticalDamageCloth { get; set; }
        public float ChanceOfFireCloth { get; set; }
        public float InitialFireStrengthCloth { get; set; }
        public float DamageLowerBoundCloth { get; set; }
        public float DamageUpperBoundCloth { get; set; }
        public float ChanceOfCriticalDamageStone { get; set; }
        public float ChanceOfFireStone { get; set; }
        public float InitialFireStrengthStone { get; set; }
        public float DamageLowerBoundStone { get; set; }
        public float DamageUpperBoundStone { get; set; }
        public float ChanceOfCriticalDamageDragon { get; set; }
        public float ChanceOfFireDragon { get; set; }
        public float InitialFireStrengthDragon { get; set; }
        public float DamageLowerBoundDragon { get; set; }
        public float DamageUpperBoundDragon { get; set; }
        public float ImpactSoundMaxDistance { get; set; }
        public float ImpactSoundVolume { get; set; }
        public float TravelSoundMaxDistance { get; set; }
        public float TravelSoundVolume { get; set; }
        public string RicochetSoundHullWood { get; set; } = string.Empty;
        public string ImpactSound0HullWood { get; set; } = string.Empty;
        public string ImpactSound1HullWood { get; set; } = string.Empty;
        public string ImpactSound2HullWood { get; set; } = string.Empty;
        public string ImpactSound3HullWood { get; set; } = string.Empty;
        public string ImpactSound4HullWood { get; set; } = string.Empty;
        public string RicochetSoundHullReInforcedWood { get; set; } = string.Empty;
        public string ImpactSound0HullReInforcedWood { get; set; } = string.Empty;
        public string ImpactSound1HullReInforcedWood { get; set; } = string.Empty;
        public string ImpactSound2HullReInforcedWood { get; set; } = string.Empty;
        public string ImpactSound3HullReInforcedWood { get; set; } = string.Empty;
        public string ImpactSound4HullReInforcedWood { get; set; } = string.Empty;
        public string RicochetSoundHullIron { get; set; } = string.Empty;
        public string ImpactSound0HullIron { get; set; } = string.Empty;
        public string ImpactSound1HullIron { get; set; } = string.Empty;
        public string ImpactSound2HullIron { get; set; } = string.Empty;
        public string ImpactSound3HullIron { get; set; } = string.Empty;
        public string ImpactSound4HullIron { get; set; } = string.Empty;
        public string RicochetSoundSailCloth { get; set; } = string.Empty;
        public string ImpactSound0SailCloth { get; set; } = string.Empty;
        public string ImpactSound1SailCloth { get; set; } = string.Empty;
        public string ImpactSound2SailCloth { get; set; } = string.Empty;
        public string ImpactSound3SailCloth { get; set; } = string.Empty;
        public string ImpactSound4SailCloth { get; set; } = string.Empty;
        public string RicochetSoundWallStone { get; set; } = string.Empty;
        public string ImpactSound0WallStone { get; set; } = string.Empty;
        public string ImpactSound1WallStone { get; set; } = string.Empty;
        public string ImpactSound2WallStone { get; set; } = string.Empty;
        public string ImpactSound3WallStone { get; set; } = string.Empty;
        public string ImpactSound4WallStone { get; set; } = string.Empty;
        public string RicochetSoundDragonScale { get; set; } = string.Empty;
        public string ImpactSound0DragonScale { get; set; } = string.Empty;
        public string ImpactSound1DragonScale { get; set; } = string.Empty;
        public string ImpactSound2DragonScale { get; set; } = string.Empty;
        public string ImpactSound3DragonScale { get; set; } = string.Empty;
        public string ImpactSound4DragonScale { get; set; } = string.Empty;
        public string TravelSound { get; set; } = string.Empty;
        public float Lifetime { get; set; } = -1; // For no Lifetime, like GravChargeCustomInfoFactory
        public BulletCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

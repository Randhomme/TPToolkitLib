using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Tools;
using TPToolkitLib.WorldObject.Definitions.CustomInfoFactories.Classes;
using TPToolkitLib.WorldObject.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class BulletCustomInfoFactory : CustomInfoDefinition
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "The HitEffect is required, and cannot be an empty string.")]
        public string HitEffect { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The BulletEffect is required, and cannot be an empty string.")]
        public string BulletEffect { get; set; } // case sensitive
        public int DamageHitPoints { get; set; }
        public DecalDamageSize DecalDamageSize { get; set; }
        public DamagePotential DamagePotential { get; } = new();
        public float ImpactSoundMaxDistance { get; set; }
        public float ImpactSoundVolume { get; set; }
        public float TravelSoundMaxDistance { get; set; }
        public float TravelSoundVolume { get; set; }
        public string RicochetSoundHullWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound0HullWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound1HullWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound2HullWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound3HullWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound4HullWood { get; set; } = string.Empty; // not case sensitive
        public string RicochetSoundHullReInforcedWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound0HullReInforcedWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound1HullReInforcedWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound2HullReInforcedWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound3HullReInforcedWood { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound4HullReInforcedWood { get; set; } = string.Empty; // not case sensitive
        public string RicochetSoundHullIron { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound0HullIron { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound1HullIron { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound2HullIron { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound3HullIron { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound4HullIron { get; set; } = string.Empty; // not case sensitive
        public string RicochetSoundSailCloth { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound0SailCloth { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound1SailCloth { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound2SailCloth { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound3SailCloth { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound4SailCloth { get; set; } = string.Empty; // not case sensitive
        public string RicochetSoundWallStone { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound0WallStone { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound1WallStone { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound2WallStone { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound3WallStone { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound4WallStone { get; set; } = string.Empty; // not case sensitive
        public string RicochetSoundDragonScale { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound0DragonScale { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound1DragonScale { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound2DragonScale { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound3DragonScale { get; set; } = string.Empty; // not case sensitive
        public string ImpactSound4DragonScale { get; set; } = string.Empty; // not case sensitive
        public string TravelSound { get; set; } = string.Empty; // not case sensitive
        public float Lifetime { get; set; } = -1; // For no Lifetime, like GravChargeCustomInfoFactory
        public BulletCustomInfoFactory(string type) : base(type)
        {
            HitEffect = TPGameTool.Effects.FirstOrDefault();
            BulletEffect = TPGameTool.Effects.FirstOrDefault();
        }
    }
}

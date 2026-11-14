using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Enums;
using TPToolkitLib.Tools;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class ShipCustomInfoFactory : CustomInfoDefinition
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "The ShipBarIconTexture is required, and cannot be an empty string.")]
        public string ShipBarIconTexture { get; set; } // case sensitive
        public float SelectedIndicatorPercentageOfRadius { get; set; }
        public float DistanceToStartSpherePicking { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The DisplayableShipnameStringID is required, and cannot be an empty string.")]
        public string DisplayableShipnameStringID { get; set; } // case sensitive
        public Race ShipRace { get; set; }
        public bool IsTender { get; set; }
        public int NumberOfLifeboats {  get; set; }
        public bool IsLifeboat { get; set; }
        public bool IsCloakable { get; set; }
        public int ShipSize { get; set; }
        public int CoreDamageSectionMaxHitPoints { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The ExplosionEffectName is required, and cannot be an empty string.")]
        public string ExplosionEffectName { get; set; } // case sensitive
        public EngineType EngineType { get; set; }
        public string EngineSoundNameEmergency { get; set; } = string.Empty; // not case sensitive
        public string EngineSoundNameFull { get; set; } = string.Empty; // not case sensitive
        public string EngineSoundNameHalf { get; set; } = string.Empty; // not case sensitive
        public int VictoryPointCost { get; set; }
        public bool IsAvailableInMultiplayer { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID00 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID00 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID01 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID01 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID02 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID02 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID03 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID03 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID04 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID04 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID05 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID05 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID06 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID06 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID07 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID07 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID08 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID08 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID09 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID09 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID10 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID10 { get; set; } // case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The AvailableUniqueShipNameID11 is required, and cannot be an empty string.")]
        public string AvailableUniqueShipNameID11 { get; set; } // case sensitive
        public int MaxNumberOfGunners { get; set; }
        public int MaxNumberOfCaptains { get; set; }
        public int MaxNumberOfFirstMates { get; set; }
        public int MaxNumberOfNavigators { get; set; }
        public int MaxNumberOfEngineers { get; set; }
        public int MaxNumberOfRiggers { get; set; }
        public int MaxNumberOfFighters { get; set; }
        public int MaxNumberOfLookouts { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The RepairEffectName is required, and cannot be an empty string.")]
        public string RepairEffectName { get; set; } // case sensitive
        public ShipCustomInfoFactory(string type) : base(type)
        {
            ShipBarIconTexture = TPGameTool.HudEntries.FirstOrDefault();
            DisplayableShipnameStringID = TPGameTool.TPGameHeaderStrings.FirstOrDefault().Strings.Keys.FirstOrDefault();
            ExplosionEffectName = TPGameTool.Effects.FirstOrDefault();
            AvailableUniqueShipNameID00 = AvailableUniqueShipNameID01 = AvailableUniqueShipNameID02 = AvailableUniqueShipNameID03 = AvailableUniqueShipNameID04 = AvailableUniqueShipNameID05 = AvailableUniqueShipNameID06 = AvailableUniqueShipNameID07 = AvailableUniqueShipNameID08 = AvailableUniqueShipNameID09 = AvailableUniqueShipNameID10 = AvailableUniqueShipNameID11 = TPGameTool.TPGameHeaderStrings.FirstOrDefault().Strings.Keys.FirstOrDefault();
            RepairEffectName = TPGameTool.Effects.FirstOrDefault();
        }
    }
}

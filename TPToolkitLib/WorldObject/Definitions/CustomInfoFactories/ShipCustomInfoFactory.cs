using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class ShipCustomInfoFactory : CustomInfoDefinition
    {
        public string ShipBarIconTexture { get; set; } = string.Empty;
        public float SelectedIndicatorPercentageOfRadius { get; set; }
        public float DistanceToStartSpherePicking { get; set; }
        public string DisplayableShipnameStringID { get; set; } = string.Empty;
        public Race ShipRace { get; set; }
        public bool IsTender { get; set; }
        public int NumberOfLifeboats {  get; set; }
        public bool IsLifeboat { get; set; }
        public bool IsCloakable { get; set; }
        public int ShipSize { get; set; }
        public int CoreDamageSectionMaxHitPoints { get; set; }
        public string ExplosionEffectName { get; set; } = string.Empty;
        public EngineType EngineType { get; set; }
        public string EngineSoundNameEmergency { get; set; } = string.Empty;
        public string EngineSoundNameFull { get; set; } = string.Empty;
        public string EngineSoundNameHalf { get; set; } = string.Empty;
        public int VictoryPointCost { get; set; }
        public bool IsAvailableInMultiplayer { get; set; }
        public string AvailableUniqueShipNameID00 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID01 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID02 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID03 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID04 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID05 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID06 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID07 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID08 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID09 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID10 { get; set; } = string.Empty;
        public string AvailableUniqueShipNameID11 { get; set; } = string.Empty;
        public int MaxNumberOfGunners { get; set; }
        public int MaxNumberOfCaptains { get; set; }
        public int MaxNumberOfFirstMates { get; set; }
        public int MaxNumberOfNavigators { get; set; }
        public int MaxNumberOfEngineers { get; set; }
        public int MaxNumberOfRiggers { get; set; }
        public int MaxNumberOfFighters { get; set; }
        public int MaxNumberOfLookouts { get; set; }
        public string RepairEffectName { get; set; } = string.Empty;
        public ShipCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

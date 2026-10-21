using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class GunCustomInfoFactory : CustomInfoDefinition
    {
        public string BulletTypeName_PrimaryString { get; set; } = "NoBulletType";
        public WorldObjectType? BulletTypeName_Primary { get; set; }
        public string BulletTypeName_SecondaryString { get; set; } = "NoBulletType";
        public WorldObjectType? BulletTypeName_Secondary { get; set; }
        public string SoundName { get; set; } = string.Empty;
        public string MuzzleFlashEffect { get; set; } = string.Empty;
        public float MuzzleSpeed { get; set; }
        public float DuringBurstReloadTime { get; set; }
        public int BulletsPerBurst { get; set; }
        public float TimeBetweenBursts { get; set; }
        public bool CalculateAccuracyDeviationDuringBurst { get; set; }
        public string WeaponBarIconTexture { get; set; } = string.Empty;
        public float VerticalRotationSpeed { get; set; }
        public float HorizontalRotationSpeed { get; set; }
        public float VerticalMinAngle { get; set; }
        public float VerticalMaxAngle { get; set; }
        public float LobAngle { get; set; }
        public float MaximumRange_Range { get; set; }
        public float LongRange_Range { get; set; }
        public float EffectiveRange_Range { get; set; }
        public bool BulletAffectedByGravity { get; set; }
        public float MaximumRange_AccuracyDeviation { get; set; }
        public float LongRange_AccuracyDeviation { get; set; }
        public float EffectiveRange_AccuracyDeviation { get; set; }
        public float MaximumRange_RicochetFactor { get; set; }
        public float LongRange_RicochetFactor { get; set; }
        public float EffectiveRange_RicochetFactor { get; set; }
        public int VictoryPointCost { get; set; }
        public string GunNameStringID { get; set; } = string.Empty;
        public MountType MountType { get; set; }
        public Race ExclusionRace { get; set; }
        public bool CanBeFiredWhileCloaked { get; set; }
        public GenericDialogSound FiringCrewAlert { get; set; }
        public float SoundMaxDistance { get; set; }
        public float SoundVolume { get; set; }
        public GunCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

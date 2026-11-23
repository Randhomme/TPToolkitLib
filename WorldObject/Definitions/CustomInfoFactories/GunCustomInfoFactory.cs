using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Enums;
using TPToolkitLib.Tools;
using TPToolkitLib.WorldObject.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class GunCustomInfoFactory : CustomInfoDefinition
    {
        public string BulletTypeName_PrimaryString { get; set; } = "NoBulletType"; // case sensitive
        public WorldObjectType? BulletTypeName_Primary
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    BulletTypeName_PrimaryString = value.Type;
                else
                    BulletTypeName_PrimaryString = string.Empty;
            }
        }
        public string BulletTypeName_SecondaryString { get; set; } = "NoBulletType"; // case sensitive
        public WorldObjectType? BulletTypeName_Secondary
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    BulletTypeName_SecondaryString = value.Type;
                else
                    BulletTypeName_SecondaryString = string.Empty;
            }
        }
        public string SoundName { get; set; } = string.Empty; // not case sensitive
        [Required(AllowEmptyStrings = false, ErrorMessage = "The MuzzleFlashEffect is required, and cannot be an empty string.")]
        public string MuzzleFlashEffect { get; set; } // case sensitive
        public float MuzzleSpeed { get; set; }
        public float DuringBurstReloadTime { get; set; }
        public int BulletsPerBurst { get; set; }
        public float TimeBetweenBursts { get; set; }
        public bool CalculateAccuracyDeviationDuringBurst { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The WeaponBarIconTexture is required, and cannot be an empty string.")]
        public string WeaponBarIconTexture { get; set; } // case sensitive
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
        [Required(AllowEmptyStrings = false, ErrorMessage = "The GunNameStringID is required, and cannot be an empty string.")]
        public string GunNameStringID { get; set; } // case sensitive
        public MountType MountType { get; set; }
        public Race ExclusionRace { get; set; }
        public bool CanBeFiredWhileCloaked { get; set; }
        public GenericDialogSound FiringCrewAlert { get; set; }
        public float SoundMaxDistance { get; set; }
        public float SoundVolume { get; set; }
        public GunCustomInfoFactory(string type) : base(type)
        {
            MuzzleFlashEffect = TPGameTool.Effects.FirstOrDefault();
            WeaponBarIconTexture = TPGameTool.HudEntries.FirstOrDefault();
            GunNameStringID = TPGameTool.TPGameHeaderStrings.FirstOrDefault().Strings.Keys.FirstOrDefault();
        }
    }
}

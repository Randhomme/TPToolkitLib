using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Enums;
using TPToolkitLib.Tools;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class CrewCustomInfoFactory : CustomInfoDefinition
    {
        public Race CrewRace { get; set; }
        public int PointValue { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The CrewNameStringID is required, and cannot be an empty string.")]
        public string CrewNameStringID { get; set; } // case sensitive
        public bool LeadershipSkillActive { get; set; }
        public int LeadershipSkillAbilityValue { get; set; }
        public bool NavigationSkillActive { get; set; }
        public int NavigationSkillAbilityValue { get; set; }
        public bool SpottingSkillActive { get; set; }
        public int SpottingSkillAbilityValue { get; set; }
        public bool EngineeringSkillActive { get; set; }
        public int EngineeringSkillAbilityValue { get; set; }
        public bool RiggingSkillActive { get; set; }
        public int RiggingSkillAbilityValue { get; set; }
        public bool CombatSkillActive { get; set; }
        public int CombatSkillAbilityValue { get; set; }
        public bool GunnerySkillActive { get; set; }
        public int GunnerySkillAbilityValue { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The TalkingHeadTexture is required, and cannot be an empty string.")]
        public string TalkingHeadTexture { get; set; } // case sensitive
        public Species Species { get; set; }
        public CrewCustomInfoFactory(string type) : base(type)
        {
            CrewNameStringID = TPGameTool.TPGameHeaderStrings.FirstOrDefault().Strings.Keys.FirstOrDefault();
            TalkingHeadTexture = TPGameTool.HudEntries.FirstOrDefault();
        }
    }
}

using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class CrewCustomInfoFactory : CustomInfoDefinition
    {
        public Race CrewRace { get; set; }
        public int PointValue { get; set; }
        public string CrewNameStringID { get; set; } = string.Empty;
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
        public string TalkingHeadTexture { get; set; } = string.Empty;
        public Species Species { get; set; }
        public CrewCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

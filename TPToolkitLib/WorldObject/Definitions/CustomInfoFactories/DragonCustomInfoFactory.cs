using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class DragonCustomInfoFactory : CustomInfoDefinition
    {
        public float SelectedIndicatorPercentageOfRadius { get; set; }
        public float DistancetoStartSpherePicking { get; set; }
        public float ShortestTimeBetweenSounds { get; set; }
        public float LongestTimeBetweenSounds { get; set; }
        public string Sound_0 { get; set; } = string.Empty;
        public string Sound_1 { get; set; } = string.Empty;
        public string Sound_2 { get; set; } = string.Empty;
        public string Sound_3 { get; set; } = string.Empty;
        public string Sound_4 { get; set; } = string.Empty;
        public string Sound_5 { get; set; } = string.Empty;
        public string Sound_6 { get; set; } = string.Empty;
        public string Sound_7 { get; set; } = string.Empty;
        public string Sound_8 { get; set; } = string.Empty;
        public string Sound_9 { get; set; } = string.Empty;
        public bool ReportSpotting { get; set; }
        public GenericDialogSound ReportType { get; set; }
        public float SoundDistance { get; set; }
        public DragonCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

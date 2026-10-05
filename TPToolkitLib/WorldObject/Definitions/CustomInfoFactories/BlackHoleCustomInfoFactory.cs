namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class BlackHoleCustomInfoFactory : CustomInfoDefinition
    {
        public float Magnitude { get; set; }
        public float Radius { get; set; }
        public string VortexEffectName { get; set; } = string.Empty;
        public float AmbientSoundMaxDistance { get; set; }
        public string AmbientSoundName { get; set; } = string.Empty;
        public BlackHoleCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

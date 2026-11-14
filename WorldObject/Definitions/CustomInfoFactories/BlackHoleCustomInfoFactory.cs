using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Tools;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class BlackHoleCustomInfoFactory : CustomInfoDefinition
    {
        public float Magnitude { get; set; }
        public float Radius { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The VortexEffectName is required, and cannot be an empty string.")]
        public string VortexEffectName { get; set; } // case sensitive
        public float AmbientSoundMaxDistance { get; set; }
        public string AmbientSoundName { get; set; } = string.Empty; // not case sensitive
        public BlackHoleCustomInfoFactory(string type) : base(type)
        {
            VortexEffectName = TPGameTool.Effects.FirstOrDefault();
        }
    }
}

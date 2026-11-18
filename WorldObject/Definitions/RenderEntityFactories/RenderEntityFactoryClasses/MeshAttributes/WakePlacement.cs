using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Tools;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class WakePlacement : MeshSubAttribute
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "The EffectName is required, and cannot be an empty string.")]
        public string EffectName { get; set; } // case sensitive
        public WakePlacement()
        {
            EffectName = TPGameTool.Effects.FirstOrDefault();
        }
    }
}

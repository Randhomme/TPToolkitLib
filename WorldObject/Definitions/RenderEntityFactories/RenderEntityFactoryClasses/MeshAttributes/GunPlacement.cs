using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Interfaces;
using TPToolkitLib.Tools;
using TPToolkitLib.WorldObject.Enums;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class GunPlacement : MeshSubAttribute, IDependencyResolvable
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "The GunName is required, and cannot be an empty string.")]
        public string GunName { get; set; } // case sensitive
        public WorldObjectType? Gun
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    GunName = value.Type;
                else
                    GunName = string.Empty;
            }
        }
        public float MaxRotation { get; set; }
        public Bank Bank { get; set; }

        public GunPlacement()
        {
            Gun = TPGameTool.WorldObjects.FirstOrDefault();
            GunName = Gun?.Type ?? string.Empty;
        }

        public void ResolveDependency()
        {
            Gun = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(GunName, StringComparison.Ordinal)); // Case sensitive
        }
    }
}

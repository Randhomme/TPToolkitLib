using System;
using System.Linq;
using TPToolkitLib.Enums;
using TPToolkitLib.Interfaces;
using TPToolkitLib.Tools;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class GunPlacement : MeshSubAttribute, IDependencyResolvable
    {
        public string GunName { get; set; } = string.Empty;
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

        public void ResolveDependency()
        {
            Gun = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(GunName, StringComparison.Ordinal)); // Case sensitive
        }
    }
}

using System;
using System.Linq;
using TPToolkitLib.Interfaces;
using TPToolkitLib.Tools;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class MeshHitPoints : MeshSubAttribute, IDependencyResolvable
    {
        public string AssociationName { get; set; } = string.Empty;
        private WorldObjectType? worldObject;
        public WorldObjectType? WorldObject
        {
            get => worldObject;
            set
            {
                worldObject = value;
                if (value != null)
                    AssociationName = value.Type;
                else
                    AssociationName = string.Empty;
            }
        }

        public void ResolveDependency()
        {
            WorldObject = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(AssociationName, StringComparison.Ordinal)); // Case sensitive
        }
    }
}

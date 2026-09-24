using System;
using System.Linq;
using TPToolkitLib.Interfaces;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class MeshHitPoints : MeshSubAttribute, IDependencyResolvable
    {
        public string AssociationName { get; set; } = string.Empty;
        public WorldObjectType? WorldObject { get; set; }

        public void ResolveDependency()
        {
            WorldObject = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(AssociationName, StringComparison.Ordinal)); // Case sensitive
        }
    }
}

using System;
using System.Linq;
using TPToolkitLib.Interfaces;
using TPToolkitLib.Tools;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class AsteroidCustomInfoFactory : CustomInfoDefinition, IDependencyResolvable
    {
        public int MaxHitPoints { get; set; }
        public string WorldObjectToCreateUponDeath0Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath1Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath2Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath3Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath4Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath5Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath6Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath7Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath8Name { get; set; } = string.Empty;
        public string WorldObjectToCreateUponDeath9Name { get; set; } = string.Empty;
        public WorldObjectType? WorldObjectToCreateUponDeath0
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath0Name = value.Type;
                else
                    WorldObjectToCreateUponDeath0Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath1
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath1Name = value.Type;
                else
                    WorldObjectToCreateUponDeath1Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath2
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath2Name = value.Type;
                else
                    WorldObjectToCreateUponDeath2Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath3
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath3Name = value.Type;
                else
                    WorldObjectToCreateUponDeath3Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath4
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath4Name = value.Type;
                else
                    WorldObjectToCreateUponDeath4Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath5
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath5Name = value.Type;
                else
                    WorldObjectToCreateUponDeath5Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath6
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath6Name = value.Type;
                else
                    WorldObjectToCreateUponDeath6Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath7
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath7Name = value.Type;
                else
                    WorldObjectToCreateUponDeath7Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath8
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath8Name = value.Type;
                else
                    WorldObjectToCreateUponDeath8Name = string.Empty;
            }
        }
        public WorldObjectType? WorldObjectToCreateUponDeath9
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    WorldObjectToCreateUponDeath9Name = value.Type;
                else
                    WorldObjectToCreateUponDeath9Name = string.Empty;
            }
        }
        public string ExplosionEffect { get; set; } = string.Empty;
        public bool ReportSpotting { get; set; }
        public AsteroidCustomInfoFactory(string type) : base(type) { }

        public void ResolveDependency()
        {
            WorldObjectToCreateUponDeath0 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath0Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath1 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath1Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath2 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath2Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath3 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath3Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath4 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath4Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath5 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath5Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath6 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath6Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath7 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath7Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath8 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath8Name, StringComparison.Ordinal)); // Case sensitive
            WorldObjectToCreateUponDeath9 = TPGameTool.WorldObjects.FirstOrDefault((wot) => wot.Type.Equals(WorldObjectToCreateUponDeath9Name, StringComparison.Ordinal)); // Case sensitive
        }
    }
}

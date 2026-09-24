using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TPToolkitLib.Enums;
using TPToolkitLib.Interfaces;
using TPToolkitLib.MeshScene.Classes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class DamageSection : MeshSubAttribute, IDependencyResolvable
    {
        public bool VitalToShip { get; set; }
        public bool VitalToMaxVelocity { get; set; }
        public bool VitalToManeuverability { get; set; }
        public bool VitalToMission { get; set; }
        public bool Swappable { get; set; }
        public Material Material { get; set; }
        public int Hitpoints { get; set; }
        public int VitalSectionCoreDamagePercent { get; set; }
        public IList<Critical> Criticals { get; } = [];
        public IList<string> Adjacents { get; } = [];
        public IList<MsbElement> AdjacentElements { get; } = [];
        public string DebrisMeshScene { get; set; } = string.Empty;
        public MsbScene? DebrisMsbScene { get; set; }
        public string DebrisFx { get; set; } = string.Empty;
        public IList<DestructFx> DestructFxs { get; } = [];

        public void ResolveDependency()
        {
            DebrisMsbScene = TPGameTool.MeshScenes.FirstOrDefault((ms) => ms.Name.Equals(DebrisMeshScene, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class DestructFx
    {
        public string Effect { get; set; } = string.Empty;
        public float A { get; set; }
        public float B { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}

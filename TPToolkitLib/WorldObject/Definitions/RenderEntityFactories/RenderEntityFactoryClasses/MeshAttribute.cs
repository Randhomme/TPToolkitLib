using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPToolkitLib.Interfaces;
using TPToolkitLib.MeshScene.Classes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses
{
    public class MeshAttribute
    {
        public string MeshName { get; set; } = string.Empty;
        /// <summary>
        /// The MsbElement that corresponds to this MeshAttribute's MeshName. This is resolved after the MsbScene is loaded and the MeshName is matched to an element in the scene.
        /// </summary>
        public MsbElement? MsbElement { get; set; }
        public IList<MeshSubAttribute> MeshSubAttributes { get; } = [];
    }
}

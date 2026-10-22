using System.Collections.Generic;
using TPToolkitLib.MeshScene.Classes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses
{
    public class MeshAttribute
    {
        public string MeshName { get; set; } = string.Empty;
        /// <summary>
        /// The MsbElement that corresponds to this MeshAttribute's MeshName. This is resolved after the MsbScene is loaded and the MeshName is matched to an element in the scene.
        /// </summary>
        public MsbElement? Mesh
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    MeshName = value.Name;
                else
                    MeshName = string.Empty;
            }
        }
        public IList<MeshSubAttribute> MeshSubAttributes { get; } = [];
    }
}

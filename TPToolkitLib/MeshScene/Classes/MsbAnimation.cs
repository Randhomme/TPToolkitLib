using System.Collections.Generic;

namespace TPToolkitLib.MeshScene.Classes
{
    public class MsbAnimation : MsbNamedElement
    {
        public float Duration { get; set; }
        public IList<MsbMotion> MsbMotions { get; } = [];
    }
}

using System.Collections.Generic;
using System.Numerics;

namespace TPToolkitLib.MeshScene.Classes
{
    public class MsbElement : MsbNamedElement
    {
        public MsbElement? Parent { get; set; }
        public Vector3 Pivot { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; } // yaw, pitch, roll (in radian)
        public IList<MsbAttribute> MsbAttirbutes { get; } = [];
    }
}

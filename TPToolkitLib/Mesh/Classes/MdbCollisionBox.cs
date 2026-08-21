using System.Numerics;

namespace TPToolkitLib.Mesh.Classes
{
    public class MdbCollisionBox
    {
        public string BoxName;
        public uint Level = 0;
        public Vector3 Position;
        public Vector3 OCross;
        public Vector3 OForward;
        public Vector3 OUp;
        public Vector3 Length;
        public MdbCollisionBox Leftchild { get; set; }
        public MdbCollisionBox Rightchild { get; set; }
    }
}

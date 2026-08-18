using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TPToolkitLib.Mesh.Structs;

namespace TPToolkitLib.Mesh.Classes
{
    public class ObjScene
    {
        public IList<Vector3> V { get; } = [];
        public IList<Vector2> VT { get; } = [];
        public IList<Vector3> VN { get; } = [];
        public IList<ObjTriangle> F { get; } = [];
        public IList<ObjGroup> ObjGroups { get; } = [];
    }
}

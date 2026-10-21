using System.Collections.Generic;
using TPToolkitLib.Mesh.Structs;

namespace TPToolkitLib.Mesh.Classes
{
    public class ObjMaterialGroup
    {
        public string MaterialName { get; set; }
        public IList<ObjTriangle> Triangles { get; }
        public ObjMaterialGroup(string materialName)
        {
            MaterialName = materialName;
            Triangles = [];
        }
        public override string ToString()
        {
            return MaterialName;
        }
    }
}

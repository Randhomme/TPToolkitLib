using System.Collections.Generic;
using TPToolkitLib.Mesh.Structs;

namespace TPToolkitLib.Mesh.Classes
{
    public class MdbMeshModel
    {
        public IList<MdbVertex> MdbVertices { get; }
        public IList<MdbTriangle> MdbTriangles { get; }

        public MdbMeshModel()
        {
            MdbVertices = new List<MdbVertex>();
            MdbTriangles = new List<MdbTriangle>();
        }
    }
}

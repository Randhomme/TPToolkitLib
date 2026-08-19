using System;
using System.Collections.Generic;
using System.IO;

namespace TPToolkitLib.Mesh.Classes
{
    public class MdbMesh
    {
        public string GroupName { get; set; }
        public IList<MdbMeshModel> MeshModels { get; }
        public IList<MdbMaterial> Materials { get; }
        public CollisionBox CollisionBox { get; }

        public MdbMesh(string groupName)
        {
            GroupName = groupName;
            MeshModels = [];
            Materials = [];
            CollisionBox = new();
        }
    }
}

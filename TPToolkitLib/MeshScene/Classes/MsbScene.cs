using System.Collections.Generic;

namespace TPToolkitLib.MeshScene.Classes
{
    public class MsbScene
    {
        public string Name { get; set; } = string.Empty;
        public IList<MsbNode> MsbNodes { get; } = [];
        public IList<MsbMesh> MsbMeshes { get; } = [];
        public IList<MsbBone> MsbBones { get; } = [];
        public IList<MsbAnimation> MsbAnimations { get; } = [];
    }
}

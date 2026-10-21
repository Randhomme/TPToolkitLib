using System.Collections.Generic;

namespace TPToolkitLib.MeshScene.Classes
{
    public class MsbScene
    {
        public static MsbScene None = new() { Name = "NONE" };
        public string Name { get; set; } = string.Empty;
        public MsbElement? RootMsbElement { get; set; }
        public IList<MsbNode> MsbNodes { get; } = [];
        public IList<MsbMesh> MsbMeshes { get; } = [];
        public IList<MsbBone> MsbBones { get; } = [];
        public IList<MsbAnimation> MsbAnimations { get; } = [];

        public override string ToString()
        {
            return Name;
        }
    }
}

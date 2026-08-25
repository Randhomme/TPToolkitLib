using System.Collections.Generic;

namespace TPToolkitLib.MeshScene.Classes
{
    public class MsbMotion
    {
        public MsbElement? MsbElement { get; set; }
        public IList<MsbKeyframe> MsbChannel1 { get; } = [];
        public IList<MsbKeyframe> MsbChannel2 { get; } = [];
        public IList<MsbKeyframe> MsbChannel3 { get; } = [];
        public IList<MsbKeyframe> MsbChannel4 { get; } = [];
        public IList<MsbKeyframe> MsbChannel5 { get; } = [];
        public IList<MsbKeyframe> MsbChannel6 { get; } = [];

        public override string ToString()
        {
            return MsbElement?.ToString() ?? string.Empty;
        }
    }
}

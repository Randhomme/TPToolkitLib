using System.Numerics;
using TPToolkitLib.Enums;

namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class IslandCustomInfoFactory : CustomInfoDefinition
    {
        public Vector3 OcclusionPolygonPoint00 { get; set; }
        public Vector3 OcclusionPolygonPoint01 { get; set; }
        public Vector3 OcclusionPolygonPoint02 { get; set; }
        public Vector3 OcclusionPolygonPoint03 { get; set; }
        public Vector3 OcclusionPolygonPoint04 { get; set; }
        public Vector3 OcclusionPolygonPoint05 { get; set; }
        public Vector3 OcclusionPolygonPoint06 { get; set; }
        public Vector3 OcclusionPolygonPoint07 { get; set; }
        public Vector3 OcclusionPolygonPoint08 { get; set; }
        public Vector3 OcclusionPolygonPoint09 { get; set; }
        public int OcclusionPolygonPointCount { get; set; }
        public Race Race { get; set; }
        public bool HasAmbientSound { get; set; }
        public float AmbientSoundMaxDistance { get; set; }
        public string AmbientSoundName { get; set; } = string.Empty;
        public int CoreDamageSectionMaxHitPoints { get; set; }
        public bool AnnounceSighting { get; set; }
        public IslandCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

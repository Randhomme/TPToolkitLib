namespace TPToolkitLib.WorldObject.Definitions.AiEntityFactories
{
    public class GunAI : AiEntityDefinition
    {
        public bool IsLobbingGun { get; set; }
        public bool IsMineLayingGun { get; set; }
        public bool IsTorpedoLauncherGun { get; set; }
        public bool IsPointDefenseGun { get; set; }
        public GunAI(string type) : base(type) { }
    }
}

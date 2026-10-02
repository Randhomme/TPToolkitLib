namespace TPToolkitLib.WorldObject.Definitions.CustomInfoFactories
{
    public class StarMortarCustomInfoFactory : BulletCustomInfoFactory
    {
        public float Magnitude { get; set; }
        public float MaxRadius { get; set; }
        public float Duration { get; set; }
        public StarMortarCustomInfoFactory(string type) : base(type)
        {
        }
    }
}

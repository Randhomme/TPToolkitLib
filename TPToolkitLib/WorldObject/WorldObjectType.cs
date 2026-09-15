using TPToolkitLib.WorldObject.Definitions;

namespace TPToolkitLib.WorldObject
{
    public class WorldObjectType
    {
        public string Type { get; set; } = string.Empty;
        public AiEntityDefinition? AiEntityDefinition { get; set; }
        public RenderEntityDefinition? RenderEntityDefinition { get; set; }
        public PhysicsDefinition? PhysicsDefinition { get; set; }
        public CollisionDefinition? CollisionDefinition { get; set; }
        public CustomInfoDefinition? CustomInfoDefinition { get; set; }

        public override string ToString()
        {
            return Type;
        }
    }
}

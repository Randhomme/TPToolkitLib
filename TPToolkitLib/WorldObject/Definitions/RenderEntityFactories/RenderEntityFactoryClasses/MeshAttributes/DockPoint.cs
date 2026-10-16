using TPToolkitLib.MeshScene.Classes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class DockPoint : MeshSubAttribute
    {
        public string AnimationName { get; set; } = string.Empty;
        public MsbAnimation? Animation { get; set; }
    }
}

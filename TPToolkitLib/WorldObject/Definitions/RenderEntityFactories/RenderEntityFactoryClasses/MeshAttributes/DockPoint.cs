using TPToolkitLib.MeshScene.Classes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class DockPoint : MeshSubAttribute
    {
        public string AnimationName { get; set; } = string.Empty;
        private MsbAnimation? animation;
        public MsbAnimation? Animation
        {
            get => animation;
            set
            {
                animation = value;
                if (value != null)
                    AnimationName = value.Name;
                else
                    AnimationName = string.Empty;
            }
        }
    }
}

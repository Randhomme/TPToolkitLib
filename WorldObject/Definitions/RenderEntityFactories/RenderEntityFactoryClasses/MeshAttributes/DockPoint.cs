using TPToolkitLib.MeshScene.Classes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class DockPoint : MeshSubAttribute
    {
        public string AnimationName { get; set; } = string.Empty;
        public MsbAnimation? Animation
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    AnimationName = value.Name;
                else
                    AnimationName = string.Empty;
            }
        }
    }
}

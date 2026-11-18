using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Interfaces;
using TPToolkitLib.MeshScene.Classes;
using TPToolkitLib.Tools;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories
{
    public class RenderEntityFactory : RenderEntityDefinition, IDependencyResolvable, IValidatableObject
    {
        public bool Distant { get; set; }
        public bool Visible { get; set; }
        public IList<MeshAttribute> MeshAttributes { get; } = [];
        public float RotationSpeed { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The MeshSceneName is required, and cannot be an empty string.")]
        public string MeshSceneName { get; set; }
        public MsbScene? MeshScene
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    MeshSceneName = value.Name;
                else
                    MeshSceneName = string.Empty;
            }
        }
        [Required(AllowEmptyStrings = false, ErrorMessage = "The RenderEffectName is required, and cannot be an empty string.")]
        public string RenderEffectName { get; set; }
        public RenderEntityFactory(string type) : base(type)
        {
            MeshSceneName = MsbScene.None.Name;
            RenderEffectName = TPGameTool.RenderEffects.FirstOrDefault();
        }

        public void ResolveDependency()
        {
            if (string.Equals(MeshSceneName, MsbScene.None.Name, StringComparison.Ordinal))
                MeshScene = MsbScene.None;
            else
                MeshScene = TPGameTool.MeshScenes.FirstOrDefault((ms) => ms.Name.Equals(MeshSceneName, StringComparison.OrdinalIgnoreCase));
            if (MeshScene != null)
            {
                for (int i = 0; i < MeshAttributes.Count; i++)
                {
                    var meshAttribute = MeshAttributes[i];
                    meshAttribute.Mesh = GetMsbElementFromMsbScene(MeshScene, meshAttribute.MeshName);
                    for (int j = 0; j < meshAttribute.MeshSubAttributes.Count; j++)
                    {
                        var meshSubAttribute = meshAttribute.MeshSubAttributes[j];
                        switch (meshSubAttribute)
                        {
                            case DockPoint dockPoint:
                                dockPoint.Animation = MeshScene.MsbAnimations.FirstOrDefault((msba) => string.Equals(msba.Name, dockPoint.AnimationName));
                                break;
                            case DamageSection damageSection:
                                for(int k = 0; k < damageSection.Adjacents.Count; k++)
                                {
                                    var adjacentName = damageSection.Adjacents[k];
                                    var adjacentElement = GetMsbMeshFromMsbScene(MeshScene, adjacentName);
                                    if (adjacentElement != null)
                                        damageSection.AdjacentElements.Add(adjacentElement);
                                }
                                break;
                        }
                    }
                }
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            int i = 0;
            try
            {
                for (i = 0; i < MeshAttributes.Count; i++)
                {
                    var meshAttribute = MeshAttributes[i];
                    Validator.ValidateObject(meshAttribute, new(meshAttribute), true);
                }
            }
            catch (Exception ex)
            {
                return [new($"MeshAttribute {i + 1}: {ex.Message}")];
            }
            return [ValidationResult.Success];
        }

        private MsbElement? GetMsbElementFromMsbScene(MsbScene msbScene, string elementName)
        {
            for (int i = 0; i < msbScene.MsbNodes.Count; i++)
            {
                var msbNode = msbScene.MsbNodes[i];
                if (msbNode.Name.Equals(elementName, StringComparison.OrdinalIgnoreCase))
                    return msbNode;
            }
            for (int i = 0; i < msbScene.MsbMeshes.Count; i++)
            {
                var msbMesh = msbScene.MsbMeshes[i];
                if (msbMesh.Name.Equals(elementName, StringComparison.OrdinalIgnoreCase))
                    return msbMesh;
            }
            return null;
        }

        private MsbMesh? GetMsbMeshFromMsbScene(MsbScene msbScene, string meshName)
        {
            for (int i = 0; i < msbScene.MsbMeshes.Count; i++)
            {
                var msbMesh = msbScene.MsbMeshes[i];
                if (msbMesh.Name.Equals(meshName, StringComparison.OrdinalIgnoreCase))
                    return msbMesh;
            }
            return null;
        }
    }
}

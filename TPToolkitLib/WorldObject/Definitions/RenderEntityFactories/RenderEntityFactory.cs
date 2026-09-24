using System;
using System.Collections.Generic;
using System.Linq;
using TPToolkitLib.Interfaces;
using TPToolkitLib.MeshScene.Classes;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories
{
    public class RenderEntityFactory : RenderEntityDefinition, IDependencyResolvable
    {
        public bool Distant { get; set; }
        public bool Visible { get; set; }
        public IList<MeshAttribute> MeshAttributes { get; }
        public float RotationSpeed { get; set; }
        public string MeshSceneName { get; set; } = string.Empty;
        public MsbScene? MsbScene { get; set; }
        public string RenderEffectName { get; set; } = string.Empty;
        public RenderEntityFactory(string type) : base(type)
        {
            MeshAttributes = [];
        }

        public void ResolveDependency()
        {
            MsbScene = TPGameTool.MeshScenes.FirstOrDefault((ms) => ms.Name.Equals(MeshSceneName, StringComparison.OrdinalIgnoreCase));
            if (MsbScene != null)
            {
                for (int i = 0; i < MeshAttributes.Count; i++)
                {
                    var meshAttribute = MeshAttributes[i];
                    meshAttribute.MsbElement = GetMsbElementFromMsbScene(MsbScene, meshAttribute.MeshName);
                    for (int j = 0; j < meshAttribute.MeshSubAttributes.Count; j++)
                    {
                        var meshSubAttribute = meshAttribute.MeshSubAttributes[j];
                        switch (meshSubAttribute)
                        {
                            //case GunPlacement gunPlacement:
                            //    break;
                            case DamageSection damageSection:
                                for(int k = 0; k < damageSection.Adjacents.Count; k++)
                                {
                                    var adjacentName = damageSection.Adjacents[k];
                                    var adjacentElement = GetMsbElementFromMsbScene(MsbScene, adjacentName);
                                    if (adjacentElement != null)
                                        damageSection.AdjacentElements.Add(adjacentElement);
                                }
                                break;
                            default:
                                throw new NotImplementedException($"MeshSubAttribute type {meshSubAttribute.GetType().Name} is not implemented.");
                        }
                    }
                }
            }
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
    }
}

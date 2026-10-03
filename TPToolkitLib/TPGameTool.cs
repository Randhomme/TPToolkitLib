using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPToolkitLib.MeshScene.Classes;
using TPToolkitLib.WorldObject;

namespace TPToolkitLib
{
    public static class TPGameTool
    {
        private static readonly List<WorldObjectType> worldObjects = [];
        private static readonly List<MsbScene> meshScenes = [];
        private static readonly List<string> effects = [];
        private static readonly List<string> meshes = [];
        private static readonly List<string> renderEffects = [];
        public static IReadOnlyList<WorldObjectType> WorldObjects { get => worldObjects; }
        public static IReadOnlyList<MsbScene> MeshScenes { get => meshScenes; }
        public static IReadOnlyList<string> Effects { get => effects; }
        public static IReadOnlyList<string> Meshes { get => meshes; }
        public static IReadOnlyList<string> RenderEffects { get => renderEffects; }

        public static void LoadTPGameFolder(string tpGameFolderpath)
        {

        }

        private static void LoadWorldObjects(string worldObjectFolderPath)
        {

        }

        private static void LoadMeshScenes(string meshSceneFolderPath)
        {
            var msbFiles = Directory.GetFiles(meshSceneFolderPath, "*.msb");
            for (int i = 0; i < msbFiles.Length; i++)
            {
                var msbFile = msbFiles[i];
                meshScenes.Add(MsbTool.MeshSceneFromMsb(msbFile));
            }
        }

        private static void LoadEffects(string effectFolderPath)
        {
            var effectFiles = Directory.GetFiles(effectFolderPath, "*.eft");
            for (int i = 0; i < effectFiles.Length; i++)
            {
                var effectFile = effectFiles[i];
                effects.Add(Path.GetFileNameWithoutExtension(effectFile));
            }
        }

        private static void LoadMeshes(string meshFolderPath)
        {
            var meshFiles = Directory.GetFiles(meshFolderPath, "*.mdb");
            for (int i = 0; i < meshFiles.Length; i++)
            {
                var meshFile = meshFiles[i];
                meshes.Add(Path.GetFileNameWithoutExtension(meshFile));
            }
        }

        private static void LoadRenderEffects(string renderEffectFolderPath)
        {
            var renderEffectFiles = Directory.GetFiles(renderEffectFolderPath, "*.ret");
            for (int i = 0; i < renderEffectFiles.Length; i++)
            {
                var renderEffectFile = renderEffectFiles[i];
                renderEffects.Add(Path.GetFileNameWithoutExtension(renderEffectFile));
            }
        }
    }
}

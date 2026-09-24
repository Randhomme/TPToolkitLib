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
        public static IList<WorldObjectType> WorldObjects { get; } = [];
        public static IList<MsbScene> MeshScenes { get; } = [];
        public static IList<string> Effects { get; } = [];
        public static IList<string> Meshes { get; } = [];
        public static IList<string> RenderEffects { get; } = [];

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
                MeshScenes.Add(MsbTool.MeshSceneFromMsb(msbFile));
            }
        }

        private static void LoadEffects(string effectFolderPath)
        {
            var effectFiles = Directory.GetFiles(effectFolderPath, "*.eft");
            for (int i = 0; i < effectFiles.Length; i++)
            {
                var effectFile = effectFiles[i];
                Effects.Add(Path.GetFileNameWithoutExtension(effectFile));
            }
        }

        private static void LoadMeshes(string meshFolderPath)
        {
            var meshFiles = Directory.GetFiles(meshFolderPath, "*.mdb");
            for (int i = 0; i < meshFiles.Length; i++)
            {
                var meshFile = meshFiles[i];
                Meshes.Add(Path.GetFileNameWithoutExtension(meshFile));
            }
        }

        private static void LoadRenderEffects(string renderEffectFolderPath)
        {
            var renderEffectFiles = Directory.GetFiles(renderEffectFolderPath, "*.ret");
            for (int i = 0; i < renderEffectFiles.Length; i++)
            {
                var renderEffectFile = renderEffectFiles[i];
                RenderEffects.Add(Path.GetFileNameWithoutExtension(renderEffectFile));
            }
        }
    }
}

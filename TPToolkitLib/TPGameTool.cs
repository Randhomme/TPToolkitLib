using System.Collections.Generic;
using System.IO;
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

        public static void LoadTPGameFolder(string tpGameDirectory, bool ignoreFormatError = false)
        {
            var tpAppSettings = ReadAppSettingsAndStrings(tpGameDirectory);
            LoadEffects(tpAppSettings.EffectsDirectory);
            LoadMeshes(tpAppSettings.MeshesDirectory);
            LoadRenderEffects(tpAppSettings.RenderEffectsDirectory);
            LoadMeshScenes(tpAppSettings.MeshScenesDirectory);
            LoadWorldObjects(tpAppSettings.WorldObjectFilesDirectory, ignoreFormatError);
        }

        private static TPAppSettings ReadAppSettingsAndStrings(string tpGameDirectory)
        {
            var tpAppSettings = new TPAppSettings() { TPGameDirectory = tpGameDirectory };
            ReadAppSettings(tpAppSettings);
            ReadStrings(tpAppSettings);
            return tpAppSettings;
        }

        private static void ReadAppSettings(TPAppSettings tpAppSettings)
        {
            var appSettingsFile = Path.Combine(tpAppSettings.TPGameDirectory, "AppSettings.ini");
            if (File.Exists(appSettingsFile))
            {
                using (var reader = new StreamReader(File.OpenRead(appSettingsFile)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.StartsWith("SOUND DIRECTORY:"))
                        {
                            tpAppSettings.SoundDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("SOUND DIRECTORY:".Length).Trim()); ;
                        }
                        else if (line.StartsWith("WORLD OBJECT FILES DIRECTORY:"))
                        {
                            tpAppSettings.WorldObjectFilesDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("WORLD OBJECT FILES DIRECTORY:".Length).Trim()); ;
                        }
                        else if (line.StartsWith("FLAG TEXTURES:"))
                        {
                            tpAppSettings.FlagTexturesDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("FLAG TEXTURES:".Length).Trim());
                        }
                        else if (line.StartsWith("EFFECTS DIRECTORY:"))
                        {
                            tpAppSettings.EffectsDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("EFFECTS DIRECTORY:".Length).Trim());
                        }
                        else if (line.StartsWith("WORLD FILES:"))
                        {
                            tpAppSettings.WorldFilesDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("WORLD FILES:".Length).Trim());
                        }
                        else if (line.StartsWith("GUI TEXTURES:"))
                        {
                            tpAppSettings.GuiTexturesDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("GUI TEXTURES:".Length).Trim());
                        }
                        else if (line.StartsWith("MESH DATA:"))
                        {
                            tpAppSettings.MeshesDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("MESH DATA:".Length).Trim());
                        }
                        else if (line.StartsWith("MESH DATA SCENE DIRECTORY:"))
                        {
                            tpAppSettings.MeshScenesDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("MESH DATA SCENE DIRECTORY:".Length).Trim());
                        }
                        else if (line.StartsWith("Render Effects Directory:"))
                        {
                            tpAppSettings.RenderEffectsDirectory = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("Render Effects Directory:".Length).Trim());
                        }
                    }
                }
            }
            else
                throw new FileNotFoundException("AppSettings.ini file not found in the selected folder.", appSettingsFile);
        }

        private static void ReadStrings(TPAppSettings tpAppSettings)
        {
            var stringsSettingsFile = Path.Combine(tpAppSettings.TPGameDirectory, "Strings.ini");
            if (File.Exists(stringsSettingsFile))
            {
                using (var reader = new StreamReader(File.OpenRead(stringsSettingsFile)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.StartsWith("Game Header Files:"))
                        {
                            tpAppSettings.GameHeaderFiles = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("Game Header Files:".Length).Trim()); ;
                        }
                        else if (line.StartsWith("Game Strings - English:"))
                        {
                            tpAppSettings.GameStringsEnglish = Path.Combine(tpAppSettings.TPGameDirectory, line.Substring("Game Strings - English:".Length).Trim()); ;
                        }
                    }
                }
            }
            else
                throw new FileNotFoundException("Strings.ini file not found in the selected folder.", stringsSettingsFile);
        }

        private static void LoadWorldObjects(string worldObjectDirectory, bool ignoreFormatError)
        {
            var wotFiles = Directory.GetFiles(worldObjectDirectory, "*.wot");
            for (int i = 0; i < wotFiles.Length; i++)
            {
                var msbFile = wotFiles[i];
                worldObjects.Add(WotTool.WorldObjectFromWot(msbFile, ignoreFormatError));
            }
            WotTool.ResolveDependencies();
        }

        private static void LoadMeshScenes(string meshSceneDirectory)
        {
            var msbFiles = Directory.GetFiles(meshSceneDirectory, "*.msb");
            for (int i = 0; i < msbFiles.Length; i++)
            {
                var msbFile = msbFiles[i];
                meshScenes.Add(MsbTool.MeshSceneFromMsb(msbFile));
            }
        }

        private static void LoadEffects(string effectDirectory)
        {
            var effectFiles = Directory.GetFiles(effectDirectory, "*.eft");
            for (int i = 0; i < effectFiles.Length; i++)
            {
                var effectFile = effectFiles[i];
                effects.Add(Path.GetFileNameWithoutExtension(effectFile));
            }
        }

        private static void LoadMeshes(string meshDirectory)
        {
            var meshFiles = Directory.GetFiles(meshDirectory, "*.mdb");
            for (int i = 0; i < meshFiles.Length; i++)
            {
                var meshFile = meshFiles[i];
                meshes.Add(Path.GetFileNameWithoutExtension(meshFile));
            }
        }

        private static void LoadRenderEffects(string renderEffectDirectory)
        {
            var renderEffectFiles = Directory.GetFiles(renderEffectDirectory, "*.ret");
            for (int i = 0; i < renderEffectFiles.Length; i++)
            {
                var renderEffectFile = renderEffectFiles[i];
                renderEffects.Add(Path.GetFileNameWithoutExtension(renderEffectFile));
            }
        }
    }
}

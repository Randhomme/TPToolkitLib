using System.Collections.Generic;
using System.IO;
using System.Text;
using TPToolkitLib.MeshScene.Classes;
using TPToolkitLib.TPSettings;
using TPToolkitLib.WorldObject;

namespace TPToolkitLib.Tools
{
    public static class TPGameTool
    {
        private static readonly List<WorldObjectType> worldObjects = [];
        private static readonly List<MsbScene> meshScenes = [];
        private static readonly List<string> effects = [];
        private static readonly List<string> meshes = [];
        private static readonly List<string> renderEffects = [];
        private static readonly List<TPGameHeaderStrings> tpGameHeaderStrings = [];
        internal static TPAppSettings TPAppSettings { get; } = new();
        public static IReadOnlyList<WorldObjectType> WorldObjects { get => worldObjects; }
        public static IReadOnlyList<MsbScene> MeshScenes { get => meshScenes; }
        public static IReadOnlyList<string> Effects { get => effects; }
        public static IReadOnlyList<string> Meshes { get => meshes; }
        public static IReadOnlyList<string> RenderEffects { get => renderEffects; }
        public static IReadOnlyList<TPGameHeaderStrings> TPGameHeaderStrings { get => tpGameHeaderStrings; }

        public static void LoadTPGameFolder(string tpGameDirectory, bool ignoreFormatError = false)
        {
            ReadAppSettingsAndStrings(tpGameDirectory);
            LoadStrings(TPAppSettings.GameStringsEnglish, TPAppSettings.GameHeaderFilesDirectory);
            LoadEffects(TPAppSettings.EffectsDirectory);
            LoadMeshes(TPAppSettings.MeshesDirectory);
            LoadRenderEffects(TPAppSettings.RenderEffectsDirectory);
            LoadMeshScenes(TPAppSettings.MeshScenesDirectory);
            LoadWorldObjects(TPAppSettings.WorldObjectFilesDirectory, ignoreFormatError);
        }

        private static void ReadAppSettingsAndStrings(string tpGameDirectory)
        {
            TPAppSettings.TPGameDirectory = tpGameDirectory;
            ReadAppSettings();
            ReadStrings();
        }

        private static void ReadAppSettings()
        {
            var appSettingsFile = Path.Combine(TPAppSettings.TPGameDirectory, "AppSettings.ini");
            if (File.Exists(appSettingsFile))
            {
                using (var reader = new StreamReader(File.OpenRead(appSettingsFile)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.StartsWith("SOUND DIRECTORY:"))
                        {
                            TPAppSettings.SoundDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("SOUND DIRECTORY:".Length).Trim()); ;
                        }
                        else if (line.StartsWith("WORLD OBJECT FILES DIRECTORY:"))
                        {
                            TPAppSettings.WorldObjectFilesDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("WORLD OBJECT FILES DIRECTORY:".Length).Trim()); ;
                        }
                        else if (line.StartsWith("FLAG TEXTURES:"))
                        {
                            TPAppSettings.FlagTexturesDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("FLAG TEXTURES:".Length).Trim());
                        }
                        else if (line.StartsWith("EFFECTS DIRECTORY:"))
                        {
                            TPAppSettings.EffectsDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("EFFECTS DIRECTORY:".Length).Trim());
                        }
                        else if (line.StartsWith("WORLD FILES:"))
                        {
                            TPAppSettings.WorldFilesDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("WORLD FILES:".Length).Trim());
                        }
                        else if (line.StartsWith("GUI TEXTURES:"))
                        {
                            TPAppSettings.GuiTexturesDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("GUI TEXTURES:".Length).Trim());
                        }
                        else if (line.StartsWith("MESH DATA:"))
                        {
                            TPAppSettings.MeshesDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("MESH DATA:".Length).Trim());
                        }
                        else if (line.StartsWith("MESH DATA SCENE DIRECTORY:"))
                        {
                            TPAppSettings.MeshScenesDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("MESH DATA SCENE DIRECTORY:".Length).Trim());
                        }
                        else if (line.StartsWith("MESH TEXTURES:"))
                        {
                            TPAppSettings.MeshTexturesDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("MESH TEXTURES:".Length).Trim());
                        }
                        else if (line.StartsWith("Render Effects Directory:"))
                        {
                            TPAppSettings.RenderEffectsDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("Render Effects Directory:".Length).Trim());
                        }
                    }
                }
            }
            else
                throw new FileNotFoundException("AppSettings.ini file not found in the selected folder.", appSettingsFile);
        }

        private static void ReadStrings()
        {
            var stringsSettingsFile = Path.Combine(TPAppSettings.TPGameDirectory, "Strings.ini");
            if (File.Exists(stringsSettingsFile))
            {
                using (var reader = new StreamReader(File.OpenRead(stringsSettingsFile)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.StartsWith("Game Header Files:"))
                        {
                            TPAppSettings.GameHeaderFilesDirectory = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("Game Header Files:".Length).Trim()); ;
                        }
                        else if (line.StartsWith("Game Strings - English:"))
                        {
                            TPAppSettings.GameStringsEnglish = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("Game Strings - English:".Length).Trim()); ;
                        }
                    }
                }
            }
            else
                throw new FileNotFoundException("Strings.ini file not found in the selected folder.", stringsSettingsFile);
        }

        private static void LoadStrings(string gameStringsEnglish, string gameHeaderFilesDirectory)
        {
            var gameStrings = LoadBaseStrings(gameStringsEnglish);
            LoadGameHeaderFiles(gameHeaderFilesDirectory, gameStrings);
        }

        private static Dictionary<string, string> LoadBaseStrings(string gameStringsEnglish)
        {
            var gameStrings = new Dictionary<string, string>();
            if (File.Exists(gameStringsEnglish))
            {
                using (var reader = new StreamReader(File.OpenRead(gameStringsEnglish), Encoding.GetEncoding("Windows-1252")))
                {
                    string key, value;
                    while ((key = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(key) || key.StartsWith("\"") || key.EndsWith("\"") || key.StartsWith("//"))
                            continue;
                        if ((value = reader.ReadLine()) != null && (value = value.Trim('"')) != null)
                            gameStrings[key] = value;
                        else
                            gameStrings[key] = key;
                    }
                }
            }
            else
                throw new FileNotFoundException($"Game strings file '{gameStringsEnglish}' does not exist.");
            return gameStrings;
        }

        private static void LoadGameHeaderFiles(string gameHeaderFilesDirectory, Dictionary<string, string> gameStrings)
        {
            var gameHeaderFiles = Directory.GetFiles(gameHeaderFilesDirectory, "*.h");
            for (int i = 0; i < gameHeaderFiles.Length; i++)
            {
                var gameHeaderFile = gameHeaderFiles[i];
                var tpghs = new TPGameHeaderStrings() { GameHeaderFile = Path.GetFileName(gameHeaderFile) };
                using (var reader = new StreamReader(File.OpenRead(gameHeaderFile)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.StartsWith("#define"))
                        {
                            var key = line.Substring(8).Split(' ')[0]; // 8 is the length of "#define "
                            if (gameStrings.TryGetValue(key, out var valueString))
                            {
                                tpghs.Strings[key] = valueString;
                            }
                        }
                    }
                    
                }
                tpGameHeaderStrings.Add(tpghs);
            }
        }

        private static void LoadWorldObjects(string worldObjectDirectory, bool ignoreFormatError)
        {
            var wotFiles = Directory.GetFiles(worldObjectDirectory, "*.wot");
            for (int i = 0; i < wotFiles.Length; i++)
            {
                var wotFile = wotFiles[i];
                worldObjects.Add(WotTool.WorldObjectFromWot(wotFile, ignoreFormatError));
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

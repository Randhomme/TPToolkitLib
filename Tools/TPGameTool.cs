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
        private static readonly List<string> effects = [];
        private static readonly List<string> hudEntries = [];
        private static readonly List<string> meshes = [];
        private static readonly List<MsbScene> meshScenes = [];
        private static readonly List<string> soundDialogues = [];
        private static readonly List<string> soundEffects = [];
        private static readonly List<string> soundMusics = [];
        private static readonly List<string> renderEffects = [];
        private static readonly List<TPGameHeaderStrings> tpGameHeaderStrings = [];
        private static readonly List<WorldObjectType> worldObjects = [];
        internal static TPAppSettings TPAppSettings { get; } = new();
        public static IReadOnlyList<string> Effects { get => effects; }
        public static IReadOnlyList<string> HudEntries { get => hudEntries; }
        public static IReadOnlyList<string> Meshes { get => meshes; }
        public static IReadOnlyList<MsbScene> MeshScenes { get => meshScenes; }
        public static IReadOnlyList<string> RenderEffects { get => renderEffects; }
        public static IReadOnlyList<string> SoundDialogues { get => soundDialogues; }
        public static IReadOnlyList<string> SoundEffects { get => soundEffects; }
        public static IReadOnlyList<string> SoundMusics { get => soundMusics; }
        public static IReadOnlyList<TPGameHeaderStrings> TPGameHeaderStrings { get => tpGameHeaderStrings; }
        public static IReadOnlyList<WorldObjectType> WorldObjects { get => worldObjects; }

        public static void LoadTPGameFolder(string tpGameDirectory, bool ignoreFormatError = false)
        {
            ReadAppSettingsAndStrings(tpGameDirectory);
            LoadStrings(TPAppSettings.GameStringsEnglish, TPAppSettings.GameHeaderFilesDirectory);
            LoadSoundDialogues($"{TPAppSettings.SoundDirectory}/Dialogue");
            LoadSoundEffects($"{TPAppSettings.SoundDirectory}/SoundEffects");
            LoadSoundMusics($"{TPAppSettings.SoundDirectory}/Music");
            LoadHudEntries(TPAppSettings.HUDSettingsFileName);
            LoadEffects(TPAppSettings.EffectsDirectory);
            LoadMeshes(TPAppSettings.MeshesDirectory);
            LoadRenderEffects(TPAppSettings.RenderEffectsDirectory);
            LoadMeshScenes(TPAppSettings.MeshScenesDirectory);
            LoadWorldObjects(TPAppSettings.WorldObjectFilesDirectory, ignoreFormatError);
        }

        public static void ReloadTPGameFolder(string tpGameDirectory, bool ignoreFormatError = false)
        {
            ClearLoadedData();
            LoadTPGameFolder(tpGameDirectory, ignoreFormatError);
        }

        public static void ClearLoadedData()
        {
            worldObjects.Clear();
            meshScenes.Clear();
            effects.Clear();
            meshes.Clear();
            renderEffects.Clear();
            hudEntries.Clear();
            for (int i = 0; i < tpGameHeaderStrings.Count; i++)
                tpGameHeaderStrings[i].Strings.Clear();
            tpGameHeaderStrings.Clear();
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
                        else if (line.StartsWith("HUD Settings FileName:"))
                        {
                            TPAppSettings.HUDSettingsFileName = Path.Combine(TPAppSettings.TPGameDirectory, line.Substring("HUD Settings FileName:".Length).Trim());
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

        private static void LoadGameHeaderFiles(string directory, Dictionary<string, string> gameStrings)
        {
            var files = Directory.GetFiles(directory, "*.h");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var tpghs = new TPGameHeaderStrings() { GameHeaderFile = Path.GetFileName(file) };
                using (var reader = new StreamReader(File.OpenRead(file)))
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

        private static void LoadWorldObjects(string directory, bool ignoreFormatError)
        {
            var files = Directory.GetFiles(directory, "*.wot");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                worldObjects.Add(WotTool.WorldObjectFromWotNoResolveDependencies(file, ignoreFormatError));
            }
            WotTool.ResolveDependencies();
        }

        private static void LoadMeshScenes(string directory)
        {
            var files = Directory.GetFiles(directory, "*.msb");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                meshScenes.Add(MsbTool.MeshSceneFromMsb(file));
            }
        }

        private static void LoadEffects(string directory)
        {
            var files = Directory.GetFiles(directory, "*.eft");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                effects.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        private static void LoadMeshes(string directory)
        {
            var files = Directory.GetFiles(directory, "*.mdb");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                meshes.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        private static void LoadRenderEffects(string directory)
        {
            var files = Directory.GetFiles(directory, "*.ret");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                renderEffects.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        private static void LoadSoundDialogues(string directory)
        {
            var files = Directory.GetFiles(directory, "*.ogg");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                soundDialogues.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        private static void LoadSoundEffects(string directory)
        {
            var files = Directory.GetFiles(directory, "*.ogg");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                soundEffects.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        private static void LoadSoundMusics(string directory)
        {
            var files = Directory.GetFiles(directory, "*.ogg");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                soundMusics.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        private static void LoadHudEntries(string hudFilePath)
        {
            using (var reader = new StreamReader(File.OpenRead(hudFilePath)))
            {
                for (int i = 0; i < 185; i++)
                {
                    if (!reader.EndOfStream)
                        reader.ReadLine(); // Skip the first 185 lines
                }
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    // skip one line
                    if (!reader.EndOfStream)
                        reader.ReadLine();
                    if (!reader.EndOfStream)
                    {
                        var line2 = reader.ReadLine();
                        if (line2.Trim().StartsWith("TextureName"))
                        {
                            var lineSplit = line.Split(' ');
                            if (lineSplit.Length > 1)
                            {
                                // Extract the face texture name
                                var hudEntry = lineSplit[1];
                                if (!string.IsNullOrEmpty(hudEntry))
                                {
                                    hudEntries.Add(hudEntry);
                                }
                            }
                            // skip another 24 lines
                            for (int j = 0; j < 24; j++)
                            {
                                if (!reader.EndOfStream)
                                    reader.ReadLine();
                            }
                        }
                    }
                }
            }
        }
    }
}

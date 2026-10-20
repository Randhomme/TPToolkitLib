using System.Collections.Generic;

namespace TPToolkitLib.TPSettings
{
    /// <summary>
    /// Links a game header file to a string dictionnary, containing the strings in the game header file
    /// </summary>
    public class TPGameHeaderStrings
    {
        public string GameHeaderFile { get; set; } = string.Empty;
        public Dictionary<string, string> Strings { get; } = [];
        public override string ToString() => GameHeaderFile;
    }
}

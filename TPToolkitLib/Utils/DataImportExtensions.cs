using System.IO;
using TPToolkitLib.Exceptions;

namespace TPToolkitLib.Utils
{
    public static class DataImportExtensions
    {
        public static bool ReadAndParseBool(this StreamReader reader, string prefix, bool ignoreFormatError = false)
        {
            var line = reader.ReadLine().Trim();
            return ignoreFormatError ? TryParseBool(line, prefix) : ParseBool(line, prefix);
        }

        public static bool ParseBool(string line, string prefix)
        {
            var valueString = line.GetSubstring(prefix);
            if (bool.TryParse(valueString, out var value))
                return value;
            else
                throw new TPException($"'{valueString}' is not a valid bool in '{line}'.");
        }

        public static bool TryParseBool(string line, string prefix)
        {
            bool.TryParse(line.GetSubstring(prefix), out var value);
            return value;
        }

        public static float ReadAndParseFloat(this StreamReader reader, string prefix, bool ignoreFormatError = false)
        {
            var line = reader.ReadLine().Trim();
            return ignoreFormatError ? TryParseFloat(line, prefix) : ParseFloat(line, prefix);
        }

        public static float ParseFloat(string line, string prefix)
        {
            var valueString = line.GetSubstring(prefix);
            if (float.TryParse(valueString, out var value))
                return value;
            else
                throw new TPException($"'{valueString}' is not a valid float in '{line}'.");
        }

        public static float TryParseFloat(string line, string prefix)
        {
            float.TryParse(line.GetSubstring(prefix), out var value);
            return value;
        }

        private static string GetSubstring(this string str, string val)
        {
            if (str.StartsWith(val))
                return str.Substring(val.Length);
            else
                throw new TPException($"'{val.Trim()}' not found in '{str}'");
        }
    }
}

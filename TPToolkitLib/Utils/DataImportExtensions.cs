using System.IO;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using TPToolkitLib.Exceptions;

namespace TPToolkitLib.Utils
{
    public static class DataImportExtensions
    {
        public static string ReadString(this StreamReader reader, string prefix)
        {
            var line = reader.ReadLine().Trim();
            return line.GetSubstring(prefix).Trim('\'');
        }

        public static bool ReadAndParseBool(this StreamReader reader, string prefix, bool ignoreFormatError = false)
        {
            var line = reader.ReadLine().Trim();
            return ignoreFormatError ? TryParseBool(line, prefix) : ParseBool(line, prefix);
        }

        public static float ReadAndParseFloat(this StreamReader reader, string prefix, bool ignoreFormatError = false)
        {
            var line = reader.ReadLine().Trim();
            return ignoreFormatError ? TryParseFloat(line, prefix) : ParseFloat(line, prefix);
        }

        public static int ReadAndParseInt(this StreamReader reader, string prefix, bool ignoreFormatError = false)
        {
            var line = reader.ReadLine().Trim();
            return ignoreFormatError ? TryParseInt(line, prefix) : ParseInt(line, prefix);
        }

        public static Vector3 ReadAndParseVector3(this StreamReader reader, string prefix, bool ignoreFormatError = false)
        {
            var line = reader.ReadLine().Trim();
            return ignoreFormatError ? TryParseVector3(line, prefix) : ParseVector3(line, prefix);
        }

        public static bool ParseBool(string line, string prefix)
        {
            var valueString = line.GetSubstring(prefix);
            if (bool.TryParse(valueString, out var value))
                return value;
            else
                throw new TPException($"'{valueString}' is not a valid bool in '{line}'.");
        }

        public static float ParseFloat(string line, string prefix)
        {
            var valueString = line.GetSubstring(prefix);
            if (float.TryParse(valueString, out var value))
                return value;
            else
                throw new TPException($"'{valueString}' is not a valid float in '{line}'.");
        }

        public static int ParseInt(string line, string prefix)
        {
            var valueString = line.GetSubstring(prefix);
            if (int.TryParse(valueString, out var value))
                return value;
            else
                throw new TPException($"'{valueString}' is not a valid int in '{line}'.");
        }

        public static Vector3 ParseVector3(string line, string prefix)
        {
            var valueString = line.GetSubstring(prefix).Trim('(', ')');
            var values = valueString.Split(',');
            if (float.TryParse(values[0], out var x) && float.TryParse(values[1], out var y) && float.TryParse(values[2], out var z))
                return new Vector3(x, y, z);
            else
                throw new TPException($"'{valueString}' is not a valid Vector3 in '{line}'.");
        }

        public static bool TryParseBool(string line, string prefix)
        {
            bool.TryParse(line.GetSubstring(prefix), out var value);
            return value;
        }

        public static float TryParseFloat(string line, string prefix)
        {
            float.TryParse(line.GetSubstring(prefix), out var value);
            return value;
        }

        public static int TryParseInt(string line, string prefix)
        {
            int.TryParse(line.GetSubstring(prefix), out var value);
            return value;
        }

        public static Vector3 TryParseVector3(string line, string prefix)
        {
            var valueString = line.GetSubstring(prefix).Trim('(', ')');
            var values = valueString.Split(',');
            float.TryParse(values[0], out var x);
            float.TryParse(values[1], out var y);
            float.TryParse(values[2], out var z);
            return new Vector3(x, y, z);
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

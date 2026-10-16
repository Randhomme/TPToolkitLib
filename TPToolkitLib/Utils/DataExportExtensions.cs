using System;
using System.IO;

namespace TPToolkitLib.Utils
{
    public static class DataExportExtensions
    {
        private static ulong currentLineNumber = 0; // If you write a section with more line than ulong.MaxValue, huh ...

        public static void WriteSection(this StreamWriter writer, string sectionName, Action<int> action, int level = 0)
        {
            if (level == 0) currentLineNumber = 0; // Reset the line count when possible
            writer.Flush();

            var pos = writer.BaseStream.Position;

            writer.WriteLineLevel($"00000000 {sectionName}", level);
            writer.WriteLineLevel("{", level);

            var lineNumberBefore = currentLineNumber;
            action.Invoke(level + 1);
            var sectionLineCount = currentLineNumber - lineNumberBefore;

            writer.WriteLineLevel("}", level);

            // Go back to write the correct number of line for the section
            writer.Flush();
            writer.BaseStream.Seek(pos, SeekOrigin.Begin);
            writer.WriteLineLevel($"{sectionLineCount:D8} {sectionName}", level);
            currentLineNumber--; // It's not a new line
            writer.Flush();
            writer.BaseStream.Seek(0, SeekOrigin.End);
        }

        public static void WriteSection(this StreamWriter writer, string sectionName, int level = 0)
        {
            writer.WriteLineLevel($"00000000 {sectionName}", level);
            writer.WriteLineLevel("{", level);
            writer.WriteLineLevel("}", level);
        }

        public static void WriteLineLevel(this StreamWriter writer, string line, int level)
        {
            string tabs = "";
            for (int i = 0; i < level; i++)
            {
                tabs += "\t";
            }
            writer.WriteLine(tabs + line);
            currentLineNumber++;
        }

        public static void WriteLevel(this StreamWriter writer, string line, int level)
        {
            string tabs = "";
            for (int i = 0; i < level; i++)
            {
                tabs += "\t";
            }
            writer.Write(tabs + line);
        }
    }
}

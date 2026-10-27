using System;
using System.IO;

namespace TPToolkitLib.Utils
{
    public class LineStreamReader : IDisposable
    {
        private readonly StreamReader reader;
        public int CurrentLine { get; private set; } = 0;

        public LineStreamReader(Stream stream)
        {
            reader = new(stream);
        }

        public string ReadLine()
        {
            var line = reader.ReadLine();
            CurrentLine += 1;
            return line;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}

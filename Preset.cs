using System;

namespace DirReader
{
    internal class Preset
    {
        public enum SearchType
        {
            LineCount = 1,
            FileCount = 2
        }
        public string ExtensionType { get; private set; }
        public string Directory { get; private set; }
        public SearchType Type { get; private set; }

        public Preset(string extensionType, string directory, SearchType type)
        {
            ExtensionType = extensionType;
            Directory = directory;
            Type = type;
        }
    }
}

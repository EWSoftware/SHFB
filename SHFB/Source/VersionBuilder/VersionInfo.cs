namespace Microsoft.Ddue.Tools
{
    internal class VersionInfo
    {
        // Fields
        public string File { get; private set; }
        public string Group { get; private set; }
        public string Name { get; private set; }
        public bool RipOldApis { get; private set; }

        // Methods
        public VersionInfo(string name, string group, string file, bool ripOldApis)
        {
            this.Name = name;
            this.Group = group;
            this.File = file;
            this.RipOldApis = ripOldApis;
        }
    }
}

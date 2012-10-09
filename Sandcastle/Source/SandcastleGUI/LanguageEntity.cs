using System.Runtime.InteropServices;

namespace SandcastleGui
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LanguageEntity
    {
        public int ID;
        public string Name;
        public LanguageEntity(int id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}


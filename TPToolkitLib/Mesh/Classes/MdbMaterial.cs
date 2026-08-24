using System.IO;

namespace TPToolkitLib.Mesh.Classes
{
    public class MdbMaterial
    {
        public string MaterialName { get; set; }
        public string TextureName { get; set; }

        public MdbMaterial()
        {
            MaterialName = TextureName = string.Empty;
        }

        public MdbMaterial(string textureName)
        {
            MaterialName = GetMaterialNameFromTextureName(textureName);
            TextureName = textureName;
        }

        public MdbMaterial(string materialName, string textureName)
        {
            MaterialName = materialName;
            TextureName = textureName;
        }

        private static string GetMaterialNameFromTextureName(string textureName)
        {
            var separator = new char[] { ' ', ';', ',', '+', '\r', '\t', '\n' };
            return Path.GetFileNameWithoutExtension(string.Join("_", textureName.Split(separator)));
        }

        public override string ToString()
        {
            return MaterialName;
        }
    }
}

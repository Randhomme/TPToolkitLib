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

        public MdbMaterial(string materialName, string textureName)
        {
            MaterialName = materialName;
            TextureName = textureName;
        }

        public override string ToString()
        {
            return MaterialName;
        }
    }
}

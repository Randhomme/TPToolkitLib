using System.IO;
using TPToolkitLib.MeshScene.Classes;
using TPToolkitLib.MeshScene.Enums;

namespace TPToolkitLib
{
    public static class MsbTool
    {
        public static MsbScene MeshSceneFromMsb(string msbFilePath)
        {
            return ReadMsb(msbFilePath);
        }

        public static void MeshSceneToMsb(MsbScene msbScene, string msbFilePath)
        {

        }

        private static MsbScene ReadMsb(string msbFilePath)
        {
            var msbScene = new MsbScene();
            using(var msbReader = new BinaryReader(File.OpenRead(msbFilePath)))
            {
                // Skip 16 bytes (mesh scene size)
                msbReader.BaseStream.Seek(16, SeekOrigin.Current);

                // Mesh scene name
                int nameLength = msbReader.ReadInt32(); // 01 00 00 00
                msbScene.Name = new string(msbReader.ReadChars(nameLength));

                // Root element id
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 02 00 00 00
                int id = msbReader.ReadInt32();

                // Nodes - Size
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 03 00 00 00
                int nodesSize = msbReader.ReadInt32();

                // Nodes - Element(s)
                for (int i = 0; i < nodesSize; i++)
                {
                    msbScene.MsbNodes.Add(ReadNodeFromMsb(msbReader));
                }

            }
            return msbScene;
        }

        private static MsbNode ReadNodeFromMsb(BinaryReader msbReader)
        {
            var msbNode = new MsbNode();

            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 04 00 00 00 + node length

            // Id
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 02 00 00 00
            int nodeId = msbReader.ReadInt32();

            // Parent id
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 05 00 00 00
            int nodeParentId = msbReader.ReadInt32();

            // Type (can ignore)
            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 06 00 00 00

            // Name
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 01 00 00 00
            int nodeNameLength = msbReader.ReadInt32();
            msbNode.Name = new string(msbReader.ReadChars(nodeNameLength));

            // Pivot position
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 07 00 00 00
            msbNode.Pivot = new(msbReader.ReadSingle(), msbReader.ReadSingle(), msbReader.ReadSingle());

            // Element (position)
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float x = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float y = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float z = msbReader.ReadSingle();
            msbNode.Position = new(x, y, z);

            // Element (rotation)
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float yaw = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float pitch = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float roll = msbReader.ReadSingle();
            msbNode.Rotation = new(yaw, pitch, roll);

            // Attributes - Size
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 09 00 00 00
            int attributesSize = msbReader.ReadInt32();

            // Attributes - Element
            for (int j = 0; j < attributesSize; j++)
            {
                var msbAttribute = new MsbAttribute();
                msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 0A 00 00 00 + attribute length

                // AttributeName
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 0B 00 00 00
                int attributeNameLength = msbReader.ReadInt32();
                var attributeName = new string(msbReader.ReadChars(nodeNameLength));
                msbAttribute.AttributeName = GetAttributeName(attributeName);

                // DescriptorName
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 0C 00 00 00
                int descriptorNameLength = msbReader.ReadInt32();
                msbAttribute.DescriptorName = new string(msbReader.ReadChars(nodeNameLength));

                msbNode.MsbAttirbutes.Add(msbAttribute);
            }

            return msbNode;
        }

        private static AttributeName GetAttributeName(string attributeName)
        {
            return attributeName switch
            {
                "BoardingEffectPoint" => AttributeName.BoardingEffectPoint,
                "DamageSection" => AttributeName.DamageSection,
                "DockPoint" => AttributeName.DockPoint,
                "EnginePortPlacement" => AttributeName.EnginePortPlacement,
                "FlagAttachmentPoint" => AttributeName.FlagAttachmentPoint,
                "GunMuzzlePlacement" => AttributeName.GunMuzzlePlacement,
                "GunPlacement" => AttributeName.GunPlacement,
                "GunVerticalPivotPlacement" => AttributeName.GunVerticalPivotPlacement,
                "PlayEffect" => AttributeName.PlayEffect,
                "TorpedoHomingPoint" => AttributeName.TorpedoHomingPoint,
                "ToweePoint" => AttributeName.ToweePoint,
                "TowerPoint" => AttributeName.TowerPoint,
                "WakePlacement" => AttributeName.WakePlacement,
                _ => AttributeName.GunPlacement,
            };
        }
    }
}

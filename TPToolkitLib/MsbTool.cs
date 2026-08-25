using System.Collections.Generic;
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
            IList<int> nodeIds = [];
            IList<int> nodeParentIds = [];
            IList<int> meshIds = [];
            IList<int> meshParentIds = [];
            IList<int> boneIds = [];
            IList<int> boneParentIds = [];
            IList<int> nodeMotionIds = [];
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
                    msbScene.MsbNodes.Add(ReadNodeFromMsb(msbReader, nodeIds, nodeParentIds));
                }

                // Meshes - Size
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 0D 00 00 00
                int meshesSize = msbReader.ReadInt32();

                // Meshes - Element(s)
                for (int i = 0; i < meshesSize; i++)
                {
                    msbScene.MsbMeshes.Add(ReadMeshFromMsb(msbReader, meshIds, meshParentIds));
                }

                // Bones - Size
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 0F 00 00 00
                int bonesSize = msbReader.ReadInt32();

                // Bones - Element(s)
                for (int i = 0; i < bonesSize; i++)
                {
                    msbScene.MsbBones.Add(ReadBoneFromMsb(msbReader, boneIds, boneParentIds));
                }

                // Animations - Size
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 13 00 00 00
                int animationsSize = msbReader.ReadInt32();

                // Animations - Element(s)
                for (int i = 0; i < animationsSize; i++)
                {
                    msbScene.MsbAnimations.Add(ReadAnimationFromMsb(msbReader, nodeMotionIds));
                }
            }
            // Set parent for every element and motion
            for (int i = 0; i < msbScene.MsbNodes.Count; i++)
            {
                var msbNode = msbScene.MsbNodes[i];
                msbNode.Parent = GetElementFromId(nodeParentIds[i], msbScene, nodeIds, meshIds, boneIds);
            }
            for (int i = 0; i < msbScene.MsbMeshes.Count; i++)
            {
                var msbMesh = msbScene.MsbMeshes[i];
                msbMesh.Parent = GetElementFromId(meshParentIds[i], msbScene, nodeIds, meshIds, boneIds);
            }
            for (int i = 0; i < msbScene.MsbBones.Count; i++)
            {
                var msbBone = msbScene.MsbBones[i];
                msbBone.Parent = GetElementFromId(boneParentIds[i], msbScene, nodeIds, meshIds, boneIds);
            }
            int nodeMotionCount = 0;
            for (int i = 0; i < msbScene.MsbAnimations.Count; i++)
            {
                var msbAnimation = msbScene.MsbAnimations[i];
                for (int j = 0; j < msbAnimation.MsbMotions.Count; j++)
                {
                    var msbMotion = msbAnimation.MsbMotions[j];
                    msbMotion.MsbElement = GetElementFromId(nodeMotionIds[j + nodeMotionCount], msbScene, nodeIds, meshIds, boneIds);
                }
                nodeMotionCount += msbAnimation.MsbMotions.Count;
            }
            return msbScene;
        }

        private static MsbNode ReadNodeFromMsb(BinaryReader msbReader, IList<int> ids, IList<int> parentIds)
        {
            var msbNode = new MsbNode();

            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 04 00 00 00 + node length

            // Id
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 02 00 00 00
            ids.Add(msbReader.ReadInt32());

            // Parent id
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 05 00 00 00
            parentIds.Add(msbReader.ReadInt32());

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
                var attributeName = new string(msbReader.ReadChars(attributeNameLength));
                msbAttribute.AttributeName = GetAttributeName(attributeName);

                // DescriptorName
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 0C 00 00 00
                int descriptorNameLength = msbReader.ReadInt32();
                msbAttribute.DescriptorName = new string(msbReader.ReadChars(descriptorNameLength));

                msbNode.MsbAttirbutes.Add(msbAttribute);
            }

            return msbNode;
        }

        private static MsbMesh ReadMeshFromMsb(BinaryReader msbReader, IList<int> ids, IList<int> parentIds)
        {
            var msbMesh = new MsbMesh();

            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 0E 00 00 00 + mesh length

            // Id
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 02 00 00 00
            ids.Add(msbReader.ReadInt32());

            // Parent id
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 05 00 00 00
            parentIds.Add(msbReader.ReadInt32());

            // Type (can ignore)
            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 06 00 00 00

            // Name
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 01 00 00 00
            int meshNameLength = msbReader.ReadInt32();
            msbMesh.Name = new string(msbReader.ReadChars(meshNameLength));

            // Pivot position
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 07 00 00 00
            msbMesh.Pivot = new(msbReader.ReadSingle(), msbReader.ReadSingle(), msbReader.ReadSingle());

            // Element (position)
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float x = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float y = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float z = msbReader.ReadSingle();
            msbMesh.Position = new(x, y, z);

            // Element (rotation)
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float yaw = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float pitch = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float roll = msbReader.ReadSingle();
            msbMesh.Rotation = new(yaw, pitch, roll);

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
                var attributeName = new string(msbReader.ReadChars(attributeNameLength));
                msbAttribute.AttributeName = GetAttributeName(attributeName);

                // DescriptorName
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 0C 00 00 00
                int descriptorNameLength = msbReader.ReadInt32();
                msbAttribute.DescriptorName = new string(msbReader.ReadChars(descriptorNameLength));

                msbMesh.MsbAttirbutes.Add(msbAttribute);
            }

            return msbMesh;
        }

        private static MsbBone ReadBoneFromMsb(BinaryReader msbReader, IList<int> ids, IList<int> parentIds)
        {
            var msbBone = new MsbBone();

            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 10 00 00 00 + bone length

            // Id
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 02 00 00 00
            ids.Add(msbReader.ReadInt32());

            // Parent id
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 05 00 00 00
            parentIds.Add(msbReader.ReadInt32());

            // Type (can ignore)
            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 06 00 00 00

            // Name
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 01 00 00 00
            int boneNameLength = msbReader.ReadInt32();
            msbBone.Name = new string(msbReader.ReadChars(boneNameLength));

            // Pivot position
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 07 00 00 00
            msbBone.Pivot = new(msbReader.ReadSingle(), msbReader.ReadSingle(), msbReader.ReadSingle());

            // Element (position)
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float x = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float y = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float z = msbReader.ReadSingle();
            msbBone.Position = new(x, y, z);

            // Element (rotation)
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float yaw = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float pitch = msbReader.ReadSingle();
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 08 00 00 00
            float roll = msbReader.ReadSingle();
            msbBone.Rotation = new(yaw, pitch, roll);

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
                var attributeName = new string(msbReader.ReadChars(attributeNameLength));
                msbAttribute.AttributeName = GetAttributeName(attributeName);

                // DescriptorName
                msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 0C 00 00 00
                int descriptorNameLength = msbReader.ReadInt32();
                msbAttribute.DescriptorName = new string(msbReader.ReadChars(descriptorNameLength));

                msbBone.MsbAttirbutes.Add(msbAttribute);
            }

            // Influence Map Name
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 11 00 00 00
            int influenceMapNameLength = msbReader.ReadInt32();
            msbBone.InfluenceMapName = new string(msbReader.ReadChars(influenceMapNameLength));

            // Rest Length
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 12 00 00 00
            msbBone.RestLength = msbReader.ReadSingle();

            return msbBone;
        }

        private static MsbAnimation ReadAnimationFromMsb(BinaryReader msbReader, IList<int> nodeMotionIds)
        {
            var msbAnimation = new MsbAnimation();

            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 14 00 00 00 + animation length

            // Name
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 01 00 00 00
            int animationNameLength = msbReader.ReadInt32();
            msbAnimation.Name = new string(msbReader.ReadChars(animationNameLength));

            // Duration
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 15 00 00 00
            msbAnimation.Duration = msbReader.ReadSingle();

            // Node Motion Count
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 16 00 00 00
            int nodeMotionCount = msbReader.ReadInt32();

            for (int i = 0; i < nodeMotionCount; i++)
            {
                msbAnimation.MsbMotions.Add(ReadMotionFromMsb(msbReader, nodeMotionIds));
            }

            return msbAnimation;
        }

        private static MsbMotion ReadMotionFromMsb(BinaryReader msbReader, IList<int> nodeMotionIds)
        {
            var msbMotion = new MsbMotion();

            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 17 00 00 00
            nodeMotionIds.Add(msbReader.ReadInt32());

            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 18 00 00 00 + motion length

            ReadChannelFromMsb(msbReader, msbMotion.MsbChannel1);
            ReadChannelFromMsb(msbReader, msbMotion.MsbChannel2);
            ReadChannelFromMsb(msbReader, msbMotion.MsbChannel3);
            ReadChannelFromMsb(msbReader, msbMotion.MsbChannel4);
            ReadChannelFromMsb(msbReader, msbMotion.MsbChannel5);
            ReadChannelFromMsb(msbReader, msbMotion.MsbChannel6);

            return msbMotion;
        }

        private static void ReadChannelFromMsb(BinaryReader msbReader, IList<MsbKeyframe> channel)
        {
            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 19 00 00 00 + channel length

            // Keyframes - Size
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 1A 00 00 00
            int keyframesSize = msbReader.ReadInt32();

            for (int i = 0; i < keyframesSize; i++)
            {
                channel.Add(ReadKeyframeFromMsb(msbReader));
            }
        }

        private static MsbKeyframe ReadKeyframeFromMsb(BinaryReader msbReader)
        {
            var msbKeyframe = new MsbKeyframe();

            msbReader.BaseStream.Seek(8, SeekOrigin.Current); // 1B 00 00 00 + keyframe length

            // Time
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 1C 00 00 00
            msbKeyframe.Time = msbReader.ReadSingle();

            // Value
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 1D 00 00 00
            msbKeyframe.Value = msbReader.ReadSingle();

            // Value
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 1E 00 00 00
            msbKeyframe.Smoothing = msbReader.ReadInt32();

            // Tension
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 1F 00 00 00
            msbKeyframe.Tension = msbReader.ReadSingle();

            // Continuity
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 20 00 00 00
            msbKeyframe.Continuity = msbReader.ReadSingle();

            // Bias
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 21 00 00 00
            msbKeyframe.Bias = msbReader.ReadSingle();

            // Incoming Tangent
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 22 00 00 00
            msbKeyframe.IncomingTangent = msbReader.ReadSingle();

            // Outgoing Tangent
            msbReader.BaseStream.Seek(4, SeekOrigin.Current); // 23 00 00 00
            msbKeyframe.OutgoingTangent = msbReader.ReadSingle();

            return msbKeyframe;
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

        private static MsbElement? GetElementFromId(int id, MsbScene msbScene, IList<int> nodeIds, IList<int> meshIds, IList<int> boneIds)
        {
            for (int i = 0; i < nodeIds.Count; i++)
            {
                if (nodeIds[i] == id)
                {
                    return msbScene.MsbNodes[i];
                }
            }
            for (int i = 0; i < meshIds.Count; i++)
            {
                if (meshIds[i] == id)
                {
                    return msbScene.MsbMeshes[i];
                }
            }
            for (int i = 0; i < boneIds.Count; i++)
            {
                if (boneIds[i] == id)
                {
                    return msbScene.MsbBones[i];
                }
            }
            return null;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text;
using TPToolkitLib.MeshScene.Classes;
using TPToolkitLib.MeshScene.Enums;

namespace TPToolkitLib
{
    public static class MsbTool
    {
        public static MsbScene MeshSceneFromMsbs(string[] msbFilePaths)
        {
            var msbScene = new MsbScene();
            for (int i = 0; i < msbFilePaths.Length; i++)
            {
                var msbFilePath = msbFilePaths[i];
                var tempMsbScene = MeshSceneFromMsb(msbFilePath);
                MergeMeshScenes(msbScene, tempMsbScene);
            }
            return msbScene;
        }

        public static MsbScene MeshSceneFromMsb(string msbFilePath)
        {
            return ReadMsb(msbFilePath);
        }

        public static void ImportMsbSceneFromMsbs(MsbScene msbScene, string[] msbFilePaths)
        {
            for (int i = 0; i < msbFilePaths.Length; i++)
            {
                var msbFilePath = msbFilePaths[i];
                ImportMsbSceneFromMsb(msbScene, msbFilePath);
            }
        }

        public static void ImportMsbSceneFromMsb(MsbScene msbScene, string msbFilePath)
        {
            var tempMsbScene = MeshSceneFromMsb(msbFilePath);
            MergeMeshScenes(msbScene, tempMsbScene);
        }

        public static void MeshSceneToMsb(MsbScene msbScene, string msbFilePath)
        {
            WriteMsbSceneToMsb(msbScene, msbFilePath);
        }

        private static void MergeMeshScenes(MsbScene msbScene1, MsbScene msbScene2)
        {
            msbScene1.Name = msbScene2.Name;
            msbScene1.RootMsbElement = msbScene2.RootMsbElement;
            for (int i = 0; i < msbScene2.MsbNodes.Count; i++)
            {
                msbScene1.MsbNodes.Add(msbScene2.MsbNodes[i]);
            }
            for (int i = 0; i < msbScene2.MsbMeshes.Count; i++)
            {
                msbScene1.MsbMeshes.Add(msbScene2.MsbMeshes[i]);
            }
            for (int i = 0; i < msbScene2.MsbBones.Count; i++)
            {
                msbScene1.MsbBones.Add(msbScene2.MsbBones[i]);
            }
            for (int i = 0; i < msbScene2.MsbAnimations.Count; i++)
            {
                msbScene1.MsbAnimations.Add(msbScene2.MsbAnimations[i]);
            }
        }

        private static MsbScene ReadMsb(string msbFilePath)
        {
            var msbScene = new MsbScene();
            int id = -1;
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
                id = msbReader.ReadInt32();

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

            // Get root element
            msbScene.RootMsbElement = GetElementFromId(id, msbScene, nodeIds, meshIds, boneIds);

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

        private static void WriteMsbSceneToMsb(MsbScene msbScene, string msbFilePath)
        {
            using(var msbWriter = new BinaryWriter(File.Open(msbFilePath, FileMode.Create)))
            {
                msbWriter.Write(0);
                msbWriter.Write(0);
                msbWriter.Write(0);

                // Mesh scene name
                msbWriter.Write(1);
                msbWriter.Write(msbScene.Name.Length);
                msbWriter.Write(Encoding.Default.GetBytes(msbScene.Name));

                // Root element id
                msbWriter.Write(2);
                msbWriter.Write(GetIdFromElement(msbScene.RootMsbElement, msbScene));

                // Nodes - Size
                msbWriter.Write(3);
                msbWriter.Write(msbScene.MsbNodes.Count);

                // Nodes - Element
                for (int i = 0; i < msbScene.MsbNodes.Count; i++)
                {
                    var msbNode = msbScene.MsbNodes[i];
                    WriteMsbNodeToMsb(msbNode, msbScene, i, msbWriter);
                }

                var elementId = msbScene.MsbNodes.Count;

                // Meshes - Size
                msbWriter.Write(13);
                msbWriter.Write(msbScene.MsbMeshes.Count);

                // Meshes - Element
                for (int i = 0; i < msbScene.MsbMeshes.Count; i++)
                {
                    var msbMesh = msbScene.MsbMeshes[i];
                    WriteMsbMeshToMsb(msbMesh, msbScene, i + elementId, msbWriter);
                }

                elementId += msbScene.MsbMeshes.Count;

                // Bones - Size
                msbWriter.Write(15);
                msbWriter.Write(msbScene.MsbBones.Count);

                // Bones - Element
                for (int i = 0; i < msbScene.MsbBones.Count; i++)
                {
                    var msbBone = msbScene.MsbBones[i];
                    WriteMsbBoneToMsb(msbBone, msbScene, i + elementId, msbWriter);
                }

                // Animations - Size
                msbWriter.Write(19);
                msbWriter.Write(msbScene.MsbAnimations.Count);

                // Animations - Element
                for (int i = 0; i < msbScene.MsbAnimations.Count; i++)
                {
                    var msbAnimation = msbScene.MsbAnimations[i];
                    WriteMsbAnimationToMsb(msbAnimation, msbScene, msbWriter);
                }
            }
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

        private static void WriteMsbNodeToMsb(MsbNode msbNode, MsbScene msbScene, int id, BinaryWriter msbWriter)
        {
            msbWriter.Write(4);
            var pos = msbWriter.BaseStream.Position;
            msbWriter.Write(0);

            // ID
            msbWriter.Write(2);
            msbWriter.Write(id);

            // Parent ID
            msbWriter.Write(5);
            msbWriter.Write(GetIdFromElement(msbNode.Parent, msbScene));

            // Type
            msbWriter.Write(6);
            msbWriter.Write(0);

            // Name
            msbWriter.Write(1);
            msbWriter.Write(msbNode.Name.Length);
            msbWriter.Write(Encoding.Default.GetBytes(msbNode.Name));

            // Pivot Position
            msbWriter.Write(7);
            msbWriter.Write(msbNode.Pivot.X);
            msbWriter.Write(msbNode.Pivot.Y);
            msbWriter.Write(msbNode.Pivot.Z);

            // Elements
            msbWriter.Write(8);
            msbWriter.Write(msbNode.Position.X);
            msbWriter.Write(8);
            msbWriter.Write(msbNode.Position.Y);
            msbWriter.Write(8);
            msbWriter.Write(msbNode.Position.Z);
            msbWriter.Write(8);
            msbWriter.Write(msbNode.Rotation.X);
            msbWriter.Write(8);
            msbWriter.Write(msbNode.Rotation.Y);
            msbWriter.Write(8);
            msbWriter.Write(msbNode.Rotation.Z);

            // Attributes - Size
            msbWriter.Write(9);
            msbWriter.Write(msbNode.MsbAttirbutes.Count);

            // Attributes - Element
            for (int i= 0; i < msbNode.MsbAttirbutes.Count; i++)
            {
                WriteMsbAttributeToMsb(msbNode.MsbAttirbutes[i], msbWriter);
            }

            var blockLength = msbWriter.BaseStream.Position - pos - 4;
            msbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            msbWriter.Write((int)blockLength);
            msbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMsbMeshToMsb(MsbMesh msbMesh, MsbScene msbScene, int id, BinaryWriter msbWriter)
        {
            msbWriter.Write(14);
            var pos = msbWriter.BaseStream.Position;
            msbWriter.Write(0);

            // ID
            msbWriter.Write(2);
            msbWriter.Write(id);

            // Parent ID
            msbWriter.Write(5);
            msbWriter.Write(GetIdFromElement(msbMesh.Parent, msbScene));

            // Type
            msbWriter.Write(6);
            msbWriter.Write(1);

            // Name
            msbWriter.Write(1);
            msbWriter.Write(msbMesh.Name.Length);
            msbWriter.Write(Encoding.Default.GetBytes(msbMesh.Name));

            // Pivot Position
            msbWriter.Write(7);
            msbWriter.Write(msbMesh.Pivot.X);
            msbWriter.Write(msbMesh.Pivot.Y);
            msbWriter.Write(msbMesh.Pivot.Z);

            // Elements
            msbWriter.Write(8);
            msbWriter.Write(msbMesh.Position.X);
            msbWriter.Write(8);
            msbWriter.Write(msbMesh.Position.Y);
            msbWriter.Write(8);
            msbWriter.Write(msbMesh.Position.Z);
            msbWriter.Write(8);
            msbWriter.Write(msbMesh.Rotation.X);
            msbWriter.Write(8);
            msbWriter.Write(msbMesh.Rotation.Y);
            msbWriter.Write(8);
            msbWriter.Write(msbMesh.Rotation.Z);

            // Attributes - Size
            msbWriter.Write(9);
            msbWriter.Write(msbMesh.MsbAttirbutes.Count);

            // Attributes - Element
            for (int i = 0; i < msbMesh.MsbAttirbutes.Count; i++)
            {
                WriteMsbAttributeToMsb(msbMesh.MsbAttirbutes[i], msbWriter);
            }

            var blockLength = msbWriter.BaseStream.Position - pos - 4;
            msbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            msbWriter.Write((int)blockLength);
            msbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMsbBoneToMsb(MsbBone msbBone, MsbScene msbScene, int id, BinaryWriter msbWriter)
        {
            msbWriter.Write(16);
            var pos = msbWriter.BaseStream.Position;
            msbWriter.Write(0);

            // ID
            msbWriter.Write(2);
            msbWriter.Write(id);

            // Parent ID
            msbWriter.Write(5);
            msbWriter.Write(GetIdFromElement(msbBone.Parent, msbScene));

            // Type
            msbWriter.Write(6);
            msbWriter.Write(2);

            // Name
            msbWriter.Write(1);
            msbWriter.Write(msbBone.Name.Length);
            msbWriter.Write(Encoding.Default.GetBytes(msbBone.Name));

            // Pivot Position
            msbWriter.Write(7);
            msbWriter.Write(msbBone.Pivot.X);
            msbWriter.Write(msbBone.Pivot.Y);
            msbWriter.Write(msbBone.Pivot.Z);

            // Elements
            msbWriter.Write(8);
            msbWriter.Write(msbBone.Position.X);
            msbWriter.Write(8);
            msbWriter.Write(msbBone.Position.Y);
            msbWriter.Write(8);
            msbWriter.Write(msbBone.Position.Z);
            msbWriter.Write(8);
            msbWriter.Write(msbBone.Rotation.X);
            msbWriter.Write(8);
            msbWriter.Write(msbBone.Rotation.Y);
            msbWriter.Write(8);
            msbWriter.Write(msbBone.Rotation.Z);

            // Attributes - Size
            msbWriter.Write(9);
            msbWriter.Write(msbBone.MsbAttirbutes.Count);

            // Attributes - Element
            for (int i = 0; i < msbBone.MsbAttirbutes.Count; i++)
            {
                WriteMsbAttributeToMsb(msbBone.MsbAttirbutes[i], msbWriter);
            }

            // Influence Map Name
            msbWriter.Write(17);
            msbWriter.Write(msbBone.InfluenceMapName.Length);
            msbWriter.Write(Encoding.Default.GetBytes(msbBone.InfluenceMapName));

            // Rest Length
            msbWriter.Write(18);
            msbWriter.Write(msbBone.RestLength);

            var blockLength = msbWriter.BaseStream.Position - pos - 4;
            msbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            msbWriter.Write((int)blockLength);
            msbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMsbAnimationToMsb(MsbAnimation msbAnimation, MsbScene msbScene, BinaryWriter msbWriter)
        {
            msbWriter.Write(20);
            var pos = msbWriter.BaseStream.Position;
            msbWriter.Write(0);

            // Name
            msbWriter.Write(1);
            msbWriter.Write(msbAnimation.Name.Length);
            msbWriter.Write(Encoding.Default.GetBytes(msbAnimation.Name));

            // Duration
            msbWriter.Write(21);
            msbWriter.Write(msbAnimation.Duration);

            // Node Motion Count
            msbWriter.Write(22);
            msbWriter.Write(msbAnimation.MsbMotions.Count);

            for (int i = 0; i < msbAnimation.MsbMotions.Count; i++)
            {
                var msbMotion = msbAnimation.MsbMotions[i];
                WriteMsbMotionToMsb(msbMotion, msbScene, msbWriter);
            }

            var blockLength = msbWriter.BaseStream.Position - pos - 4;
            msbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            msbWriter.Write((int)blockLength);
            msbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMsbAttributeToMsb(MsbAttribute msbAttribute, BinaryWriter msbWriter)
        {
            msbWriter.Write(10);
            var pos = msbWriter.BaseStream.Position;
            msbWriter.Write(0);

            // Attribute Name
            msbWriter.Write(11);
            var attributeName = msbAttribute.AttributeName.ToString();
            msbWriter.Write(attributeName.Length);
            msbWriter.Write(Encoding.Default.GetBytes(attributeName));

            // Descriptor Name
            msbWriter.Write(12);
            msbWriter.Write(msbAttribute.DescriptorName.Length);
            msbWriter.Write(Encoding.Default.GetBytes(msbAttribute.DescriptorName));

            var blockLength = msbWriter.BaseStream.Position - pos - 4;
            msbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            msbWriter.Write((int)blockLength);
            msbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMsbMotionToMsb(MsbMotion msbMotion, MsbScene msbScene, BinaryWriter msbWriter)
        {
            msbWriter.Write(23);
            msbWriter.Write(GetIdFromElement(msbMotion.MsbElement, msbScene));

            msbWriter.Write(24);
            var pos = msbWriter.BaseStream.Position;
            msbWriter.Write(0);

            WriteMsbChannelToMsb(msbMotion.MsbChannel1, msbWriter);
            WriteMsbChannelToMsb(msbMotion.MsbChannel2, msbWriter);
            WriteMsbChannelToMsb(msbMotion.MsbChannel3, msbWriter);
            WriteMsbChannelToMsb(msbMotion.MsbChannel4, msbWriter);
            WriteMsbChannelToMsb(msbMotion.MsbChannel5, msbWriter);
            WriteMsbChannelToMsb(msbMotion.MsbChannel6, msbWriter);

            var blockLength = msbWriter.BaseStream.Position - pos - 4;
            msbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            msbWriter.Write((int)blockLength);
            msbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMsbChannelToMsb(IList<MsbKeyframe> channel, BinaryWriter msbWriter)
        {
            msbWriter.Write(25);
            var pos = msbWriter.BaseStream.Position;
            msbWriter.Write(0);

            // Keyframes - Size
            msbWriter.Write(26);
            msbWriter.Write(channel.Count);

            // Keyframes - Element
            for (int i = 0; i < channel.Count; i++)
            {
                WriteMsbKeyframeToMsb(channel[i], msbWriter);
            }

            var blockLength = msbWriter.BaseStream.Position - pos - 4;
            msbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            msbWriter.Write((int)blockLength);
            msbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMsbKeyframeToMsb(MsbKeyframe msbKeyframe, BinaryWriter msbWriter)
        {
            msbWriter.Write(27);
            var pos = msbWriter.BaseStream.Position;
            msbWriter.Write(0);

            // Time
            msbWriter.Write(28);
            msbWriter.Write(msbKeyframe.Time);

            // Value
            msbWriter.Write(29);
            msbWriter.Write(msbKeyframe.Value);

            // Smoothing
            msbWriter.Write(30);
            msbWriter.Write(msbKeyframe.Smoothing);

            // Tension
            msbWriter.Write(31);
            msbWriter.Write(msbKeyframe.Tension);

            // Continuity
            msbWriter.Write(32);
            msbWriter.Write(msbKeyframe.Continuity);

            // Bias
            msbWriter.Write(33);
            msbWriter.Write(msbKeyframe.Bias);

            // Incoming Tangent
            msbWriter.Write(34);
            msbWriter.Write(msbKeyframe.IncomingTangent);

            // Outgoing Tangent
            msbWriter.Write(35);
            msbWriter.Write(msbKeyframe.OutgoingTangent);

            var blockLength = msbWriter.BaseStream.Position - pos - 4;
            msbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            msbWriter.Write((int)blockLength);
            msbWriter.BaseStream.Seek(0, SeekOrigin.End);
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
            if (id >= 0)
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
            }
            return null;
        }

        private static int GetIdFromElement(MsbElement? msbElement, MsbScene msbScene)
        {
            if (msbElement != null)
            {
                for (int i = 0; i < msbScene.MsbNodes.Count; i++)
                {
                    var msbNode = msbScene.MsbNodes[i];
                    if (msbElement == msbNode)
                        return i;
                }
                for (int i = 0; i < msbScene.MsbMeshes.Count; i++)
                {
                    var msbMesh = msbScene.MsbMeshes[i];
                    if (msbElement == msbMesh)
                        return i + msbScene.MsbNodes.Count;
                }
                for (int i = 0; i < msbScene.MsbBones.Count; i++)
                {
                    var msbBone = msbScene.MsbBones[i];
                    if (msbElement == msbBone)
                        return i + msbScene.MsbNodes.Count + msbScene.MsbMeshes.Count;
                }
            }
            return -1;
        }
    }
}

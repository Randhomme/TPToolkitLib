using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using TPToolkitLib.Mesh.Classes;
using TPToolkitLib.Utils;

namespace TPToolkitLib
{
    public static class MdbTool
    {
        public static void XMdbTo1Obj(string[] mdbFilePaths, string objFilePath, string textureDirectory, bool lods)
        {
            var meshes = MeshFromMdbs(mdbFilePaths, lods);
            MeshesToObj(meshes, objFilePath, textureDirectory);
        }

        public static IEnumerable<MdbMesh> MeshFromMdbs(string[] mdbFilePaths, bool lods)
        {
            IList<MdbMesh> mdbMeshes = [];
            for (int i = 0; i < mdbFilePaths.Length; i++)
            {
                var mdbFilePath = mdbFilePaths[i];
                mdbMeshes.Add(MeshFromMdb(mdbFilePath, lods));
            }
            return mdbMeshes;
        }

        public static MdbMesh MeshFromMdb(string mdbFilePath, bool lods)
        {
            string groupName = Path.GetFileNameWithoutExtension(mdbFilePath);
            var mesh = new MdbMesh(groupName);
            ReadMdb(mesh, mdbFilePath, lods);
            return mesh;
        }

        public static void MeshesToObj(IEnumerable<MdbMesh> meshes, string objFilePath, string textureDirectory)
        {
            string mtlPath = Path.ChangeExtension(objFilePath, "mtl");
            // Write mtl
            WriteMtarialsToMtl(meshes, mtlPath, textureDirectory);
            // Write obj
            WriteMeshesToObj(meshes, objFilePath, mtlPath);
        }

        private static void ReadMdb(MdbMesh mesh, string mdbFilePath, bool lods)
        {
            using (BinaryReader mdbReader = new BinaryReader(File.OpenRead(mdbFilePath)))
            {
                uint modelCount, modelLength, modelStart, matCount, vCount, tCount;
                //File length stuff
                try
                {
                    //skip 12 bytes (file data block length)
                    mdbReader.BaseStream.Seek(12, SeekOrigin.Begin);
                    //number of model in the mdb (lod)
                    modelCount = mdbReader.ReadUInt32();
                }
                catch
                {
                    throw new Exception("Skipped\nUnable to read the number of model in the file.\n");
                }

                //3d model
                for (int i = 0; i < modelCount; i++)
                {
                    try
                    {
                        //model block length
                        modelLength = mdbReader.ReadUInt32();
                        modelStart = (uint)mdbReader.BaseStream.Position;
                        //skip 4 bytes (model index)
                        mdbReader.BaseStream.Seek(4, SeekOrigin.Current);
                        //vertex count
                        vCount = mdbReader.ReadUInt32();
                    }
                    catch
                    {
                        throw new Exception("Skipped\nUnable to read point count of model number " + i + " in the file.\n");
                    }
                    if (!lods && i > 0) //Skip lods
                    {
                        //Vertexes
                        for (uint j = 0; j < vCount; j++)
                        {
                            mdbReader.BaseStream.Seek(36, SeekOrigin.Current);
                        }
                        tCount = mdbReader.ReadUInt32();

                        //Triangles
                        for (uint j = 0; j < tCount; j++)
                        {
                            mdbReader.BaseStream.Seek(12, SeekOrigin.Current);
                        }

                        //skip 4 bytes (potential animation count)
                        mdbReader.BaseStream.Seek(4, SeekOrigin.Current);
                        //check if we reach the end (because potential animation block)
                        var length = mdbReader.BaseStream.Position - modelStart;
                        if (length < modelLength)
                            mdbReader.BaseStream.Seek(modelLength - length, SeekOrigin.Current);
                    }
                    else
                    {
                        var meshModel = new MdbMeshModel();
                        //Vertexes
                        for (uint j = 0; j < vCount; j++)
                        {
                            try
                            {
                                //skip 4 bytes (vertex block length)
                                mdbReader.BaseStream.Seek(4, SeekOrigin.Current);
                                float x, y, z, u, v, nx, ny;
                                byte r, g, b, a;
                                x = mdbReader.ReadSingle();
                                y = mdbReader.ReadSingle();
                                z = mdbReader.ReadSingle();
                                u = mdbReader.ReadSingle();
                                v = mdbReader.ReadSingle();
                                nx = mdbReader.ReadSingle();
                                ny = mdbReader.ReadSingle();
                                r = mdbReader.ReadByte();
                                g = mdbReader.ReadByte();
                                b = mdbReader.ReadByte();
                                a = mdbReader.ReadByte();
                                meshModel.MdbVertices.Add(new(x, y, z, u, v, nx, ny, r, g, b, a));
                            }
                            catch
                            {
                                throw new Exception("Skipped\nUnable to read point number " + j + " of model number " + i + " in the file.\n");
                            }
                        }
                        try
                        {
                            //tris count
                            tCount = mdbReader.ReadUInt32();
                        }
                        catch
                        {
                            throw new Exception("Skipped\nUnable to read triangle count of model number " + i + " in the file.\n");
                        }
                        //Triangles
                        for (uint j = 0; j < tCount; j++)
                        {
                            try
                            {
                                //skip 4 bytes (tri block length)
                                mdbReader.BaseStream.Seek(4, SeekOrigin.Current);
                                ushort p0, p1, p2, textureIndex;
                                p0 = mdbReader.ReadUInt16();
                                p1 = mdbReader.ReadUInt16();
                                p2 = mdbReader.ReadUInt16();
                                textureIndex = mdbReader.ReadUInt16();
                                meshModel.MdbTriangles.Add(new(p0, p1, p2, textureIndex));
                            }
                            catch
                            {
                                throw new Exception("Skipped\nUnable to read triangle number " + j + " of model number " + i + ".\n");
                            }
                        }
                        try
                        {
                            //skip 4 bytes (potential animation count)
                            mdbReader.BaseStream.Seek(4, SeekOrigin.Current);

                            //check if we reach the end (because potential animation block)
                            var length = mdbReader.BaseStream.Position - modelStart;
                            if (length < modelLength)
                                mdbReader.BaseStream.Seek(modelLength - length, SeekOrigin.Current);
                        }
                        catch
                        {
                            throw new Exception("Skipped\nUnable to reach the end of model " + i + " in the file.\n");
                        }
                        mesh.MeshModels.Add(meshModel);
                    }
                }

                //Material
                try
                {
                    //material count
                    matCount = mdbReader.ReadUInt32();
                }
                catch
                {
                    throw new Exception("Skipped\nUnable to read texture count in the file.\n");
                }
                var separator = new char[] { ' ', ';', ',', '+', '\r', '\t', '\n' };
                for (uint i = 0; i < matCount; i++)
                {
                    try
                    {
                        //skip 4 bytes (block length)
                        mdbReader.BaseStream.Seek(4, SeekOrigin.Current);
                        //texture name
                        int strlength = mdbReader.ReadInt32(); //can't use uint because ReadChars uses int and it's not cool
                                                               //checking negative case
                        if (strlength < 0)
                            strlength = -strlength - 1; //let's just use the opposite for simplicity
                                                        //creating material
                        var mat = new MdbMaterial();
                        mat.TextureName = new string(mdbReader.ReadChars(strlength));
                        //generate material name from texture name
                        mat.MaterialName = Path.GetFileNameWithoutExtension
                            (string.Join("_", mat.TextureName.Split(separator)));
                        //add to current mat
                        mesh.Materials.Add(mat);
                        //skip 72 bytes (material data)
                        mdbReader.BaseStream.Seek(72, SeekOrigin.Current);
                    }
                    catch
                    {
                        throw new Exception("Skipped\nUnable to read material " + i + ".\n");
                    }
                }
                //ReorganizeTextureIndex(mdbMesh);
                //currentMdbMaterials.Clear();
                //mdbMeshes.Add(mdbMesh);
            }
        }

        private static IList<MdbMaterial> GetFinalMdbMaterials(IEnumerable<MdbMesh> meshes)
        {
            List<MdbMaterial> finalMaterials = [];
            foreach (var mesh in meshes)
            {
                for (int i = 0; i < mesh.Materials.Count; i++)
                {
                    var mat = mesh.Materials[i];
                    if (!finalMaterials.Exists((m) => m.MaterialName.Equals(mat.MaterialName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalMaterials.Add(mat);
                    }
                }
            }
            return finalMaterials;
        }

        private static void WriteMtarialsToMtl(IEnumerable<MdbMesh> meshes, string mtlPath, string textureDirectory)
        {
            var finalMdbMaterials = GetFinalMdbMaterials(meshes);
            using (var mtlWriter = new StreamWriter(File.Open(mtlPath, FileMode.Create, FileAccess.ReadWrite)))
            {
                for (int i = 0; i < finalMdbMaterials.Count; i++)
                {
                    var mat = finalMdbMaterials[i];
                    mtlWriter.WriteLine($"newmtl {mat.MaterialName}");
                    mtlWriter.WriteLine("Ka 0.200000 0.200000 0.200000");
                    mtlWriter.WriteLine("Kd 1.000000 1.000000 1.000000");
                    mtlWriter.WriteLine("Ks 0.000000 0.000000 0.000000");
                    mtlWriter.WriteLine("illum 2");
                    mtlWriter.WriteLine("Ns 8.000000");
                    mtlWriter.WriteLine($"map_Kd {Path.Combine(textureDirectory, Path.ChangeExtension(mat.TextureName, "dds"))}");
                    mtlWriter.WriteLine();
                }
            }
        }

        private static void WriteMeshesToObj(IEnumerable<MdbMesh> meshes, string objFilePath, string mtlPath)
        {
            using (var objWriter = new StreamWriter(File.Open(objFilePath, FileMode.Create, FileAccess.ReadWrite)))
            {
                objWriter.WriteLine($"mtllib {Path.GetFileName(mtlPath)}");
                int vCount = 1;
                foreach (var mesh in meshes)
                {
                    for (int i = 0; i < mesh.MeshModels.Count; i++)
                    {
                        var meshModel = mesh.MeshModels[i];
                        objWriter.WriteLine($"g {mesh.GroupName}_{i}");
                        objWriter.WriteLine($"o {mesh.GroupName}_{i}");
                        for (int j = 0; j < meshModel.MdbVertices.Count; j++)
                        {
                            var mdbVertice = meshModel.MdbVertices[j];
                            objWriter.WriteLine($"v {mdbVertice.X} {mdbVertice.Z} {-mdbVertice.Y}");
                            objWriter.WriteLine($"vt {mdbVertice.U} {-mdbVertice.V}");
                            objWriter.WriteLine($"vn {-Math.Sin(mdbVertice.NX)} {Math.Sin(mdbVertice.NY)} {-Math.Cos(mdbVertice.NX)}");
                        }
                        var triGroups = meshModel.MdbTriangles.GroupBy((t) => t.TextureIndex);
                        foreach (var triGroup in triGroups)
                        {
                            var mat = mesh.Materials[triGroup.Key];
                            objWriter.WriteLine($"usemtl {mat.MaterialName}");
                            foreach (var mdbTriangle in triGroup)
                            {
                                var p0 = mdbTriangle.P0 + vCount;
                                var p1 = mdbTriangle.P1 + vCount;
                                var p2 = mdbTriangle.P2 + vCount;
                                objWriter.WriteLine($"f {p2}/{p2}/{p2} {p1}/{p1}/{p1} {p0}/{p0}/{p0}");
                            }
                        }
                        vCount += meshModel.MdbVertices.Count;
                    }
                }
            }
        }

        private static void WriteMeshesToGlb(IEnumerable<MdbMesh> meshes, string glbPath, string textureDirectory)
        {
            var finalMdbMaterials = GetFinalMdbMaterials(meshes);
            var glbScene = new SceneBuilder();
            var glbMaterials = new List<MaterialBuilder>();
            for (int i = 0; i < finalMdbMaterials.Count; i++)
            {
                var finalMdbMaterial = finalMdbMaterials[i];
                var glbMaterial = new MaterialBuilder(finalMdbMaterial.MaterialName)
                    .WithMetallicRoughness(0, 1f);

                glbMaterial.Extras = new JsonObject
                {
                    ["TextureName"] = Path.GetFileNameWithoutExtension(finalMdbMaterial.TextureName)
                };

                try
                {
                    var pngBytes = DDSUtils.ConvertDdsToPngBytes(textureDirectory + Path.ChangeExtension(finalMdbMaterial.TextureName, "dds"));
                    var imageBuilder = ImageBuilder.From(pngBytes, Path.GetFileNameWithoutExtension(finalMdbMaterial.TextureName));
                    glbMaterial.WithChannelImage(KnownChannel.BaseColor, imageBuilder);
                }
                finally
                {
                    glbMaterials.Add(glbMaterial);
                }
            }
            foreach (var mdbMesh in meshes)
            {
                for (int j = 0; j < mdbMesh.MeshModels.Count; j++)
                {
                    var meshModel = mdbMesh.MeshModels[j];
                    var finalGroupName = $"{mdbMesh.GroupName}_{j}";
                    var glbNode = new NodeBuilder(finalGroupName);
                    var glbMesh = new MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>(finalGroupName);
                    for (int k = 0; k < meshModel.MdbTriangles.Count; k++)
                    {
                        var mdbTriangle = meshModel.MdbTriangles[k];
                        var v0 = meshModel.MdbVertices[mdbTriangle.P0];
                        var v1 = meshModel.MdbVertices[mdbTriangle.P1];
                        var v2 = meshModel.MdbVertices[mdbTriangle.P2];
                        var pos0 = new Vector3(v0.X, v0.Z, -v0.Y);
                        var pos1 = new Vector3(v1.X, v1.Z, -v1.Y);
                        var pos2 = new Vector3(v2.X, v2.Z, -v2.Y);
                        var n0 = Vector3.Normalize(new Vector3((float)-Math.Sin(v0.NX), (float)Math.Sin(v0.NY), (float)-Math.Cos(v0.NX)));
                        var n1 = Vector3.Normalize(new Vector3((float)-Math.Sin(v1.NX), (float)Math.Sin(v1.NY), (float)-Math.Cos(v1.NX)));
                        var n2 = Vector3.Normalize(new Vector3((float)-Math.Sin(v2.NX), (float)Math.Sin(v2.NY), (float)-Math.Cos(v2.NX)));
                        var glbPrim = glbMesh.UsePrimitive(glbMaterials[mdbTriangle.TextureIndex]);
                        var p0 = new VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>
                            (new(pos0, n0), new VertexColor1Texture1(new(v0.R / 255f, v0.G / 255f, v0.B / 255f, v0.A / 255f), new(v0.U, v0.V)));
                        var p1 = new VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>
                            (new(pos1, n1), new VertexColor1Texture1(new(v1.R / 255f, v1.G / 255f, v1.B / 255f, v1.A / 255f), new(v1.U, v1.V)));
                        var p2 = new VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>
                            (new(pos2, n2), new VertexColor1Texture1(new(v2.R / 255f, v2.G / 255f, v2.B / 255f, v2.A / 255f), new(v2.U, v2.V)));
                        glbPrim.AddTriangle(p2, p1, p0);
                    }
                    glbScene.AddNode(glbNode);
                    glbScene.AddRigidMesh(glbMesh, glbNode);
                }
            }
            glbScene.ToGltf2().SaveGLB(glbPath);
        }
    }
}

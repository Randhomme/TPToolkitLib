using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TPToolkitLib.Mesh.Classes;
using TPToolkitLib.Mesh.Structs;
using TPToolkitLib.Utils;

namespace TPToolkitLib
{
    public static class MdbTool
    {
        public static void XMdbTo1Obj(string[] mdbFilePaths, string objFilePath, string textureDirectory, bool lods)
        {
            var meshes = MeshesFromMdbs(mdbFilePaths, lods);
            MeshesToObj(meshes, objFilePath, textureDirectory);
        }

        public static void XMdbTo1Glb(string[] mdbFilePaths, string glbFilePath, string textureDirectory, bool lods)
        {
            var meshes = MeshesFromMdbs(mdbFilePaths, lods);
            MeshesToGlb(meshes, glbFilePath, textureDirectory);
        }

        public static void XMdbToXObj(string[] mdbFilePaths, string objFolderPath, string textureDirectory, bool lods)
        {
            for (int i = 0; i < mdbFilePaths.Length; i++)
            {
                var mdbFilePath = mdbFilePaths[i];
                var mesh = MeshFromMdb(mdbFilePath, lods);
                string objFilePath = Path.Combine(objFolderPath, Path.ChangeExtension(mesh.GroupName, "obj"));
                MeshToObj(mesh, objFilePath, textureDirectory);
            }
        }

        public static void XMdbToXGlb(string[] mdbFilePaths, string glbFolderPath, string textureDirectory, bool lods)
        {
            for (int i = 0; i < mdbFilePaths.Length; i++)
            {
                var mdbFilePath = mdbFilePaths[i];
                var mesh = MeshFromMdb(mdbFilePath, lods);
                string glbFilePath = Path.Combine(glbFolderPath, Path.ChangeExtension(mesh.GroupName, "glb"));
                MeshToGlb(mesh, glbFilePath, textureDirectory);
            }
        }

        public static void XObjToXMdb(string[] objFilePaths, string mdbFolderPath)
        {
            for (int i = 0; i < objFilePaths.Length; i++)
            {
                var objFilePath = objFilePaths[i];
                var meshes = MeshesFromObj(objFilePath);
            }
        }

        public static IEnumerable<MdbMesh> MeshesFromMdbs(string[] mdbFilePaths, bool lods)
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
            return ReadMdb(groupName, mdbFilePath, lods);
        }

        public static IEnumerable<MdbMesh> MeshesFromObj(string objFilePath)
        {
            IList<MdbMesh> mdbMeshes = [];
            var objGroups = ReadObj(objFilePath);
            return mdbMeshes;
        }

        public static void MeshesToObj(IEnumerable<MdbMesh> meshes, string objFilePath, string textureDirectory)
        {
            string mtlFilePath = Path.ChangeExtension(objFilePath, "mtl");
            var finalMdbMaterials = GetFinalMdbMaterials(meshes);
            // Write mtl
            WriteMaterialsToNewMtl(finalMdbMaterials, mtlFilePath, textureDirectory);
            // Write obj
            WriteMeshesToNewObj(meshes, objFilePath, mtlFilePath);
        }

        public static void MeshToObj(MdbMesh mesh, string objFilePath, string textureDirectory)
        {
            string mtlFilePath = Path.ChangeExtension(objFilePath, "mtl");
            // Write mtl
            WriteMaterialsToNewMtl(mesh.Materials, mtlFilePath, textureDirectory);
            // Write obj
            WriteMeshToNewObj(mesh, objFilePath, mtlFilePath);
        }

        public static void MeshesToGlb(IEnumerable<MdbMesh> meshes, string glbFilePath, string textureDirectory)
        {
            WriteMeshesToNewGlb(meshes, glbFilePath, textureDirectory);
        }

        public static void MeshToGlb(MdbMesh mesh, string glbFilePath, string textureDirectory)
        {
            WriteMeshToNewGlb(mesh, glbFilePath, textureDirectory);
        }

        private static MdbMesh ReadMdb(string groupName, string mdbFilePath, bool lods)
        {
            var mesh = new MdbMesh(groupName);
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
            }
            return mesh;
        }

        private static ObjScene ReadObj(string objFilePath)
        {
            char[] separator = { ' ' };
            string mtlFileName = string.Empty;
            var objScene = new ObjScene();
            ObjGroup currentObjGroup = new(string.Empty);
            ObjMaterialGroup currentObjMaterialGroup = new("NULL");
            using (var objReader = new StreamReader(File.OpenRead(objFilePath)))
            {
                uint lineNumber = 0;
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();
                    lineNumber += 1;
                    // v
                    if(line.StartsWith("v ", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var s = line.Split(separator, 4);
                            objScene.V.Add(new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3])));
                        }
                        catch (Exception ex)
                        {
                            throw new($"Invalid vertex at line {lineNumber} : {ex.Message}", ex);
                        }
                    }
                    // vt
                    else if(line.StartsWith("vt ", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var s = line.Split(separator, 3);
                            objScene.VT.Add(new Vector2(float.Parse(s[1]), float.Parse(s[2])));
                        }
                        catch (Exception ex)
                        {
                            throw new($"Invalid vertex texture at line {lineNumber} : {ex.Message}", ex);
                        }
                    }
                    // vn
                    else if(line.StartsWith("vn ", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var s = line.Split(separator, 4);
                            objScene.VN.Add(new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3])));
                        }
                        catch (Exception ex)
                        {
                            throw new($"Invalid normal at line {lineNumber} : {ex.Message}", ex);
                        }
                    }
                    // f
                    else if(line.StartsWith("f ", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var s = line.Split(separator);
                            if (s.Length > 4)
                                throw new("It has more than 3 points");
                            var p0 = s[1].Split('/');
                            var p1 = s[2].Split('/');
                            var p2 = s[3].Split('/');
                            var tri = new ObjTriangle();
                            for (int i = 0; i < Math.Min(p0.Length, 3); i++)
                            {
                                int.TryParse(p0[i], out tri.P0[i]);
                            }
                            for (int i = 0; i < Math.Min(p1.Length, 3); i++)
                            {
                                int.TryParse(p1[i], out tri.P1[i]);
                            }
                            for (int i = 0; i < Math.Min(p2.Length, 3); i++)
                            {
                                int.TryParse(p2[i], out tri.P2[i]);
                            }
                            currentObjMaterialGroup.Triangles.Add(tri);
                        }
                        catch(Exception ex)
                        {
                            throw new($"Invalid triangle line {lineNumber} : {ex.Message}");
                        }
                    }
                    // group/object
                    else if(line.StartsWith("g ", StringComparison.OrdinalIgnoreCase) || line.StartsWith("o ", StringComparison.OrdinalIgnoreCase))
                    {
                        var objGroupName = line.Substring(2);
                        // if it's not the same group (if it is, do nothing)
                        if (!string.Equals(objGroupName, currentObjGroup.GroupName))
                        {
                            var groupExists = false;
                            foreach (var tempObjGroup in objScene.ObjGroups)
                            {
                                if (tempObjGroup.GroupName.Equals(objGroupName))
                                {
                                    groupExists = true;
                                    currentObjGroup = tempObjGroup;
                                    break;
                                }
                            }
                            // if it's a new group
                            if (!groupExists)
                            {
                                // Add the current mat group to the current group before creating a new one
                                if (currentObjMaterialGroup.Triangles.Count > 0)
                                {
                                    currentObjGroup.MaterialGroups.Add(currentObjMaterialGroup);
                                }
                                currentObjMaterialGroup = new(currentObjMaterialGroup.MaterialName);
                                // Add the group to the group list if it's not empty
                                if (currentObjGroup.MaterialGroups.Count > 0)
                                {
                                    objScene.ObjGroups.Add(currentObjGroup);
                                }
                                currentObjGroup = new(objGroupName);
                            }
                        }
                    }
                    // usemtl
                    else if(line.StartsWith("usemtl ", StringComparison.OrdinalIgnoreCase))
                    {
                        // Add the current mat group to the current group before creating a new one
                        if (currentObjMaterialGroup.Triangles.Count > 0)
                        {
                            currentObjGroup.MaterialGroups.Add(currentObjMaterialGroup);
                        }
                        currentObjMaterialGroup = new(line.Substring(7));
                    }
                    // mtllb (mtl file name)
                    else if(line.StartsWith("mtllib "))
                    {
                        mtlFileName = line.Substring(7);
                    }
                }
            }
            // Add the last group if not empty
            // Add the current mat group to the current group before creating a new one
            if (currentObjMaterialGroup.Triangles.Count > 0)
            {
                currentObjGroup.MaterialGroups.Add(currentObjMaterialGroup);
            }
            // Add the group to the group list if it's not empty
            if (currentObjGroup.MaterialGroups.Count > 0)
            {
                objScene.ObjGroups.Add(currentObjGroup);
            }
            var mtlFilePath = string.IsNullOrWhiteSpace(mtlFileName) ? Path.ChangeExtension(objFilePath, "mtl") : Path.Combine(Path.GetDirectoryName(objFilePath), mtlFileName);
            ReadMtl(mtlFilePath);
            return objScene;
        }

        private static void ReadMtl(string mtlFilePath)
        {

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

        private static IList<MaterialBuilder> GetFinalGlbMaterials(IList<MdbMaterial> finalMdbMaterials, string textureDirectory)
        {
            var glbMaterials = new List<MaterialBuilder>();
            for (int i = 0; i < finalMdbMaterials.Count; i++)
            {
                var finalMdbMaterial = finalMdbMaterials[i];
                var glbMaterial = new MaterialBuilder(finalMdbMaterial.MaterialName)
                    .WithMetallicRoughness(0, 1f)
                    .WithDoubleSide(true)
                    .WithSpecularColor(null, new(0, 0, 0));

                glbMaterial.Extras = new JsonObject
                {
                    ["TextureName"] = Path.GetFileNameWithoutExtension(finalMdbMaterial.TextureName)
                };

                try
                {
                    var pngBytes = DDSUtils.ConvertDdsToPngBytes(Path.Combine(textureDirectory, Path.ChangeExtension(finalMdbMaterial.TextureName, "dds")));
                    var imageBuilder = ImageBuilder.From(pngBytes, Path.GetFileNameWithoutExtension(finalMdbMaterial.TextureName));
                    glbMaterial.WithChannelImage(KnownChannel.BaseColor, imageBuilder);
                }
                catch (FileNotFoundException) { } // No texture for the material, we keep going
                finally
                {
                    glbMaterials.Add(glbMaterial);
                }
            }
            return glbMaterials;
        }

        private static void ReorganizeTextureIndex(MdbMesh mesh, IList<MdbMaterial> finalMdbMaterials)
        {
            for (int i = 0; i < mesh.MeshModels.Count; i++)
            {
                var meshModel = mesh.MeshModels[i];
                for (int j = 0; j < meshModel.MdbTriangles.Count; j++)
                {
                    var textureIndex = meshModel.MdbTriangles[j].TextureIndex;
                    if (textureIndex < mesh.Materials.Count)
                    {
                        var mat = mesh.Materials[textureIndex];
                        for (ushort k = 0; k < finalMdbMaterials.Count; k++)
                        {
                            if (mat.MaterialName == finalMdbMaterials[k].MaterialName)
                            {
                                var mdbTriangle = meshModel.MdbTriangles[j];
                                meshModel.MdbTriangles[j] = new(mdbTriangle.P0, mdbTriangle.P1, mdbTriangle.P2, k);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void WriteMaterialsToNewMtl(IList<MdbMaterial> finalMdbMaterials, string mtlPath, string textureDirectory)
        {
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

        private static void WriteMeshesToNewObj(IEnumerable<MdbMesh> meshes, string objFilePath, string mtlFilePath)
        {
            using (var objWriter = new StreamWriter(File.Open(objFilePath, FileMode.Create, FileAccess.ReadWrite)))
            {
                objWriter.WriteLine($"mtllib {Path.GetFileName(mtlFilePath)}");
                int vCount = 1;
                foreach (var mesh in meshes)
                {
                    WriteMeshToObj(mesh, objWriter, ref vCount);
                }
            }
        }

        private static void WriteMeshToNewObj(MdbMesh mesh, string objFilePath, string mtlFilePath)
        {
            using (var objWriter = new StreamWriter(File.Open(objFilePath, FileMode.Create, FileAccess.ReadWrite)))
            {
                objWriter.WriteLine($"mtllib {Path.GetFileName(mtlFilePath)}");
                int vCount = 1;
                WriteMeshToObj(mesh, objWriter, ref vCount);
            }
        }

        private static void WriteMeshToObj(MdbMesh mesh, StreamWriter objWriter, ref int vCount)
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

        private static void WriteMeshesToNewGlb(IEnumerable<MdbMesh> meshes, string glbFilePath, string textureDirectory)
        {
            var finalMdbMaterials = GetFinalMdbMaterials(meshes);
            var glbScene = new SceneBuilder();
            var glbMaterials = GetFinalGlbMaterials(finalMdbMaterials, textureDirectory);
            foreach (var mesh in meshes)
            {
                ReorganizeTextureIndex(mesh, finalMdbMaterials);
                AddMeshToGlbScene(mesh, glbScene, glbMaterials);
            }
            glbScene.ToGltf2().SaveGLB(glbFilePath);
        }

        private static void WriteMeshToNewGlb(MdbMesh mesh, string glbFilePath, string textureDirectory)
        {
            var glbScene = new SceneBuilder();
            var glbMaterials = GetFinalGlbMaterials(mesh.Materials, textureDirectory);
            ReorganizeTextureIndex(mesh, mesh.Materials);
            AddMeshToGlbScene(mesh, glbScene, glbMaterials);
            glbScene.ToGltf2().SaveGLB(glbFilePath);
        }

        private static void AddMeshToGlbScene(MdbMesh mesh, SceneBuilder glbScene, IList<MaterialBuilder> glbMaterials)
        {
            for (int j = 0; j < mesh.MeshModels.Count; j++)
            {
                var meshModel = mesh.MeshModels[j];
                var finalGroupName = $"{mesh.GroupName}_{j}";
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

        private static string RealGroupName(string groupname)
        {
            int index = groupname.LastIndexOf('_');
            if (index != -1)
            {
                string temp = groupname.Substring(index + 1);
                return int.TryParse(temp, out _) ? groupname.Remove(index) : groupname;
            }
            else
            {
                return groupname;
            }
        }
    }
}

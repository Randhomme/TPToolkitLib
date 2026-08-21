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
using System.Text;
using System.Text.Json.Nodes;
using TPToolkitLib.Mesh;
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
                foreach (var mdbMesh in meshes)
                {
                    var mdbFilePath = Path.Combine(mdbFolderPath, Path.ChangeExtension(mdbMesh.GroupName, "mdb"));
                    MeshToMdb(mdbMesh, mdbFilePath);
                }
            }
        }

        public static void XGlbToXMdb(string[] glbFilePaths, string mdbFolderPath)
        {
            for (int i = 0; i < glbFilePaths.Length; i++)
            {
                var glbFilePath = glbFilePaths[i];
                var meshes = MeshesFromGlb(glbFilePath);
                foreach (var mdbMesh in meshes)
                {
                    var mdbFilePath = Path.Combine(mdbFolderPath, Path.ChangeExtension(mdbMesh.GroupName, "mdb"));
                    MeshToMdb(mdbMesh, mdbFilePath);
                }
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
            var objScene = ReadObj(objFilePath);
            return ObjSceneToMdbMeshes(objScene);
        }

        public static IEnumerable<MdbMesh> MeshesFromGlb(string glbFilePath)
        {
            var glbScene = SceneBuilder.LoadDefaultScene(glbFilePath);
            return GlbSceneToMdbMeshes(glbScene);
        }

        public static void MeshesToObj(IEnumerable<MdbMesh> mdbMeshes, string objFilePath, string textureDirectory)
        {
            string mtlFilePath = Path.ChangeExtension(objFilePath, "mtl");
            var finalMdbMaterials = GetFinalMdbMaterials(mdbMeshes);
            // Write mtl
            WriteMaterialsToNewMtl(finalMdbMaterials, mtlFilePath, textureDirectory);
            // Write obj
            WriteMeshesToNewObj(mdbMeshes, objFilePath, mtlFilePath);
        }

        public static void MeshToObj(MdbMesh mdbMesh, string objFilePath, string textureDirectory)
        {
            string mtlFilePath = Path.ChangeExtension(objFilePath, "mtl");
            // Write mtl
            WriteMaterialsToNewMtl(mdbMesh.Materials, mtlFilePath, textureDirectory);
            // Write obj
            WriteMeshToNewObj(mdbMesh, objFilePath, mtlFilePath);
        }

        public static void MeshesToGlb(IEnumerable<MdbMesh> mdbMeshes, string glbFilePath, string textureDirectory)
        {
            WriteMeshesToNewGlb(mdbMeshes, glbFilePath, textureDirectory);
        }

        public static void MeshToGlb(MdbMesh mdbMesh, string glbFilePath, string textureDirectory)
        {
            WriteMeshToNewGlb(mdbMesh, glbFilePath, textureDirectory);
        }

        public static void MeshToMdb(MdbMesh mdbMesh, string mdbFilePath)
        {
            WriteMeshToNewMdb(mdbMesh, mdbFilePath);
        }

        private static MdbMesh ReadMdb(string groupName, string mdbFilePath, bool lods)
        {
            var mdbMesh = new MdbMesh(groupName);
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
                        mdbMesh.MeshModels.Add(meshModel);
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
                        var mat = new MdbMaterial(new string(mdbReader.ReadChars(strlength)));
                        //add to current mat
                        mdbMesh.Materials.Add(mat);
                        //skip 72 bytes (material data)
                        mdbReader.BaseStream.Seek(72, SeekOrigin.Current);
                    }
                    catch
                    {
                        throw new Exception("Skipped\nUnable to read material " + i + ".\n");
                    }
                }
            }
            return mdbMesh;
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
                            objScene.Vt.Add(new Vector2(float.Parse(s[1]), float.Parse(s[2])));
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
                            objScene.Vn.Add(new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3])));
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
            ReadMtl(mtlFilePath, objScene);
            return objScene;
        }

        private static void ReadMtl(string mtlFilePath, ObjScene objScene)
        {
            using(var mtlReader = new StreamReader(File.OpenRead(mtlFilePath)))
            {
                string line;
                MdbMaterial? mat = null;
                while ((line = mtlReader.ReadLine()) != null)
                {
                    if (line.StartsWith("newmtl ", StringComparison.OrdinalIgnoreCase))
                    {
                        if (mat != null && !objScene.Materials.Any(m => m.MaterialName.Equals(mat.MaterialName, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (objScene.Materials.Count > ushort.MaxValue)
                                throw new Exception("Material count can't exceed 65536.");
                            objScene.Materials.Add(mat);
                        }
                        mat = new(line.Substring(7), "NULL");
                    }
                    else if (line.StartsWith("map_kd ", StringComparison.OrdinalIgnoreCase))
                    {
                        if (mat != null)
                        {
                            mat.TextureName = Path.GetFileName(line.Substring(7));
                        }
                    }
                }
                //add the last mat
                if (mat != null && !objScene.Materials.Any(m => m.MaterialName.Equals(mat.MaterialName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (objScene.Materials.Count > ushort.MaxValue)
                        throw new Exception("Material count can't exceed 65536.");
                    objScene.Materials.Add(mat);
                }
            }
        }

        private static IEnumerable<MdbMesh> ObjSceneToMdbMeshes(ObjScene objScene)
        {
            IList<MdbMesh> mdbMeshes = [];
            var sortedGroups = objScene.ObjGroups.OrderBy((og) => og.GroupName, new CustomComparer<string>(NaturalStringComparer.CompareNatural));
            var groups = sortedGroups.GroupBy((g) => RealGroupName(g.GroupName));
            foreach (var group in groups)
            {
                var mdbMesh = new MdbMesh(group.Key);
                foreach (var objGroup in group)
                {
                    var mdbMeshModel = new MdbMeshModel();
                    IList<int[]> currentObjPoints = []; // used for correct point indexing
                    foreach (var objMaterialGroup in objGroup.MaterialGroups)
                    {
                        ushort materialIndex;
                        bool hasFoundMaterial = false;
                        var currentMdbMaterial = objScene.Materials.First((m) => m.MaterialName.Equals(objMaterialGroup.MaterialName, StringComparison.OrdinalIgnoreCase));
                        var currentTextureNameNoExt = Path.GetFileNameWithoutExtension(currentMdbMaterial.TextureName);
                        for (materialIndex = 0; materialIndex < mdbMesh.Materials.Count; materialIndex++)
                        {
                            var mdbMaterial = mdbMesh.Materials[materialIndex];
                            var textureName = Path.GetFileNameWithoutExtension(mdbMaterial.TextureName);
                            if (textureName.Equals(currentTextureNameNoExt, StringComparison.OrdinalIgnoreCase))
                            {
                                hasFoundMaterial = true;
                                break;
                            }
                        }
                        if (!hasFoundMaterial)
                        {
                            var newMdbMaterial = new MdbMaterial(currentTextureNameNoExt);
                            if (!currentTextureNameNoExt.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                newMdbMaterial.TextureName = Path.ChangeExtension(currentTextureNameNoExt, "tga");
                            mdbMesh.Materials.Add(newMdbMaterial);
                        }
                        foreach (var triangle in objMaterialGroup.Triangles)
                        {
                            int p0Index, p1Index, p2Index;
                            p0Index = p1Index = p2Index = 0;
                            bool hasFoundP0, hasFoundP1, hasFoundP2;
                            hasFoundP0 = hasFoundP1 = hasFoundP2 = false;
                            // get real indexes
                            for (int i = 0; i < currentObjPoints.Count; i++)
                            {
                                var p = currentObjPoints[i];
                                if (PointsEquals(triangle.P0, p))
                                {
                                    p0Index = i;
                                    hasFoundP0 = true;
                                    if (hasFoundP0 && hasFoundP1 && hasFoundP2)
                                        break;
                                }
                                if (PointsEquals(triangle.P1, p))
                                {
                                    p1Index = i;
                                    hasFoundP1 = true;
                                    if (hasFoundP0 && hasFoundP1 && hasFoundP2)
                                        break;
                                }
                                if (PointsEquals(triangle.P2, p))
                                {
                                    p2Index = i;
                                    hasFoundP2 = true;
                                    if (hasFoundP0 && hasFoundP1 && hasFoundP2)
                                        break;
                                }
                            }
                            if (!hasFoundP2)
                            {
                                if (currentObjPoints.Count > ushort.MaxValue)
                                    throw new Exception("Model vertex count exceeded 65536.\n");
                                p2Index = currentObjPoints.Count;
                                currentObjPoints.Add(triangle.P2);
                            }
                            if (!hasFoundP1)
                            {
                                if (currentObjPoints.Count > ushort.MaxValue)
                                    throw new Exception("Model vertex count exceeded 65536.\n");
                                p1Index = currentObjPoints.Count;
                                currentObjPoints.Add(triangle.P1);
                            }
                            if (!hasFoundP0)
                            {
                                if (currentObjPoints.Count > ushort.MaxValue)
                                    throw new Exception("Model vertex count exceeded 65536.\n");
                                p0Index = currentObjPoints.Count;
                                currentObjPoints.Add(triangle.P0);
                            }
                            mdbMeshModel.MdbTriangles.Add(new((ushort)p2Index, (ushort)p1Index, (ushort)p0Index, materialIndex));
                        }
                    }
                    for (int i = 0; i < currentObjPoints.Count; i++)
                    {
                        var p = currentObjPoints[i];
                        var v = objScene.V[p[0] - 1];
                        var vt = objScene.Vt[p[1] - 1];
                        var vn = objScene.Vn[p[2] - 1];
                        if (vn.Y < -1) vn.Y = -1;
                        else if (vn.Y > 1) vn.Y = 1;
                        if (vn.Z < -1) vn.Z = -1;
                        else if (vn.Z > 1) vn.Z = 1;
                        var nx = vn.X <= 0 ? Math.Acos(-vn.Z) : -Math.Acos(-vn.Z);
                        var ny = Math.Asin(vn.Y);
                        mdbMeshModel.MdbVertices.Add(new(v.X, -v.Z, v.Y, vt.X, -vt.Y, (float)nx, (float)ny, 255, 255, 255, 255));
                    }
                    if (mdbMeshModel.MdbTriangles.Count > 0)
                    {
                        mdbMesh.MeshModels.Add(mdbMeshModel);
                    }
                }
                if (mdbMesh.MeshModels.Count > 0)
                {
                    mdbMeshes.Add(mdbMesh);
                }
            }
            return mdbMeshes;
        }

        private static IEnumerable<MdbMesh> GlbSceneToMdbMeshes(SceneBuilder glbScene)
        {
            IList<MdbMesh> mdbMeshes = [];
            var sortedGroups = glbScene.Instances.OrderBy((i) => i.Name, new CustomComparer<string>(NaturalStringComparer.CompareNatural)).GroupBy((i) => RealGroupName(i.Name));
            foreach (var group in sortedGroups)
            {
                var mdbMesh = new MdbMesh(group.Key);
                foreach (var glbGroup in group)
                {
                    var mdbMeshModel = new MdbMeshModel();
                    var glbMesh = glbGroup.Content.GetGeometryAsset();
                    if (glbMesh != null)
                    {
                        int vCount = 0;
                        foreach (var primitive in glbMesh.Primitives)
                        {
                            string currentTextureNameNoExt = "NULL";
                            if (primitive.Material.Extras is JsonObject obj)
                            {
                                if (obj.TryGetPropertyValue("TextureName", out var value))
                                {
                                    if (value != null)
                                    {
                                        currentTextureNameNoExt = Path.GetFileNameWithoutExtension(value.GetValue<string>());
                                    }
                                }
                                else
                                {
                                    var channel = primitive.Material.GetChannel(KnownChannel.BaseColor);
                                    if (channel != null)
                                    {
                                        currentTextureNameNoExt = Path.GetFileNameWithoutExtension(channel.Texture.PrimaryImage.Name);
                                    }
                                }
                            }
                            else
                            {
                                var channel = primitive.Material.GetChannel(KnownChannel.BaseColor);
                                if (channel != null)
                                {
                                    currentTextureNameNoExt = Path.GetFileNameWithoutExtension(channel.Texture.PrimaryImage.Name);
                                }
                            }
                            ushort materialIndex;
                            bool hasFoundMaterial = false;
                            for (materialIndex = 0; materialIndex < mdbMesh.Materials.Count; materialIndex++)
                            {
                                var mdbMaterial = mdbMesh.Materials[materialIndex];
                                var textureNameNoExt = Path.GetFileNameWithoutExtension(mdbMaterial.TextureName);
                                if (textureNameNoExt.Equals(currentTextureNameNoExt, StringComparison.OrdinalIgnoreCase))
                                {
                                    hasFoundMaterial = true;
                                    break;
                                }
                            }
                            if (!hasFoundMaterial)
                            {
                                var newMdbMaterial = new MdbMaterial(currentTextureNameNoExt);
                                if (!currentTextureNameNoExt.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                    newMdbMaterial.TextureName = Path.ChangeExtension(currentTextureNameNoExt, "tga");
                                mdbMesh.Materials.Add(newMdbMaterial);
                            }
                            for (int i = 0; i < primitive.Vertices.Count; i++)
                            {
                                var v = primitive.Vertices[i];
                                var vGeom = v.GetGeometry();
                                var vMat = v.GetMaterial();
                                var vPos = vGeom.GetPosition();
                                var vTexPos = vMat.GetTexCoord(0);
                                var vColor = vMat.GetColor(0);
                                float nx = 0, ny = 0;
                                if (vGeom.TryGetNormal(out var vNorm))
                                {
                                    if (vNorm.Y < -1) vNorm.Y = -1;
                                    else if (vNorm.Y > 1) vNorm.Y = 1;
                                    if (vNorm.Z < -1) vNorm.Z = -1;
                                    else if (vNorm.Z > 1) vNorm.Z = 1;
                                    if (vNorm.X <= 0)
                                        nx = (float)Math.Acos(-vNorm.Z);
                                    else
                                        nx = (float)-Math.Acos(-vNorm.Z);
                                    ny = (float)Math.Asin(vNorm.Y);
                                }
                                mdbMeshModel.MdbVertices.Add(new(vPos.X, -vPos.Z, vPos.Y, vTexPos.X, vTexPos.Y, nx, ny, (byte)(vColor.X * 255), (byte)(vColor.Y * 255), (byte)(vColor.Z * 255), (byte)(vColor.W * 255)));
                            }
                            for (int i = 0; i < primitive.Triangles.Count; i++)
                            {
                                var t = primitive.Triangles[i];
                                mdbMeshModel.MdbTriangles.Add(new((ushort)(t.C + vCount), (ushort)(t.B + vCount), (ushort)(t.A + vCount), materialIndex));
                            }
                            vCount += primitive.Vertices.Count;
                        }
                        if (mdbMeshModel.MdbTriangles.Count > 0)
                        {
                            mdbMesh.MeshModels.Add(mdbMeshModel);
                        }
                    }
                }
                if (mdbMesh.MeshModels.Count > 0)
                {
                    mdbMeshes.Add(mdbMesh);
                }
            }
            return mdbMeshes;
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
                    .WithDoubleSide(true);

                glbMaterial.Extras = new JsonObject
                {
                    ["TextureName"] = Path.GetFileNameWithoutExtension(finalMdbMaterial.TextureName)
                };

                try
                {
                    var pngBytes = DDSUtils.ConvertDdsToPngBytes(Path.Combine(textureDirectory, Path.ChangeExtension(finalMdbMaterial.TextureName, "dds")));
                    var imageBuilder = ImageBuilder.From(pngBytes, Path.GetFileNameWithoutExtension(finalMdbMaterial.TextureName));
                    byte[] a = new byte[64];
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

        private static void ReorganizeTextureIndex(MdbMesh mdbMesh, IList<MdbMaterial> finalMdbMaterials)
        {
            for (int i = 0; i < mdbMesh.MeshModels.Count; i++)
            {
                var meshModel = mdbMesh.MeshModels[i];
                for (int j = 0; j < meshModel.MdbTriangles.Count; j++)
                {
                    var textureIndex = meshModel.MdbTriangles[j].TextureIndex;
                    if (textureIndex < mdbMesh.Materials.Count)
                    {
                        var mat = mdbMesh.Materials[textureIndex];
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

        private static void WriteMeshesToNewObj(IEnumerable<MdbMesh> mdbMeshes, string objFilePath, string mtlFilePath)
        {
            using (var objWriter = new StreamWriter(File.Open(objFilePath, FileMode.Create, FileAccess.ReadWrite)))
            {
                objWriter.WriteLine($"mtllib {Path.GetFileName(mtlFilePath)}");
                int vCount = 1;
                foreach (var mdbMesh in mdbMeshes)
                {
                    WriteMeshToObj(mdbMesh, objWriter, ref vCount);
                }
            }
        }

        private static void WriteMeshToNewObj(MdbMesh mdbMesh, string objFilePath, string mtlFilePath)
        {
            using (var objWriter = new StreamWriter(File.Open(objFilePath, FileMode.Create, FileAccess.ReadWrite)))
            {
                objWriter.WriteLine($"mtllib {Path.GetFileName(mtlFilePath)}");
                int vCount = 1;
                WriteMeshToObj(mdbMesh, objWriter, ref vCount);
            }
        }

        private static void WriteMeshToObj(MdbMesh mdbMesh, StreamWriter objWriter, ref int vCount)
        {
            for (int i = 0; i < mdbMesh.MeshModels.Count; i++)
            {
                var meshModel = mdbMesh.MeshModels[i];
                objWriter.WriteLine($"g {mdbMesh.GroupName}_{i}");
                objWriter.WriteLine($"o {mdbMesh.GroupName}_{i}");
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
                    var mat = mdbMesh.Materials[triGroup.Key];
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

        private static void WriteMeshesToNewGlb(IEnumerable<MdbMesh> mdbMeshes, string glbFilePath, string textureDirectory)
        {
            var finalMdbMaterials = GetFinalMdbMaterials(mdbMeshes);
            var glbScene = new SceneBuilder();
            var glbMaterials = GetFinalGlbMaterials(finalMdbMaterials, textureDirectory);
            foreach (var mdbMesh in mdbMeshes)
            {
                ReorganizeTextureIndex(mdbMesh, finalMdbMaterials);
                AddMeshToGlbScene(mdbMesh, glbScene, glbMaterials);
            }
            glbScene.ToGltf2().SaveGLB(glbFilePath);
        }

        private static void WriteMeshToNewGlb(MdbMesh mdbMesh, string glbFilePath, string textureDirectory)
        {
            var glbScene = new SceneBuilder();
            var glbMaterials = GetFinalGlbMaterials(mdbMesh.Materials, textureDirectory);
            ReorganizeTextureIndex(mdbMesh, mdbMesh.Materials);
            AddMeshToGlbScene(mdbMesh, glbScene, glbMaterials);
            glbScene.ToGltf2().SaveGLB(glbFilePath);
        }

        private static void AddMeshToGlbScene(MdbMesh mdbMesh, SceneBuilder glbScene, IList<MaterialBuilder> glbMaterials)
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

        private static void WriteMeshToNewMdb(MdbMesh mdbMesh, string mdbFilePath)
        {
            using(var mdbWriter = new BinaryWriter(File.Open(mdbFilePath, FileMode.Create)))
            {
                float minX, minY, minZ, maxX, maxY, maxZ;
                minX = minY = minZ = float.MaxValue;
                maxX = maxY = maxZ = float.MinValue;
                mdbWriter.Write(0);
                mdbWriter.Write(0);
                mdbWriter.Write(0);
                mdbWriter.Write(mdbMesh.MeshModels.Count);
                for (int i = 0; i < mdbMesh.MeshModels.Count; i++)
                {
                    var mdbMeshModel = mdbMesh.MeshModels[i];
                    if(i == 0)
                    {
                        WriteMeshModelToMdb(mdbMeshModel, mdbWriter, i, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
                    }
                    else
                    {
                        WriteMeshModelToMdb(mdbMeshModel, mdbWriter, i);
                    }
                }
                mdbWriter.Write(mdbMesh.Materials.Count);
                for (int i = 0; i < mdbMesh.Materials.Count; i++)
                {
                    var mdbMaterial = mdbMesh.Materials[i];
                    WriteMaterialToMdb(mdbMaterial, mdbWriter);
                }
                mdbWriter.Write(0); // bones, not for now
                WriteBoundingValuesToMdb(minX, minY, minZ, maxX, maxY, maxZ, mdbWriter);
                var pos = mdbWriter.BaseStream.Position;
                mdbWriter.Write(0);
                mdbWriter.Write(1);
                if (mdbMesh.MeshModels.Count > 0)
                {
                    AutoGenerateCBox(mdbMesh.CollisionBox, mdbMesh.MeshModels[0], mdbMesh.MeshModels[0].MdbTriangles);
                    WriteCollisionBoxToMdb(mdbMesh.CollisionBox, mdbWriter, mdbMesh.MeshModels[0].MdbTriangles.Count);
                    mdbWriter.Write(17); //max level
                    mdbWriter.Write(5);
                    WriteHitboxToMdb(mdbMesh.MeshModels[0], mdbWriter);
                }
                else
                {
                    WriteCollisionBoxToMdb(mdbMesh.CollisionBox, mdbWriter, mdbMesh.MeshModels[0].MdbTriangles.Count);
                    mdbWriter.Write(17); //max level
                    mdbWriter.Write(0);
                    mdbWriter.Write(18);
                    mdbWriter.Write(0);
                }
                var currentPos = mdbWriter.BaseStream.Position;
                var blockLength0 = (int)(currentPos - pos);
                mdbWriter.Write(false); // true allows correct transparency (over nebula and objects), but breaks hitbox
                mdbWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                mdbWriter.Write(currentPos + 1);
                mdbWriter.Write((int)currentPos - 11);
                mdbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
                mdbWriter.Write(blockLength0);
                mdbWriter.BaseStream.Seek(0, SeekOrigin.End);
                WriteFinalStringsToMdb(mdbWriter);
            }
        }

        private static void WriteMeshModelToMdb(MdbMeshModel mdbMeshModel, BinaryWriter mdbWriter, int modelIndex)
        {
            var pos = mdbWriter.BaseStream.Position;
            mdbWriter.Write(0);
            mdbWriter.Write(modelIndex);
            mdbWriter.Write(mdbMeshModel.MdbVertices.Count);
            for (int i = 0; i < mdbMeshModel.MdbVertices.Count; i++)
            {
                var mdbVertice = mdbMeshModel.MdbVertices[i];
                mdbWriter.Write(32);
                mdbWriter.Write(mdbVertice.X);
                mdbWriter.Write(mdbVertice.Y);
                mdbWriter.Write(mdbVertice.Z);
                mdbWriter.Write(mdbVertice.U);
                mdbWriter.Write(mdbVertice.V);
                mdbWriter.Write(mdbVertice.NX);
                mdbWriter.Write(mdbVertice.NY);
                mdbWriter.Write(mdbVertice.R);
                mdbWriter.Write(mdbVertice.G);
                mdbWriter.Write(mdbVertice.B);
                mdbWriter.Write(mdbVertice.A);
            }
            mdbWriter.Write(mdbMeshModel.MdbTriangles.Count);
            for (int i = 0; i < mdbMeshModel.MdbTriangles.Count; i++)
            {
                var mdbTriangle = mdbMeshModel.MdbTriangles[i];
                mdbWriter.Write(8);
                mdbWriter.Write(mdbTriangle.P0);
                mdbWriter.Write(mdbTriangle.P1);
                mdbWriter.Write(mdbTriangle.P2);
                mdbWriter.Write(mdbTriangle.TextureIndex);
            }
            mdbWriter.Write(0); // animation stuff ?
            var blockLength = (int)(mdbWriter.BaseStream.Position - pos);
            mdbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            mdbWriter.Write(blockLength - 4);
            mdbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMeshModelToMdb(MdbMeshModel mdbMeshModel, BinaryWriter mdbWriter, int modelIndex, ref float minX, ref float minY, ref float minZ, ref float maxX, ref float maxY, ref float maxZ)
        {
            var pos = mdbWriter.BaseStream.Position;
            mdbWriter.Write(0);
            mdbWriter.Write(modelIndex);
            mdbWriter.Write(mdbMeshModel.MdbVertices.Count);
            for (int i = 0; i < mdbMeshModel.MdbVertices.Count; i++)
            {
                var mdbVertice = mdbMeshModel.MdbVertices[i];
                mdbWriter.Write(32);
                mdbWriter.Write(mdbVertice.X);
                mdbWriter.Write(mdbVertice.Y);
                mdbWriter.Write(mdbVertice.Z);
                mdbWriter.Write(mdbVertice.U);
                mdbWriter.Write(mdbVertice.V);
                mdbWriter.Write(mdbVertice.NX);
                mdbWriter.Write(mdbVertice.NY);
                mdbWriter.Write(mdbVertice.R);
                mdbWriter.Write(mdbVertice.G);
                mdbWriter.Write(mdbVertice.B);
                mdbWriter.Write(mdbVertice.A);
                if (mdbVertice.X > maxX)
                    maxX = mdbVertice.X;
                if (mdbVertice.Y > maxY)
                    maxY = mdbVertice.Y;
                if (mdbVertice.Z > maxZ)
                    maxZ = mdbVertice.Z;
                if (mdbVertice.X < minX)
                    minX = mdbVertice.X;
                if (mdbVertice.Y < minY)
                    minY = mdbVertice.Y;
                if (mdbVertice.Z < minZ)
                    minZ = mdbVertice.Z;
            }
            mdbWriter.Write(mdbMeshModel.MdbTriangles.Count);
            for (int i = 0; i < mdbMeshModel.MdbTriangles.Count; i++)
            {
                var mdbTriangle = mdbMeshModel.MdbTriangles[i];
                mdbWriter.Write(8);
                mdbWriter.Write(mdbTriangle.P0);
                mdbWriter.Write(mdbTriangle.P1);
                mdbWriter.Write(mdbTriangle.P2);
                mdbWriter.Write(mdbTriangle.TextureIndex);
            }
            mdbWriter.Write(0); // animation stuff ?
            var blockLength = (int)(mdbWriter.BaseStream.Position - pos);
            mdbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            mdbWriter.Write(blockLength - 4);
            mdbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteMaterialToMdb(MdbMaterial mdbMaterial, BinaryWriter mdbWriter)
        {
            var pos = mdbWriter.BaseStream.Position;
            mdbWriter.Write(0);
            mdbWriter.Write(mdbMaterial.TextureName.Length);
            mdbWriter.Write(Encoding.Default.GetBytes(mdbMaterial.TextureName));
            mdbWriter.Write(1.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(0.0f);
            mdbWriter.Write(0.0f);
            mdbWriter.Write(0.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(0.0f);
            mdbWriter.Write(0.0f);
            mdbWriter.Write(0.0f);
            mdbWriter.Write(1.0f);
            mdbWriter.Write(0.0f);
            mdbWriter.Write(0.0f);
            var blockLength = (int)(mdbWriter.BaseStream.Position - pos);
            mdbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            mdbWriter.Write(blockLength - 4);
            mdbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteBoundingValuesToMdb(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, BinaryWriter mdbWriter)
        {
            var posx = (minX + maxX) / 2;
            var posy = (minY + maxY) / 2;
            var posz = (minZ + maxZ) / 2;
            var lenx = maxX - minX;
            var leny = maxY - minY;
            var lenz = maxZ - minZ;
            //data block
            mdbWriter.Write(minX);
            mdbWriter.Write(-maxZ);
            mdbWriter.Write(minY);
            mdbWriter.Write(maxX);
            mdbWriter.Write(-minZ);
            mdbWriter.Write(maxY);
            mdbWriter.Write(posx);
            mdbWriter.Write(-posz);
            mdbWriter.Write(posy);
            var diag = (float)Math.Sqrt(lenx * lenx + leny * leny + lenz * lenz) / 2;
            mdbWriter.Write(diag);
        }

        private static void WriteCollisionBoxToMdb(CollisionBox box, BinaryWriter mdbWriter, int collisionTrianglesCount)
        {
            var pos = mdbWriter.BaseStream.Position;
            mdbWriter.Write(0);
            mdbWriter.Write(2);
            mdbWriter.Write(72);
            mdbWriter.Write(3);
            mdbWriter.Write(box.Position.X);
            mdbWriter.Write(box.Position.Y);
            mdbWriter.Write(box.Position.Z);
            mdbWriter.Write(4);
            mdbWriter.Write(0);
            mdbWriter.Write(5);
            mdbWriter.Write(box.OCross.X);
            mdbWriter.Write(box.OCross.Y);
            mdbWriter.Write(box.OCross.Z);
            mdbWriter.Write(6);
            mdbWriter.Write(box.OUp.X);
            mdbWriter.Write(box.OUp.Y);
            mdbWriter.Write(box.OUp.Z);
            mdbWriter.Write(7);
            mdbWriter.Write(box.OForward.X);
            mdbWriter.Write(box.OForward.Y);
            mdbWriter.Write(box.OForward.Z);
            mdbWriter.Write(8);
            mdbWriter.Write(box.Length.X);
            mdbWriter.Write(box.Length.Y);
            mdbWriter.Write(box.Length.Z);
            mdbWriter.Write(9);
            mdbWriter.Write(Math.Max(Math.Max(box.Length.X, box.Length.Y), box.Length.Z));
            mdbWriter.Write(10);
            mdbWriter.Write(box.Level);
            mdbWriter.Write(11);
            if (box.Leftchild != null)
            {
                mdbWriter.Write(true);
                mdbWriter.Write(12);
                if (box.Rightchild != null)
                {
                    mdbWriter.Write(true);
                    mdbWriter.Write(13);
                    WriteCollisionBoxToMdb(box.Leftchild, mdbWriter, collisionTrianglesCount);
                    mdbWriter.Write(16);
                    WriteCollisionBoxToMdb(box.Rightchild, mdbWriter, collisionTrianglesCount);
                }
                else
                {
                    mdbWriter.Write(false);
                    mdbWriter.Write(13);
                    WriteCollisionBoxToMdb(box.Leftchild, mdbWriter, collisionTrianglesCount);
                }
            }
            else
            {
                mdbWriter.Write(false);
                mdbWriter.Write(12);
                if (box.Rightchild != null)
                {
                    mdbWriter.Write(true);
                    mdbWriter.Write(16);
                    WriteCollisionBoxToMdb(box.Rightchild, mdbWriter, collisionTrianglesCount);
                }
                else
                {
                    mdbWriter.Write(false);
                }
            }
            mdbWriter.Write(14);
            if (box.Level != 0)
                mdbWriter.Write(0);
            else
            {
                mdbWriter.Write(collisionTrianglesCount);
                for (int i = 0; i < collisionTrianglesCount; i++)
                {
                    mdbWriter.Write(15);
                    mdbWriter.Write(i);
                }
            }
            var blockLength = (int)(mdbWriter.BaseStream.Position - pos);
            mdbWriter.BaseStream.Seek(pos, SeekOrigin.Begin);
            mdbWriter.Write(blockLength - 4);
            mdbWriter.BaseStream.Seek(0, SeekOrigin.End);
        }

        private static void WriteHitboxToMdb(MdbMeshModel mdbMeshModel, BinaryWriter mdbWriter)
        {
            mdbWriter.Write(18);
            mdbWriter.Write(mdbMeshModel.MdbTriangles.Count);
            for (int i = 0; i < mdbMeshModel.MdbTriangles.Count; i++)
            {
                var mdbTriangle = mdbMeshModel.MdbTriangles[i];
                var p0 = mdbMeshModel.MdbVertices[mdbTriangle.P0];
                var p1 = mdbMeshModel.MdbVertices[mdbTriangle.P1];
                var p2 = mdbMeshModel.MdbVertices[mdbTriangle.P2];
                mdbWriter.Write(19);
                mdbWriter.Write(48);
                mdbWriter.Write(20);
                mdbWriter.Write(p0.X);
                mdbWriter.Write(p0.Y);
                mdbWriter.Write(p0.Z);
                mdbWriter.Write(21);
                mdbWriter.Write(p1.X);
                mdbWriter.Write(p1.Y);
                mdbWriter.Write(p1.Z);
                mdbWriter.Write(22);
                mdbWriter.Write(p2.X);
                mdbWriter.Write(p2.Y);
                mdbWriter.Write(p2.Z);
            }
        }

        private static void WriteFinalStringsToMdb(BinaryWriter mdbWriter)
        {
            mdbWriter.Write(21);
            mdbWriter.Write(8);
            mdbWriter.Write(Encoding.Default.GetBytes("MeshData"));
            mdbWriter.Write(4);
            mdbWriter.Write(Encoding.Default.GetBytes("Root"));
            mdbWriter.Write(10);
            mdbWriter.Write(Encoding.Default.GetBytes("LocalBasis"));
            mdbWriter.Write(8);
            mdbWriter.Write(Encoding.Default.GetBytes("Position"));
            mdbWriter.Write(20);
            mdbWriter.Write(Encoding.Default.GetBytes("LookAt Vector Length"));
            mdbWriter.Write(19);
            mdbWriter.Write(Encoding.Default.GetBytes("Orientation - Cross"));
            mdbWriter.Write(21);
            mdbWriter.Write(Encoding.Default.GetBytes("Orientation - Forward"));
            mdbWriter.Write(16);
            mdbWriter.Write(Encoding.Default.GetBytes("Orientation - Up"));
            mdbWriter.Write(6);
            mdbWriter.Write(Encoding.Default.GetBytes("Length"));
            mdbWriter.Write(6);
            mdbWriter.Write(Encoding.Default.GetBytes("Radius"));
            mdbWriter.Write(5);
            mdbWriter.Write(Encoding.Default.GetBytes("Level"));
            mdbWriter.Write(12);
            mdbWriter.Write(Encoding.Default.GetBytes("HasLeftChild"));
            mdbWriter.Write(13);
            mdbWriter.Write(Encoding.Default.GetBytes("HasRightChild"));
            mdbWriter.Write(39);
            mdbWriter.Write(Encoding.Default.GetBytes("Valid Collision Triangle Indices - Size"));
            mdbWriter.Write(42);
            mdbWriter.Write(Encoding.Default.GetBytes("Valid Collision Triangle Indices - Element"));
            mdbWriter.Write(8);
            mdbWriter.Write(Encoding.Default.GetBytes("MaxLevel"));
            mdbWriter.Write(25);
            mdbWriter.Write(Encoding.Default.GetBytes("CollisionTriangles - Size"));
            mdbWriter.Write(28);
            mdbWriter.Write(Encoding.Default.GetBytes("CollisionTriangles - Element"));
            mdbWriter.Write(2);
            mdbWriter.Write(Encoding.Default.GetBytes("P0"));
            mdbWriter.Write(2);
            mdbWriter.Write(Encoding.Default.GetBytes("P1"));
            mdbWriter.Write(2);
            mdbWriter.Write(Encoding.Default.GetBytes("P2"));

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

        private static bool PointsEquals(int[] p0, int[] p1)
        {
            if (p0.Length == p1.Length)
            {
                for (int i = 0; i < p0.Length; i++)
                {
                    if (p0[i] != p1[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        private static void AutoGenerateCBox(CollisionBox box, MdbMeshModel mdbMeshModel, IList<MdbTriangle> triangles)
        {
            var points = GetPointsFromCBoxGroup(triangles);
            AllPca(box, mdbMeshModel, points, out Vector3 mean);
            if (box.Level < 5)
            {
                box.Leftchild = new CollisionBox() { Level = box.Level + 1 };
                box.Rightchild = new CollisionBox() { Level = box.Level + 1 };
                IList<MdbTriangle> leftChild = [];
                IList<MdbTriangle> rightChild = [];
                var maxLength = Math.Max(box.Length.X, Math.Max(box.Length.Y, box.Length.Z));
                if (maxLength == box.Length.X)
                {
                    var tempPosX = box.OCross.X * mean.X + box.OCross.Y * mean.Y + box.OCross.Z * mean.Z;
                    for (int i = 0; i < triangles.Count; i++)
                    {
                        var tri = triangles[i];
                        MdbVertex p0 = mdbMeshModel.MdbVertices[tri.P0], p1 = mdbMeshModel.MdbVertices[tri.P1], p2 = mdbMeshModel.MdbVertices[tri.P2];
                        var center = new Vector3((Math.Min(Math.Min(p0.X, p1.X), p2.X) + Math.Max(Math.Max(p0.X, p1.X), p2.X)) / 2,
                                                 (Math.Min(Math.Min(p0.Y, p1.Y), p2.Y) + Math.Max(Math.Max(p0.Y, p1.Y), p2.Y)) / 2,
                                                 (Math.Min(Math.Min(p0.Z, p1.Z), p2.Z) + Math.Max(Math.Max(p0.Z, p1.Z), p2.Z)) / 2);
                        var centerTempX = box.OCross.X * center.X + box.OCross.Y * center.Y + box.OCross.Z * center.Z;
                        if (centerTempX < tempPosX)
                            leftChild.Add(tri);
                        else
                            rightChild.Add(tri);
                    }
                    AutoGenerateCBox(box.Leftchild, mdbMeshModel, leftChild);
                    AutoGenerateCBox(box.Rightchild, mdbMeshModel, rightChild);
                }
                else if (maxLength == box.Length.Y)
                {
                    var tempPosY = box.OUp.X * mean.X + box.OUp.Y * mean.Y + box.OUp.Z * mean.Z;
                    for (int i = 0; i < triangles.Count; i++)
                    {
                        var tri = triangles[i];
                        MdbVertex p0 = mdbMeshModel.MdbVertices[tri.P0], p1 = mdbMeshModel.MdbVertices[tri.P1], p2 = mdbMeshModel.MdbVertices[tri.P2];
                        var center = new Vector3((Math.Min(Math.Min(p0.X, p1.X), p2.X) + Math.Max(Math.Max(p0.X, p1.X), p2.X)) / 2,
                                                 (Math.Min(Math.Min(p0.Y, p1.Y), p2.Y) + Math.Max(Math.Max(p0.Y, p1.Y), p2.Y)) / 2,
                                                 (Math.Min(Math.Min(p0.Z, p1.Z), p2.Z) + Math.Max(Math.Max(p0.Z, p1.Z), p2.Z)) / 2);
                        var centerTempY = box.OUp.X * center.X + box.OUp.Y * center.Y + box.OUp.Z * center.Z;
                        if (centerTempY < tempPosY)
                            leftChild.Add(tri);
                        else
                            rightChild.Add(tri);
                    }
                    AutoGenerateCBox(box.Leftchild, mdbMeshModel, leftChild);
                    AutoGenerateCBox(box.Rightchild, mdbMeshModel, rightChild);
                }
                else
                {
                    var tempPosZ = box.OForward.X * mean.X + box.OForward.Y * mean.Y + box.OForward.Z * mean.Z;
                    for (int i = 0; i < triangles.Count; i++)
                    {
                        var tri = triangles[i];
                        MdbVertex p0 = mdbMeshModel.MdbVertices[tri.P0], p1 = mdbMeshModel.MdbVertices[tri.P1], p2 = mdbMeshModel.MdbVertices[tri.P2];
                        var center = new Vector3((Math.Min(Math.Min(p0.X, p1.X), p2.X) + Math.Max(Math.Max(p0.X, p1.X), p2.X)) / 2,
                                                 (Math.Min(Math.Min(p0.Y, p1.Y), p2.Y) + Math.Max(Math.Max(p0.Y, p1.Y), p2.Y)) / 2,
                                                 (Math.Min(Math.Min(p0.Z, p1.Z), p2.Z) + Math.Max(Math.Max(p0.Z, p1.Z), p2.Z)) / 2);
                        var centerTempZ = box.OForward.X * center.X + box.OForward.Y * center.Y + box.OForward.Z * center.Z;
                        if (centerTempZ < tempPosZ)
                            leftChild.Add(tri);
                        else
                            rightChild.Add(tri);
                    }
                    AutoGenerateCBox(box.Leftchild, mdbMeshModel, leftChild);
                    AutoGenerateCBox(box.Rightchild, mdbMeshModel, rightChild);
                }
            }
        }

        private static IList<int> GetPointsFromCBoxGroup(IList<MdbTriangle> mdbTriangles)
        {
            IList<int> points = [];
            for (int i = 0; i < mdbTriangles.Count; i++)
            {
                var tri = mdbTriangles[i];
                bool hasFoundP0, hasFoundP1, hasFoundP2;
                hasFoundP0 = hasFoundP1 = hasFoundP2 = false;
                for (int k = 0; k < points.Count; k++)
                {
                    var p = points[k];
                    if (p == tri.P0)
                    {
                        hasFoundP0 = true;
                        if (hasFoundP0 && hasFoundP1 && hasFoundP2)
                            break;
                    }
                    if (p == tri.P1)
                    {
                        hasFoundP1 = true;
                        if (hasFoundP0 && hasFoundP1 && hasFoundP2)
                            break;
                    }
                    if (p == tri.P2)
                    {
                        hasFoundP2 = true;
                        if (hasFoundP0 && hasFoundP1 && hasFoundP2)
                            break;
                    }
                }
                if (!hasFoundP0)
                    points.Add(tri.P0);
                if (!hasFoundP1)
                    points.Add(tri.P1);
                if (!hasFoundP2)
                    points.Add(tri.P2);
            }
            return points;
        }
        
        private static void AllPca(CollisionBox box, MdbMeshModel mdbMeshModel, IList<int> points, out Vector3 mean)
        {
            for (int i = 0; i < points.Count; i++)
            {
                try
                {
                    var p = mdbMeshModel.MdbVertices[points[i]];
                    box.Position.X += p.X;
                    box.Position.Y += p.Y;
                    box.Position.Z += p.Z;
                }
                catch { }
            }
            if (points.Count != 0)
            {
                box.Position /= points.Count;
            }
            mean = box.Position;
            Vector3 covMatRow1 = Vector3.Zero, covMatRow2 = covMatRow1, covMatRow3 = covMatRow1;
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                try
                {
                    var p = mdbMeshModel.MdbVertices[point];
                    covMatRow1.X += (p.X - box.Position.X) * (p.X - box.Position.X);
                    covMatRow1.Y += (p.X - box.Position.X) * (p.Y - box.Position.Y);
                    covMatRow1.Z += (p.X - box.Position.X) * (p.Z - box.Position.Z);
                    covMatRow2.Y += (p.Y - box.Position.Y) * (p.Y - box.Position.Y);
                    covMatRow2.Z += (p.Y - box.Position.Y) * (p.Z - box.Position.Z);
                    covMatRow3.Z += (p.Z - box.Position.Z) * (p.Z - box.Position.Z);
                }
                catch { }
            }
            if (points.Count != 0)
            {
                covMatRow1 /= points.Count;
                covMatRow2 /= points.Count;
                covMatRow3 /= points.Count;
            }
            //Symetry in the covariance matrix
            covMatRow2.X = covMatRow1.Y;
            covMatRow3.X = covMatRow1.Z;
            covMatRow3.Y = covMatRow2.Z;
            var eigenValues = MatrixEigenStuff.EigenValues(covMatRow1, covMatRow2, covMatRow3);
            box.OCross = Vector3.Normalize(MatrixEigenStuff.EigenVector(covMatRow1, covMatRow2, eigenValues.X));
            box.OUp = Vector3.Normalize(MatrixEigenStuff.EigenVector(covMatRow1, covMatRow2, eigenValues.Z));
            box.OForward = Vector3.Cross(box.OCross, box.OUp);
            Vector3 tempx = new Vector3(box.OCross.X, box.OUp.X, box.OForward.X),
                    tempy = new Vector3(box.OCross.Y, box.OUp.Y, box.OForward.Y),
                    tempz = new Vector3(box.OCross.Z, box.OUp.Z, box.OForward.Z);
            float minx, miny, minz, maxx, maxy, maxz;
            minx = miny = minz = float.MaxValue;
            maxx = maxy = maxz = float.MinValue;

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                try
                {
                    var p = mdbMeshModel.MdbVertices[point];
                    float vTempx = tempx.X * p.X + tempy.X * p.Y + tempz.X * p.Z,
                          vTempy = tempx.Y * p.X + tempy.Y * p.Y + tempz.Y * p.Z,
                          vTempz = tempx.Z * p.X + tempy.Z * p.Y + tempz.Z * p.Z;
                    if (vTempx < minx)
                        minx = vTempx;
                    if (vTempy < miny)
                        miny = vTempy;
                    if (vTempz < minz)
                        minz = vTempz;
                    if (vTempx > maxx)
                        maxx = vTempx;
                    if (vTempy > maxy)
                        maxy = vTempy;
                    if (vTempz > maxz)
                        maxz = vTempz;
                }
                catch { }
            }

            box.Position.X = (minx + maxx) / 2;
            box.Position.Y = (miny + maxy) / 2;
            box.Position.Z = (minz + maxz) / 2;
            box.Length.X = (maxx - minx) / 2;
            box.Length.Y = (maxy - miny) / 2;
            box.Length.Z = (maxz - minz) / 2;
            var tempPos = box.Position;
            box.Position.X = box.OCross.X * tempPos.X + box.OUp.X * tempPos.Y + box.OForward.X * tempPos.Z;
            box.Position.Y = box.OCross.Y * tempPos.X + box.OUp.Y * tempPos.Y + box.OForward.Y * tempPos.Z;
            box.Position.Z = box.OCross.Z * tempPos.X + box.OUp.Z * tempPos.Y + box.OForward.Z * tempPos.Z;
        }
    }
}

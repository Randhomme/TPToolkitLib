# TPToolkitLib

## About

Welcome to TPToolkitLib, a project meant to be used to read and write files from the game Treasure Planet: Battle at Procyon.<br>
It currently supports the following file types : mdb (mesh), msb (mesh scene) and wot (world object).<br>
The library can also read the game strings, and knows about pretty much anything else there is to know about your TPGame folder.

## Usage

The best way to get started is by using the "tool" classes. There is a tool class for each file type (mdb, msb and wot).<br>
The tool classes are static, and provide public static methods to read and write files in an easy way.

```C#
using TPToolkitLib.Tools;

public static void Main()
{
    // Populate read only lists, available using the static class TPGameTool
    // The lists contain the world objects, the mesh scenes, the meshes (as strings),
    // the game strings, and more
    TPGameTool.LoadTPGameFolder("Path/To/TPGameFolder");

    // Read multiple meshes (.mdb) and combine them into a new 3D file (.glb format recommended)
    // Choose your mdb files, your output file path, the texture directory (not needed)
    // and chose if you want to include the lods
    string[] mdbFilePaths = ["Path/To/ShipLayer1.mdb", "Path/To/ShipLayer2.mdb"];
    string glbFilePath = "Path/To/Ship.glb";
    string textureDirectory = "Path/To/TPGameFolder/BD_Textures";
    bool lods = true;
    XMdbTo1Glb(mdbFilePaths, glbFilePath, textureDirectory, lods);

    // You can then read your 3D file (you can read multiple file at the same time)
    // and generate the mesh files from it
    string[] glbFilePaths = ["Path/To/Ship1.glb", "Path/To/Ship2.glb"];
    string meshDirectory = "Path/To/TPGameFolder/BD_Meshes";
    XGlbToXMdb(glbFilePaths, meshDirectory);

    // Read and write mesh scene files, and even combine mesh scenes into one
    // You can of course access the properties of an MsbScene object to modify it
    string msbFilePath1 = "Path/To/MeshScene1.msb";
    string msbFilePath2 = "Path/To/MeshScene2.msb";
    string[] msbFilePaths = [msbFilePath1, msbFilePath2];
    MsbScene msbScene = MeshSceneFromMsb(msbFilePath1);
    ImportMsbSceneFromMsb(msbScene, msbFilePath2);
    MeshSceneToMsb(msbScene, "Path/To/NewMeshScene.msb");

    // Read and write world object files
    // Access the properties to mofify your world object as you want
    WorldObjectType wot = WorldObjectFromWot("Path/To/WorldObject.wot");
    WorldObjectToWot(wot, "Path/To/NewWorldObject.wot");
}
```

## Main types

### Tool classes (static)

- `TPGameTool`
- `MdbTool`
- `MsbTool`
- `WotTool`

### Data classes

- `MdbMesh`
- `MsbScene`
- `WorldObjectType`

## Documentation

You can learn more about the classes and methods available in this library at<br>
https://github.com/Randhomme/TPToolkitLib/wiki

## Found a bug ?

Report a bug by opening an issue on the github repository :<br>
https://github.com/Randhomme/TPToolkitLib
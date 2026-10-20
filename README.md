# TPToolkitLib

Welcome to TPToolkitLib, a project meant to be used to read and write files from the game Treasure Planet: Battle at Procyon.

This repository contains 2 projects :
- TPToolkitLib : A class library written in C#. It is the main project, and is automatically built and published as a NuGet package using a Github Action script. See the project's [README](TPToolkitLib/README.md) for more information.
- TPToolkitTestUI : A UI written in C# WPF. It's only purpose is to quickly test functions from the library in developement, using the library as a project reference. This project is not built and included in the NuGet package.

Since the class library `TPToolkitLib` is published as a NuGet package, it is usable in any other project. I strongly recommend you to read the project's [README](TPToolkitLib/README.md) to get started with it.

To build and run the class library and the test UI yourself, clone this repository (or download it), then open `TPToolkitLib.slnx` in VisualStudio. Set `TPToolkitTestUI` as startup project to automatically launch the test UI.
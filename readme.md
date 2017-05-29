Fruiton Client
========

This is a repo for the Unity Client part of Fruiton game.

Prerequisites:
-------------
- [Unity 5.5.1](https://unity3d.com/get-unity/download/archive)
- [Protobuf Compiler (protoc) 3.3.0](https://github.com/google/protobuf/releases/tag/v3.3.0)
    - Direct links:
    [Windows](https://github.com/google/protobuf/releases/download/v3.3.0/protoc-3.3.0-win32.zip),
    [Linux 64bit](https://github.com/google/protobuf/releases/download/v3.3.0/protoc-3.3.0-linux-x86_64.zip),
    [OS X 64bit](https://github.com/google/protobuf/releases/download/v3.3.0/protoc-3.3.0-osx-x86_64.zip).
    - For Windows: This tool is used by the build script, so either move it to the directory
    that is in your `PATH` enviroment variable (recommended if you're also planning to work on the game server)
    or move it to the project root folder, as the build script is located there
- [.proto files (Protobuf Definitions)](http://prak.mff.cuni.cz:7990/projects/FRUIT/repos/protobufs)
    - Git url: [`http://prak.mff.cuni.cz:7990/scm/fruit/protobufs.git`](http://prak.mff.cuni.cz:7990/scm/fruit/protobufs.git)
- [Fruiton Haxe Kernel](http://prak.mff.cuni.cz:7990/projects/FRUIT/repos/kernel/browse)
    - Git url: [`http://prak.mff.cuni.cz:7990/scm/fruit/kernel.git`](http://prak.mff.cuni.cz:7990/scm/fruit/kernel.git)
- Enviroment Variables
    - `FRUITON_PROTOBUFS` - set this variable to absolute path of the folder containing .proto files
    - `FRUITON_KERNEL` - set this variable to absolute path of the folder containing Fruiton Haxe Kernel


Note: On Windows enviroment variables can be set using the command `SETX`,
e.g. `SETX FRUITON_PROTOBUFS C:/Users/Username/Documents/Fruiton/Protobufs`

Compiling .proto files and Haxe Kernel
--------------
#### Windows
1. Make sure the enviroment variable `FRUITON_PROTOBUFS` is pointing to the folder with your .proto files and
that they are up to date.
2. Make sure the enviroment variable `FRUITON_KERNEL` is pointing to the folder with your Fruiton Haxe Kernel
project and that it is up to date.
3. Run the `build.bat` script found in the root folder.

@echo off

echo Compiling Kernel...

IF NOT DEFINED FRUITON_KERNEL (
    echo Enviroment variable FRUITON_KERNEL is not defined!
    echo Please set its value to the abolute path of the folder containing Kernel Haxe sources.
    echo You can set the enviroment variable by using the SETX command
    echo e.g. SETX FRUITON_KERNEL C:\Users\Username\Documents\Fruiton\Kernel
    pause
    exit /b
)

SET kernel_dest=FruitonClient\Assets\Scripts\Kernel\Generated

IF NOT EXIST %kernel_dest% mkdir %kernel_dest%

haxe -D no-compilation -cs %kernel_dest% -cp %FRUITON_KERNEL% Kernel.hx

IF NOT DEFINED FRUITON_PROTOBUFS (
    echo Enviroment variable FRUITON_PROTOBUFS is not defined!
    echo Please set its value to the abolute path of the folder containing .proto files.
    echo You can set the enviroment variable by using the SETX command
    echo e.g. SETX FRUITON_PROTOBUFS C:\Users\Username\Documents\Fruiton\Protobufs
    pause
    exit /b
)

echo Compiling .proto files...

SET proto_dest=FruitonClient\Assets\Scripts\Protobufs

IF NOT EXIST %proto_dest% mkdir %proto_dest%

FOR %%p in (%FRUITON_PROTOBUFS%\*.proto) DO (
  protoc --csharp_out=%proto_dest% --proto_path="%FRUITON_PROTOBUFS%" %%p
  echo %%p compiled!
)

echo Done!

pause
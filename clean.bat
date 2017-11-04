echo "Cleaning..."

:: Remove develop directories
del /s /q FruitonClient\Assets\Scripts\Kernel
del /s /q FruitonClient\Assets\Scripts\Protobufs

:: Remove changeable libraries
del /q FruitonClient\Assets\Libraries\protobufs.dll
del /q FruitonClient\Assets\Libraries\kernel.dll

:: Remove resources
del /s /q FruitonClient\Assets\Resources\Kernel

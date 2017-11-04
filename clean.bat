@echo off
echo Cleaning...

:: Remove develop directories
del /s /q FruitonClient\Assets\Scripts\Kernel >nul 2>&1
del /s /q FruitonClient\Assets\Scripts\Protobufs >nul 2>&1

:: Remove changeable libraries
del /q FruitonClient\Assets\Libraries\protobufs.dll >nul 2>&1
del /q FruitonClient\Assets\Libraries\kernel.dll >nul 2>&1

:: Remove resources
del /s /q FruitonClient\Assets\Resources\Kernel >nul 2>&1

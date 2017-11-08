#!/bin/bash

echo "Cleaning..."

# Remove develop directories
rm -rf FruitonClient/Assets/Scripts/Kernel 2> /dev/null
rm -rf FruitonClient/Assets/Scripts/Protobufs 2> /dev/null

# Remove changeable libraries
rm FruitonClient/Assets/Libraries/protobufs.dll 2> /dev/null
rm FruitonClient/Assets/Libraries/kernel.dll 2> /dev/null

# Remove resources
rm -rf FruitonClient/Assets/Resources/Kernel 2> /dev/null

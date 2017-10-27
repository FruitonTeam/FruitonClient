#!/bin/bash

# Kernel
echo "Compiling kernel..."

FRUITON_CLIENT_LOC=`pwd`
KERNEL_DEST=${FRUITON_CLIENT_LOC}/FruitonClient/Assets/Scripts/Kernel/Generated
mkdir -p ${KERNEL_DEST}

UNITY_RESOURCES_LOC=${FRUITON_CLIENT_LOC}/FruitonClient/Assets/Resources
mkdir -p ${UNITY_RESOURCES_LOC}

cd ${KERNEL_LOC}

haxe --macro include\(\'fruiton\'\,true\,\[\'fruiton.fruitDb.models\'\,\'fruiton.macro\'\]\) -D no-compilation -D no-root -cs ${KERNEL_DEST} -cp ${KERNEL_LOC} fruiton.kernel.Kernel

cp -r ${KERNEL_LOC}/resources/* ${UNITY_RESOURCES_LOC}
cd ${FRUITON_CLIENT_LOC}


# Protobufs
echo "Compiling protobufs..."

PROTO_DEST="FruitonClient/Assets/Scripts/Protobufs"
mkdir -p ${PROTO_DEST}

for proto_file in ${PROTO_LOC}/*.proto; do
    protoc --csharp_out=${PROTO_DEST} --proto_path=${PROTO_LOC} ${proto_file}
done

#!/bin/bash

mkdir -p ${KERNEL_DEST}

cd ${KERNEL_LOC}

haxe --macro include\(\'fruiton\'\,true\,\[\'fruiton.fruitDb.models\'\,\'fruiton.macro\'\]\) -D no-compilation -cs ${KERNEL_DEST} -cp ${KERNEL_LOC} fruiton.kernel.Kernel

cp -r ${KERNEL_LOC}/resources ${KERNEL_DEST}/resources

mkdir -p ${PROTO_DEST}

for proto_file in ${PROTO_LOC}/*.proto; do
	protoc --csharp_out=${PROTO_DEST} --proto_path=${PROTO_LOC} ${proto_file}
done

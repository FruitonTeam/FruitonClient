#!/bin/bash

mkdir -p ${KERNEL_DEST}

haxe -D no-compilation -cs ${KERNEL_DEST} -cp ${KERNEL_LOC} fruiton.kernel.Kernel

mkdir -p ${PROTO_DEST}

for proto_file in ${PROTO_LOC}/*.proto; do
	protoc --csharp_out=${PROTO_DEST} --proto_path=${PROTO_LOC} ${proto_file}
done

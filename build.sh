#!/bin/bash

# Google Secret
if [ -n "$FRUITON_GOOGLE_SECRET" ]; then
	echo "$FRUITON_GOOGLE_SECRET" > FruitonClient/Assets/Resources/Google/google_client_secret.txt
else
	echo "Build failed: No Google secret specified!"
	exit 1
fi

./clean.sh

. pom.properties

# Protobufs
echo "Downloading protobufs library..."
rm protobufs-${protobufsVersion}.dll 2> /dev/null

wget --header="User-Agent: Mozilla/5.0 (Windows NT 5.1; rv:23.0) Gecko/20100101 Firefox/23.0" http://prak.mff.cuni.cz:8081/artifactory/libs-release/protobufs-${protobufsVersion}.dll

mv protobufs-${protobufsVersion}.dll FruitonClient/Assets/Libraries/protobufs.dll

# Kernel
echo "Downloading kernel library..."
rm kernel-${kernelVersion}.dll 2> /dev/null

wget --header="User-Agent: Mozilla/5.0 (Windows NT 5.1; rv:23.0) Gecko/20100101 Firefox/23.0" http://prak.mff.cuni.cz:8081/artifactory/libs-release/kernel-${kernelVersion}.dll

mv kernel-${kernelVersion}.dll FruitonClient/Assets/Libraries/kernel.dll

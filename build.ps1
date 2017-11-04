$properties = Get-Content "pom.properties" | Out-String | ConvertFrom-StringData;

# Protobufs
Write-Output "Downloading protobufs library...";
$protoFile = "protobufs-$($properties.protobufsVersion).dll";
if (Test-Path $protoFile) {
    Remove-Item $protoFile;
}

$client = new-object System.Net.WebClient
$client.DownloadFile("http://prak.mff.cuni.cz:8081/artifactory/libs-release/protobufs-$($properties.protobufsVersion).dll", $protoFile);

Move-Item -force $protoFile FruitonClient/Assets/Libraries/protobufs.dll

# Kernel
Write-Output "Downloading kernel library...";
$kernelFile = "protobufs-$($properties.kernelVersion).dll";
if (Test-Path $kernelFile) {
    Remove-Item $kernelFile;
}

$client.DownloadFile("http://prak.mff.cuni.cz:8081/artifactory/libs-release/kernel-$($properties.kernelVersion).dll", $kernelFile);

Move-Item -force $kernelFile FruitonClient/Assets/Libraries/kernel.dll

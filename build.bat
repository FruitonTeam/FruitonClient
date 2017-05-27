@echo off
SET proto_dest=FruitonClient\Assets\Scripts\Protobufs
IF NOT EXIST %proto_dest% mkdir %proto_dest%
FOR %%p in (%FRUITON_PROTOBUFS%\*.proto) DO (
  protoc --csharp_out=%proto_dest% --proto_path="%FRUITON_PROTOBUFS%" %%p
  echo %%p compiled!
)
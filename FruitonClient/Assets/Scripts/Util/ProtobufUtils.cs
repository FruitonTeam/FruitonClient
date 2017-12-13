using Google.Protobuf;

namespace Util
{
    public static class ProtobufUtils
    {
        
        public static byte[] GetBinaryData(IMessage protobuf)
        {
            var binaryData = new byte[protobuf.CalculateSize()];
            var stream = new CodedOutputStream(binaryData);
            protobuf.WriteTo(stream);

            return binaryData;
        }
        
    }
}
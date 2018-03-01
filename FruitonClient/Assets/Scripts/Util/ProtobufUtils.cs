using Google.Protobuf;

namespace Util
{
    /// <summary>
    /// Class containing helper methods for protobufs.
    /// </summary>
    public static class ProtobufUtils
    {
        /// <summary>
        /// Converts protobuf message to binary data.
        /// </summary>
        /// <param name="protobuf">protobuf message to convert</param>
        /// <returns>binary data representing given protobuf message</returns>
        public static byte[] GetBinaryData(IMessage protobuf)
        {
            var binaryData = new byte[protobuf.CalculateSize()];
            var stream = new CodedOutputStream(binaryData);
            protobuf.WriteTo(stream);

            return binaryData;
        }
        
    }
}
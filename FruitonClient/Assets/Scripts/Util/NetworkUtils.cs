using System.Collections.Generic;

namespace Util
{
    public static class NetworkUtils
    {
        /// <summary>
        /// Creates headers dictionary with `Content-Type` key.
        /// </summary>
        /// <param name="useProtobuf">true for protobuf content type, false for json</param>
        /// <returns>created headers dictionary</returns>
        public static Dictionary<string, string> CreateRequestHeaders(bool useProtobuf)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (useProtobuf)
            {
                headers.Add("Content-Type", "application/x-protobuf");
            }
            else
            {
                headers.Add("Content-Type", "application/json");
            }
            return headers;
        }
    }
}
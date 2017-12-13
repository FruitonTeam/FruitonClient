using System.Collections.Generic;

namespace Util
{
    public static class NetworkUtils
    {
        public static Dictionary<string, string> GetRequestHeaders(bool useProtobuf)
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
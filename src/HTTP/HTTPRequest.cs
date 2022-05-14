using System;
using System.Collections.Generic;
using System.Text;

namespace libicecast.HTTP
{
    /// <summary>
    /// A HTTP & HTTP-like request
    /// </summary>
    public class HTTPRequest
    {
        // Client
        /// <summary>
        /// The access method (or null)
        /// </summary>
        public string AccessMethod;
        /// <summary>
        /// The access path (or null)
        /// </summary>
        public string AccessPath;

        // Server
        /// <summary>
        /// The response code (or -1)
        /// </summary>
        public int ResponseCode;
        /// <summary>
        /// The response string (or null)
        /// </summary>
        public string ResponseString;

        // Generic
        public double ProtocolVersion;
        public HTTPRequestType RequestType;
        public Dictionary<string, string> RequestHeaders;
        public string RequestData;
    }
}

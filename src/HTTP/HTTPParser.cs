using System;
using System.Collections.Generic;
using System.Text;

namespace libicecast.HTTP
{
    /// <summary>
    /// Parser tool for HTTP & HTTP-like requests
    /// </summary>
    public static class HTTPParser
    {
        /// <summary>
        /// Gets the HTTPRequest representation of this string
        /// </summary>
        public static HTTPRequest GetAsHTTPRequest(this string str, HTTPRequestType requestType) 
        {
            try
            {
                if (str.Length < 1) return null;
                if (!str.Contains("\n") || !str.Contains(((char)0x20).ToString())) return null;
                HTTPRequest httpRequest = new HTTPRequest();

                string[] requestSplitted = str.Split('\n');
                string[] requestHead = requestSplitted[0].Split((char)0x20);

                switch (requestType)
                {
                    case HTTPRequestType.TYPE_CLIENT:
                        httpRequest.RequestType = HTTPRequestType.TYPE_CLIENT;
                        httpRequest.AccessMethod = requestHead[0];
                        httpRequest.AccessPath = requestHead[1];
                        httpRequest.ProtocolVersion = double.Parse(requestHead[2].Replace("HTTP/", string.Empty));

                        if (requestSplitted.Length > 3)
                        {
                            Array.Reverse(requestSplitted);
                            Array.Resize(ref requestSplitted, requestSplitted.Length - 1);
                            Array.Reverse(requestSplitted);
                            httpRequest.RequestData = requestSplitted[requestSplitted.Length - 2];
                            Array.Resize(ref requestSplitted, requestSplitted.Length - 3);

                            httpRequest.RequestHeaders = new Dictionary<string, string>();
                            foreach (string requestHeader in requestSplitted)
                            {
                                string[] requestHeaderSplitted = requestHeader.Split(new[] { ':' }, 2);
                                httpRequest.RequestHeaders.Add(requestHeaderSplitted[0], requestHeaderSplitted[1].TrimStart());
                            }
                        }

                        return httpRequest;
                    case HTTPRequestType.TYPE_SERVER:
                        httpRequest.RequestType = HTTPRequestType.TYPE_SERVER;
                        httpRequest.ProtocolVersion = double.Parse(requestHead[0].Replace("HTTP/", string.Empty));
                        httpRequest.ResponseCode = int.Parse(requestHead[1]);
                        
                        for (int i = 2; i < requestHead.Length; i++) 
                        {
                            httpRequest.ResponseString += $"{requestHead[i]} ";
                        }
                        httpRequest.ResponseString = httpRequest.ResponseString.TrimEnd();

                        if (requestSplitted.Length > 3) 
                        {
                            Array.Reverse(requestSplitted);
                            Array.Resize(ref requestSplitted, requestSplitted.Length - 1);
                            Array.Reverse(requestSplitted);
                            httpRequest.RequestData = requestSplitted[requestSplitted.Length - 2];
                            Array.Resize(ref requestSplitted, requestSplitted.Length - 3);

                            httpRequest.RequestHeaders = new Dictionary<string, string>();
                            foreach (string requestHeader in requestSplitted)
                            {
                                string[] requestHeaderSplitted = requestHeader.Split(new[] { ':' }, 2);
                                httpRequest.RequestHeaders.Add(requestHeaderSplitted[0], requestHeaderSplitted[1].TrimStart());
                            }
                        }

                        return httpRequest;
                    default:
                        return null;
                }
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the string representation of this HTTPRequest
        /// </summary>
        public static string GetAsString(this HTTPRequest request) 
        {
            try 
            {
                string finalStr = null;

                switch (request.RequestType)
                {
                    case HTTPRequestType.TYPE_CLIENT:
                        finalStr = $"{request.AccessMethod} {request.AccessPath} HTTP/{request.ProtocolVersion:0.0}\n";

                        foreach (KeyValuePair<string, string> requestHeader in request.RequestHeaders) 
                        {
                            finalStr += $"{requestHeader.Key}: {requestHeader.Value}\n";
                        }

                        finalStr += $"\n{request.RequestData}\n\r";

                        return finalStr;
                    case HTTPRequestType.TYPE_SERVER:
                        finalStr = $"HTTP/{request.ProtocolVersion:0.0} {request.ResponseCode} {request.ResponseString}\n";

                        foreach (KeyValuePair<string, string> requestHeader in request.RequestHeaders)
                        {
                            finalStr += $"{requestHeader.Key}: {requestHeader.Value}\n";
                        }

                        finalStr += $"\n{request.RequestData}\n\r";

                        return finalStr;
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
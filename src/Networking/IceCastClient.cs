using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using libicecast.HTTP;

/**
 * IceCastClient
 * Written by vlOd
 */

namespace libicecast.Networking
{
    /// <summary>
    /// A client implementing the IceCast protocol
    /// </summary>
    public class IceCastClient
    {
        /// <summary>
        /// The TCP client that is being used
        /// </summary>
        public TCPClient TCPClient = null;
        /// <summary>
        /// The current stream the client is streaming
        /// </summary>
        public IceCastStream CurrentStream = null;
        private string serverIP = null;
        private int serverPort = -1;
        private string loginUser = null;
        private string loginPass = null;
        private string loginErrorStr = null;
        private bool isLoggingIn = false;
        private bool isLoggedIn = false;

        /// <summary>
        /// A client implementing the IceCast protocol
        /// </summary>
        public IceCastClient(string serverIP, int serverPort,
            string loginUser, string loginPass)
        {
            this.serverIP = serverIP;
            this.serverPort = serverPort;
            this.loginUser = loginUser;
            this.loginPass = loginPass;
        }


        /// <summary>
        /// Connects to the server and sets up stream information for login
        /// </summary>
        public bool Connect(string contentType, bool isPublic, bool isPrivate,
            string streamName, string streamDescription,
            string streamGenre, string streamURL)
        {
            TCPClient = new TCPClient(serverIP, serverPort);
            CurrentStream = new IceCastStream(contentType, isPublic, isPrivate,
                streamName, streamDescription, streamGenre, streamURL);

            bool connectionResult = TCPClient.Connect();

            if (connectionResult)
            {
                isLoggingIn = true;
                return true;
            }
            else
            {
                TCPClient = null;
                CurrentStream = null;
                isLoggingIn = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the server and clears up the stream
        /// </summary>
        public void Disconnect() 
        {
            TCPClient.Disconnect();
            CurrentStream = null;
            TCPClient = null;
            isLoggingIn = false;
            isLoggedIn = false;
        }

        /// <summary>
        /// Sends audio data to the server
        /// </summary>
        public bool SendAudioData(byte[] audioData)
        {
            if (!isLoggedIn)
            {
                CancelLoginWithErr("Attempted to send data, but not logged in.");
                throw new Exception(loginErrorStr);
            }

            //TCPClient.Send(Encoding.ASCII.GetBytes(string.Empty));
            TCPClient.Send(audioData);

            return true;
        }

        /// <summary>
        /// Cancels the login process and displays an error
        /// </summary>
        private void CancelLoginWithErr(string err) 
        {
            loginErrorStr = err;
            isLoggingIn = false;
            isLoggedIn = false;
            if (TCPClient != null) TCPClient.Disconnect();
        }

        /// <summary>
        /// Event for TCPClient
        /// </summary>
        private void TCPClient_DataReceived(object sender, DataReceivedEventArgs e)
        {
            HTTPRequest decodedResponse = Encoding.UTF8.GetString(e.ReceivedData)
                .GetAsHTTPRequest(HTTPRequestType.TYPE_SERVER);

            if (isLoggingIn)
            {
                if (decodedResponse.ResponseCode.Equals(401))
                {
                    // Failed login
                    if (loginUser != null && loginPass != null)
                    {
                        CancelLoginWithErr("The server requested login credentials " +
                            "but the specified credentials were invalid.");
                    }
                    else
                    {
                        CancelLoginWithErr("The server requested login credentials " +
                            "but no credentials were specified.");
                    }
                }
                else if (decodedResponse.ResponseCode.Equals(403))
                {
                    if (decodedResponse.RequestData.Contains("too many sources connected"))
                        CancelLoginWithErr("The server has denied the login " +
                            "because there are too many sources connected.");
                    else if (decodedResponse.RequestData.Contains("Mountpoint in use"))
                        CancelLoginWithErr("The server has denied the login " +
                            "because the specified mountpoint is already used.");
                    else
                        CancelLoginWithErr("The server has denied the login " +
                            "without specifing a valid reason.");
                }
                else if (decodedResponse.ResponseCode.Equals(500))
                {
                    CancelLoginWithErr("An internal server error has occured.");
                }
                else if (decodedResponse.ResponseCode.Equals(200))
                {
                    // Successfull login
                    loginErrorStr = null;
                    isLoggingIn = false;
                    isLoggedIn = true;
                }
                else
                {
                    // Invalid response
                    CancelLoginWithErr($"Expected response code \"200\", \"401\", \"403\" or \"500\" " +
                        $"but got \"{decodedResponse.ResponseCode}\".");
                    return;
                }
            }
            else 
            {
                CancelLoginWithErr("The server has sent a response, but not logging in.");
            }
        }

        /// <summary>
        /// Logs in and starts streaming
        /// </summary>

        public bool Login()
        {
            if (!isLoggingIn)
            {
                CancelLoginWithErr("Attempted to login, but not connected.");
                throw new Exception(loginErrorStr);
            }

            TCPClient.DataReceived += TCPClient_DataReceived;

            if (loginUser != null && loginPass != null)
            {
                // Login with credentials
                string loginString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{loginUser}:{loginPass}"));
                TCPClient.Send(Encoding.UTF8.GetBytes(
                    (
                        $"SOURCE /stream HTTP/1.0\n" +
                        $"Authorization: Basic {loginString}\n" +
                        $"Host: {serverIP}:{serverPort}\n" +
                        $"User-Agent: libicecast\n" +
                        $"Content-Type: {CurrentStream.ContentType}\n" +
                        $"ice-public: {(CurrentStream.IsPublic ? "1" : "0")}\n" +
                        $"ice-private: {(CurrentStream.IsPrivate ? "1" : "0")}\n" +
                        $"ice-name: {CurrentStream.Name}\n" +
                        $"ice-description: {CurrentStream.Description}\n" +
                        $"ice-genre: {CurrentStream.Genre}\n" +
                        $"ice-url: {CurrentStream.URL}\n" +
                        $"\n\r"
                    ).GetAsHTTPRequest(HTTPRequestType.TYPE_CLIENT).GetAsString()
                ));
            }
            else
            {
                // Login without credentials
                TCPClient.Send(Encoding.UTF8.GetBytes(
                    (
                        $"SOURCE /stream HTTP/1.0\n" +
                        $"Host: {serverIP}:{serverPort}\n" +
                        $"User-Agent: libicecast\n" +
                        $"Content-Type: {CurrentStream.ContentType}\n" +
                        $"ice-public: {(CurrentStream.IsPublic ? "1" : "0")}\n" +
                        $"ice-private: {(CurrentStream.IsPrivate ? "1" : "0")}\n" +
                        $"ice-name: {CurrentStream.Name}\n" +
                        $"ice-description: {CurrentStream.Description}\n" +
                        $"ice-genre: {CurrentStream.Genre}\n" +
                        $"ice-url: {CurrentStream.URL}\n" +
                        $"\n\r"
                    ).GetAsHTTPRequest(HTTPRequestType.TYPE_CLIENT).GetAsString()
                ));
            }

            int ticksPassed = 0;
            while (!isLoggedIn) 
            {
                if (ticksPassed >= 10000)
                    break;
                ticksPassed++;
                Thread.Sleep(1);
            }

            if (!isLoggedIn && loginErrorStr != null)
                throw new Exception(loginErrorStr);
            else if (!isLoggedIn)
                throw new Exception("An unspecified login error has occured.");
            else
                return true;
        }
    }
}
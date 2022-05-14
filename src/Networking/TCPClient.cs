using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/**
 * TCPClient
 * Written by vlOd
 */

namespace libicecast.Networking
{
    /// <summary>
    /// EventArgs for the DataReceived event in TCPClient
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        private byte[] receivedData = null;
        /// <summary>
        /// The data received from the server
        /// </summary>
        public byte[] ReceivedData { get => receivedData; }

        /// <summary>
        /// EventArgs for the DataReceived event in TCPClient
        /// </summary>
        public DataReceivedEventArgs(byte[] data) 
        {
            receivedData = data;
        }
    }

    /// <summary>
    /// A TCP client
    /// </summary>
    public class TCPClient
    {
        /// <summary>
        /// The IP the client is connecting to
        /// </summary>
        public readonly string Server = null;
        /// <summary>
        /// The port the client is connecting to
        /// </summary>
        public readonly int Port = -1;
        private bool isConnected = false;
        private TcpClient tcpClient = null;
        private NetworkStream networkStream = null;
        private Thread receiveDataThread = null;

        /// <summary>
        /// Event fired when the client has connected
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// Event fired when data is received from the server
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        /// <summary>
        /// Event fired when the client has disconnected
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// A TCP client
        /// </summary>
        public TCPClient(string server, int port) 
        {
            Server = server;
            Port = port;
        }

        /// <summary>
        /// Connects to the server
        /// </summary>
        public bool Connect() 
        {
            try
            {
                tcpClient = new TcpClient(Server, Port);
                receiveDataThread = new Thread(new ThreadStart(ReceiveDataThread_Function));
                networkStream = tcpClient.GetStream();

                isConnected = true;
                receiveDataThread.Start();
                if (Connected != null) Connected.Invoke(this, EventArgs.Empty);
				
                return true;
            }
            catch
            {
                Disconnect();
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the server
        /// </summary>
        public void Disconnect() 
        {
            if (tcpClient != null) tcpClient.Close();
            if (networkStream != null) networkStream.Close();

            isConnected = false;
            tcpClient = null;
            networkStream = null;
            receiveDataThread = null;

            if (Disconnected != null) Disconnected.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sends data to the server
        /// </summary>
        public void Send(byte[] data) 
        {
            if (!isConnected || tcpClient == null) return;
            networkStream.Write(data, 0, data.Length);
        }

        private void DataRecv(byte[] data) 
        {
            if (data.Length < 1) return;
            if (DataReceived != null) DataReceived.Invoke(this, new DataReceivedEventArgs(data));
        }

        private void ReceiveDataThread_Function() 
        {
            while (isConnected) 
            {
                try
                {
                    if (tcpClient.Available != 0)
                    {
                        byte[] data = new byte[tcpClient.Available];
                        networkStream.Read(data, 0, data.Length);
                        DataRecv(data);
                    }
                    else
                    {
                        if (tcpClient.Client.Poll(1, SelectMode.SelectRead))
                        {
                            byte[] recvByte = new byte[1];
                            if (tcpClient.Client.Receive(recvByte, 0, 1, SocketFlags.Peek) == 0)
                                throw new Exception("DISCONNECTED_BY_SERVER");
                            if (recvByte[0] == 0x00)
                                throw new Exception("DISCONNECTED_BY_SERVER");
                        }
                    }
                }
                catch
                {
                    Disconnect();
                }
            }
        }
    }
}
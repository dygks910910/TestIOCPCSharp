using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace TestIOCP
{
    partial class AsynchronousSocketListener
    {
        public class StateObject
        {
            // Client  socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 1024;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
            // Received data string.  
            public StringBuilder sb = new StringBuilder();
        }
        public static List<StateObject> clientList = new List<StateObject>();
        public List<TcpClient> serverList = new List<TcpClient>();
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);


        public AsynchronousSocketListener()
        {
        }
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        public void ConnectToAGVServer()
        {
            Int32 port = 9999;


            serverList.Add(new TcpClient(Dns.GetHostName(), port));
            TcpClient connectToAGVServer = new TcpClient(Dns.GetHostName(), port);
            Send(connectToAGVServer.Client, "a");


            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = connectToAGVServer.Client;
            connectToAGVServer.Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

            String responseData = String.Empty;
            responseData = System.Text.Encoding.ASCII.GetString(state.buffer, 0, StateObject.BufferSize);
        }
        public void CheckThreadPoolReceivedData()
        {
            int receivedData = 0;
            while(true)
            {
                for(int i = 0 ; i < clientList.Count; ++i)
                {
                    //바이트수를 받아옴.
                    receivedData = clientList[i].workSocket.Available;
                    if(receivedData > 0)
                    {
                        Console.WriteLine("데이터를 받음");
                    }
                }

            }

        }
    }
}

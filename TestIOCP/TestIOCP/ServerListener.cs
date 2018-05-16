using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
namespace IOCPServer
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

        #region socket정보저장배열변수
        public static List<StateObject> clientList = new List<StateObject>();
        public List<TcpClient> serverList = new List<TcpClient>();
        #endregion

        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Console.WriteLine(Dns.GetHostName());
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine(addr);
                }
            }
            Console.WriteLine(localEndPoint.Port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            Task.Factory.StartNew(new Action(CheckThreadPoolReceivedData));
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }
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
        //항상 돌아가는 스레드(recv할 데이터가 있는지 항시체크.)
        public void CheckThreadPoolReceivedData()
        {
            int availableByte = 0;
            StateObject tmpState;
            while(true)
            {
                for(int i = 0 ; i < clientList.Count; ++i)
                {
                    //바이트수를 받아옴.
                    availableByte = clientList[i].workSocket.Available;
                    if (availableByte > 0)
                    {
                        tmpState = new StateObject();
                        tmpState.workSocket = clientList[i].workSocket;
                        Console.WriteLine("데이터를 받음");
                        clientList[i].workSocket.BeginReceive(clientList[i].buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), clientList[i]);
                    }
                }

            }

        }
    }
}

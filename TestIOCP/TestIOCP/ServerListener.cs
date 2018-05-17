﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
namespace IOCPServer
{
    partial class Server
    {
        private const int LISTEN_PORT = 11000;
        private const int AGV_SERVER_PORT = 9999;
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
        static ManualResetEvent manualEvent = new ManualResetEvent(false);

        #region (공유리소스)
        private static List<StateObject> clientList = new List<StateObject>();
        private List<TcpClient> serverList = new List<TcpClient>();
        System.Timers.Timer mTimer = new System.Timers.Timer();
        #endregion

        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  

            IPHostEntry ipHostInfo = YH_Util.GetLoopbackIPHostEntry();
            IPAddress ipAddress = YH_Util.GetLoopbackIPAddress();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, LISTEN_PORT);

            Console.WriteLine(YH_Util.GetLoopbackHostName());
            Console.WriteLine(YH_Util.GetLoopbackIPAddress());
            Console.WriteLine(LISTEN_PORT);


            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            YH_Util.WorkingInThreadPool(CheckThreadPoolReceivedData);
            StartTimer(HeartBeat, 1);
            //YH_Util.WorkingInThreadPool(ConnectToOtherServer);
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
        public Server()
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
        //public void ConnectToAGVServer()
        //{
        //    Int32 port = 9999;


        //    serverList.Add(new TcpClient(Dns.GetHostName(), port));
        //    TcpClient connectToAGVServer = new TcpClient(Dns.GetHostName(), port);
        //    Send(connectToAGVServer.Client, "a");


        //    // Create the state object.  
        //    StateObject state = new StateObject();
        //    state.workSocket = connectToAGVServer.Client;
        //    connectToAGVServer.Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        //        new AsyncCallback(ReadCallback), state);

        //    String responseData = String.Empty;
        //    responseData = System.Text.Encoding.ASCII.GetString(state.buffer, 0, StateObject.BufferSize);
        //}

        #region 스레드함수.
        //Read only
        //항상 돌아가는 스레드(recv할 데이터가 있는지 항시체크.)
        //clientList와 ServerList공유자원 read/write
        public void CheckThreadPoolReceivedData()
        {
            int availableByte = 0;
            StateObject tmpState;
            Socket tmpSocket;
            while(true)
            {
                if (clientList != null)
                for(int i = 0 ; i < clientList.Count; ++i)
                {
                    tmpSocket = clientList[i].workSocket;
                    //바이트수를 받아옴.
                    if (tmpSocket.Connected)
                    {
                        availableByte = tmpSocket.Available;
                        if (availableByte > 0)
                        {
                            tmpState = new StateObject();
                            tmpState.workSocket = tmpSocket;
                            //Console.WriteLine("데이터를 받음");
                            if (tmpState.workSocket.Connected)
                            {
                                clientList[i].workSocket.BeginReceive(clientList[i].buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), clientList[i]);
                            }
                        }
                    }
                    else
                    {
                        //int tmp = 0;
                        //tmp = clientList.FindIndex(x => x.workSocket.Equals(tmpSocket));
                        //clientList.RemoveAt(tmp);
                    }
                }
                //for (int i = 0; i < serverList.Count; ++i)
                //{
                //    availableByte = serverList[i].Client.Available;
                //    if (availableByte > 0)
                //    {
                //        tmpState = new StateObject();
                //        tmpState.workSocket = serverList[i].Client;
                //        //Console.WriteLine("데이터를 받음");
                //        serverList[i].Client.BeginReceive(tmpState.buffer, 0, StateObject.BufferSize, 0,
                //new AsyncCallback(ReadCallback), tmpState);
                //    }
                //}

            }

        }

        //write,공유자원 접근(serverList).
        public void ConnectToAGVServer()
        {
            serverList.Add(new TcpClient(YH_Util.GetLoopbackHostName(), 9999));
        }
        public void HeartBeat(object sender, ElapsedEventArgs e)
        {
            manualEvent.Reset();
            Console.WriteLine("하트비트");
            for (int i = 0; i < serverList.Count; ++i)
           {
               if(!YH_Util.SocketConnected(serverList[i].Client))
               {
                   //연결안되있다면?
                   serverList.RemoveAt(i);

               }
           }
            for (int i = 0; i < clientList.Count; ++i)
            {
                if (!YH_Util.SocketConnected(clientList[i].workSocket))
                {
                    clientList.RemoveAt(i);
                    Console.WriteLine("끊킴");
                    //연결안되있다면?
                }
                
            }
            manualEvent.Set();

        }
        #endregion
        private void StartTimer(Action<object, ElapsedEventArgs> doSomething,double sec)
        {
            mTimer.Interval = sec * 1000; //  *1000 밀리초 = 1초
            mTimer.Elapsed += new ElapsedEventHandler(doSomething);
            mTimer.Start();
        }
    }
}

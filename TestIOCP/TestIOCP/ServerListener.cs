﻿using ConsoleClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IOCPServer
{

    class Server
    {
        struct ClientInfo
        {
            public Socket socket;   //Socket of the client
            public string strName;  //Name by which the user logged into the chat room
        }
        byte[] byteData = new byte[1024];
        List<ClientInfo> clientList = new List<ClientInfo>();
        Socket listenSocket;
        //string txtLog;
        private const int LISTEN_PORT = 11000;
        private const int AGV_SERVER_PORT = 9999;
       
        static ManualResetEvent acceptDone = new ManualResetEvent(false);

        public void StartListening()
        {
            try
            {
                listenSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);

                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any,LISTEN_PORT);

                listenSocket.Bind(ipEndPoint);
                listenSocket.Listen(4);
                while(true)
                {
                    acceptDone.Reset();
                    listenSocket.BeginAccept(new AsyncCallback(OnAccept), null);
                    acceptDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                YH_Util.YH_Exception_Form(ex);
            }
        }
        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                acceptDone.Set();
                Socket clientSocket = listenSocket.EndAccept(ar);

                listenSocket.BeginAccept(new AsyncCallback(OnAccept), null);

                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(OnReceive), clientSocket);
            }
            catch (Exception ex)
            {
                YH_Util.YH_Exception_Form(ex);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;
                clientSocket.EndReceive(ar);

                Data msgReceived = new Data(byteData);

                Data msgToSend = new Data();

                byte[] message;

                msgToSend.cmdCommand = msgReceived.cmdCommand;
                msgToSend.strName = msgReceived.strName;

                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:


                        ClientInfo clientInfo = new ClientInfo();
                        clientInfo.socket = clientSocket;
                        clientInfo.strName = msgReceived.strName;

                        clientList.Add(clientInfo);

                        msgToSend.strMessage = "<<<" + msgReceived.strName + " has joined the room>>>";
                        Console.WriteLine(msgToSend.strMessage);
                        break;

                    case Command.Logout:
                        int nIndex = 0;
                        foreach (ClientInfo client in clientList)
                        {
                            if (client.socket == clientSocket)
                            {
                                clientList.RemoveAt(nIndex);
                                break;
                            }
                            ++nIndex;
                        }

                        clientSocket.Close();

                        msgToSend.strMessage = "<<<" + msgReceived.strName + " has left the room>>>";
                        Console.WriteLine(msgToSend.strMessage);
                        break;

                    case Command.Message:

                        msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                        Console.WriteLine(msgToSend.strMessage);
                        break;

                    case Command.List:

                        msgToSend.cmdCommand = Command.List;
                        msgToSend.strName = null;
                        msgToSend.strMessage = null;

                        foreach (ClientInfo client in clientList)
                        {
                            msgToSend.strMessage += client.strName + "*";
                        }

                        message = msgToSend.ToByte();

                        clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None,
                                new AsyncCallback(OnSend), clientSocket);
                        break;
                }

                //브로드캐스트로 전체 Client에게 뿌려주기.
                {
                    message = msgToSend.ToByte();

                    foreach (ClientInfo clientInfo in clientList)
                    {
                        if (clientInfo.socket != clientSocket ||
                            msgToSend.cmdCommand != Command.Login)
                        {
                            if (msgToSend.cmdCommand != Command.List)  
                            clientInfo.socket.BeginSend(message, 0, message.Length, SocketFlags.None,
                                new AsyncCallback(OnSend), clientInfo.socket);
                        }
                    }
                   // txtLog += msgToSend.strMessage + "\r\n";
                }

                if (msgReceived.cmdCommand != Command.Logout)
                {
                    clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                }
            }
            catch (Exception ex)
            {
                YH_Util.YH_Exception_Form(ex);
                for (int i = 0; i < clientList.Count; ++i)
                {
                    if (!YH_Util.SocketConnected(clientList[i].socket))
                    {
                        clientList.Remove(clientList[i]);
                        break;
                    }
                }
                
            }
        }

        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                YH_Util.YH_Exception_Form(ex);
            }
        }
        public Server()
        {

        }
    }
    
          
}

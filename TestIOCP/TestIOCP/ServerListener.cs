using System;
using System.Collections.Generic;
using System.Collections;
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
    enum Command
    {
        Login,     
        Logout,     
        Message,    
        List,      
        Null       
    }
    class Server
    {
        struct ClientInfo
        {
            public Socket socket;   //Socket of the client
            public string strName;  //Name by which the user logged into the chat room
        }
        byte[] byteData = new byte[1024];
        ArrayList clientList = new ArrayList();
        Socket listenSocket;
        string txtLog;
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

                IPEndPoint ipEndPoint = new IPEndPoint(YH_Util.GetLoopbackIPAddress(),LISTEN_PORT);

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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
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
                    txtLog += msgToSend.strMessage + "\r\n";
                }

                if (msgReceived.cmdCommand != Command.Logout)
                {
                    clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
            }
        }
        public Server()
        {

        }
    }
    
          
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace ConsoleClient
{

    enum Command
    {
        Login,
        Logout,
        Message,
        List,
        Null
    }

    class Client
    {
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 11000;
        private Socket clientSocket; //The main client socket
        private string strName = "client";      //Name by which the user logs into the room

        private byte[] byteData = new byte[1024];

        private void SendStr()
        {
            try
            {
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.strMessage = Console.ReadLine();
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                //Send it to the server
                clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

            }
            catch (Exception)
            {
                Console.WriteLine("서버에 메시지를 보낼수 없습니다.");
            }
        }
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);

                Data msgReceived = new Data(byteData);
                //Accordingly process the message received
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        break;

                    case Command.Logout:
                        break;

                    case Command.Message:
                        break;

                    case Command.List:
                        Console.WriteLine("<<<" + strName + " has joined the room>>>\r\n");
                        break;
                }

                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
                {
                    //txtChatBox.Text += msgReceived.strMessage + "\r\n";
                    Console.WriteLine(msgReceived.strMessage + "\r\n");
                }

                byteData = new byte[1024];

                clientSocket.BeginReceive(byteData,
                                          0,
                                          byteData.Length,
                                          SocketFlags.None,
                                          new AsyncCallback(OnReceive),
                                          null);

            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CloseClient()
        {
            try
            {
                //Send a message to logout of the server
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Logout;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte();
                clientSocket.Send(b, 0, b.Length, SocketFlags.None);
                clientSocket.Close();
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void Connect()
        {
            try
            {
                if(clientSocket != null)
                {
                    clientSocket.Close();
                    clientSocket = null;
                }
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ipAddress = IPAddress.Parse(SERVER_IP);
                //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, SERVER_PORT);

                //Connect to the server
                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);

                //We are connected so we login into the server
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Login;
                msgToSend.strName = "클라이언트";
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte();

                //Send the message to the server
                clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Run()
        {
            Connect();
            while(true)
            {
                try
                {
                    SendStr();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Connect();
                }
            }
           
        }
    }
}

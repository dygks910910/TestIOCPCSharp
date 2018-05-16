using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace TestIOCPClient
{
    class clientClass
    {
        private NetworkStream stream = null;
        public TcpClient client{get;private set;}
        private const int PORT_NO = 11000;
        private const string HOST = "DESKTOP-F8O7NVP";
        public void Send(string msg)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);

            stream.Write(data, 0, data.Length);

            Console.WriteLine("Sent: {0}", msg);
        }
        public void Receive()
        {
            Byte[] data = new byte[256];
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received: {0}", responseData);
        }
        public void CloseSocketAndStream()
        {
            stream.Close();
            client.Close();
        }
        public void Connect1()
        {
            try
            {
                if(client != null)
                {
                    client.Close();
                }
                if(stream != null)
                {
                    stream.Close();
                }
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                client = new TcpClient(HOST, PORT_NO);
                stream = client.GetStream();
                Console.WriteLine("연결됨");
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}

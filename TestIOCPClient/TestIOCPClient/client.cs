using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
namespace TestIOCPClient
{


    class Program
    {
        public List<TcpClient> serverSocket = new List<TcpClient>();

        public void Connect1(String server, String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 11000;
                for (int i = 0; i < 1; ++i)
                {
                    //TcpClient client = new TcpClient(server, port);
                    serverSocket.Add(new TcpClient(server, port));
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            //Console.Read();
        }
        public String Receive(TcpClient client)
        {
            // Receive the TcpServer.response.

            // Buffer to store the response bytes.
            NetworkStream stream = client.GetStream();

            Byte[] data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);

            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received: {0}", responseData);
            stream.Close();
            return responseData;

        }
        public void Send(String msg, TcpClient client)
        {
            // Translate the passed message into ASCII and store it as a Byte array.
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer. 
            stream.Write(data, 0, data.Length);
            //stream.Close();

            Console.WriteLine("Sent: {0}", msg);

        }

        public void Destroy()
        {
            for (int i = 0; i < serverSocket.Count; ++i)
            {
                // Close everything.
                serverSocket[i].Close();
            }
        }


        static void Main(string[] args)
        {
            //Connect1("DESKTOP-F8O7NVP", "helloServer");
            Program program = new Program();
            program.Connect1(Dns.GetHostName(), "helloServer");
            string strLine;
            //             while (Console.ReadKey().Key == ConsoleKey.Enter)
            //             {
            while (true)
            {
                strLine = Console.ReadLine();
                program.Send(strLine, program.serverSocket[0]);
            }
            /*}*/

            //for (int i = 0; i < program.serverSocket.Count(); ++i)
            //{
            //    program.serverSocket[i].Close();
            //}


        }
    }
}

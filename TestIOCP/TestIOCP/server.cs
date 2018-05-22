using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

// State object for reading client data asynchronously  
namespace IOCPServer
{
    public class MainClass
    {
        static int Main(String[] args)
        {
            Server server = new Server();
            server.StartListening();
            return 0;
        }
    }

}
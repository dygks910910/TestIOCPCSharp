using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
// State object for reading client data asynchronously  
namespace TestIOCP
{
    public class main
    {
        static int Main(String[] args)
        {
            AsynchronousSocketListener server = new AsynchronousSocketListener();

            //server.ConnectToAGVServer();
            server.StartListeningCallBack();

            return 0;
        }
    }

}
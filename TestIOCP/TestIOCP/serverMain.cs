using System;
using System.Threading.Tasks;
// State object for reading client data asynchronously  
namespace IOCPServer
{
    public class MainClass
    {
        static int Main(String[] args)
        {
            ConsoleClient.Client slave = new ConsoleClient.Client();
            Server server = new Server();
            Task.Factory.StartNew(new Action(slave.Run));
            server.StartListening();
            return 0;
        }
    }

}
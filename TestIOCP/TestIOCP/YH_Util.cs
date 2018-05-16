using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace IOCPServer
{
    class YH_Util
    {
        public static IPHostEntry GetLoopbackIPHostEntry()
        {
            return Dns.GetHostEntry(Dns.GetHostName());
        }
        public static IPAddress GetLoopbackIPAddress()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress returnVal = null;
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    returnVal = addr;
                }
            }
            return returnVal;
        }
        public static string GetLoopbackHostName()
        {
            return Dns.GetHostName();
        }
        public static void WorkingInThreadPool(Action func)
        {
            Task.Factory.StartNew(func);
        }
        public static bool SocketConnected(Socket s)
        {
            if (s == null)
                return false;
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
            {
                try
                {
                    int sentBytesCount = s.Send(new byte[1], 1, 0);
                    return sentBytesCount == 1;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}

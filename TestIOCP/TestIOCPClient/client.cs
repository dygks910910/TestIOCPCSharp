using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TestIOCPClient
{
    class Program
    {

        static void Main(string[] args)
        {
            clientClass client = new clientClass();

            string msg;
            while (true)
            {
                if (client.client != null && client.client.Connected)
                {
                    msg = Console.ReadLine();
                    msg += "<EOF>";
                    client.Send(msg);
                }
                else
                {
                    Console.WriteLine("접속끊킴");
                    client.Connect1();
                }


            }
        }
    }
}

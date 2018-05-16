using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
namespace IOCPServer
{
    partial class Server
    {

       #region callback함수들
        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;

            clientList.Add(state);


            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

//             String responseData = String.Empty;
//             responseData = System.Text.Encoding.ASCII.GetString(state.buffer, 0, StateObject.BufferSize);
//             Console.WriteLine(responseData);
            //Send(handler, responseData);
        }
        public static void ReceiveCallBack()
        {


        }
        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;

            Socket handler = state.workSocket;
            if (!YH_Util.SocketConnected(handler))
            {
                handler.Close();
                return;
            }
            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the   
                    // client. Display it on the console.  
//                     Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
//                         content.Length, content);
                    // Echo the data back to the client.  
                    //Send(handler, content);
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                       content.Length, content);
                    state.sb.Clear();

                    
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);

                }
                

            }
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
       #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class MVMServer
    {
        protected Socket ServerSocket;
        protected int MaxConnectCount;
        protected int Port = 32765;
        SocketAsyncEventArgsPool SAEAPool;
        protected SocketAsyncEventArgs ServerAcceptArgs = new SocketAsyncEventArgs();
        public MVMServer()
        {
            MaxConnectCount = 100;
            SAEAPool = new SocketAsyncEventArgsPool(MaxConnectCount);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
            ServerSocket.Bind(endpoint);
            ServerSocket.Listen(MaxConnectCount);
            init();
        }
        private void init()
        {
            
            ServerAcceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>((sender, args) => {
                lock (SAEAPool)
                {
                    var client = new SocketAsyncEventArgs();
                    client.AcceptSocket = args.AcceptSocket;
                    SAEAPool.Push(client);
                }
                ProcessAccept(args);
                args.AcceptSocket = null;
                ServerSocket.AcceptAsync(args);
                //ServerSocket.Send(Encoding.UTF8.GetBytes("welcome"));
            });
            ServerSocket.AcceptAsync(ServerAcceptArgs);
            Console.WriteLine("Server is listening, waiting for client connect...");
        }

        private void ProcessAccept(SocketAsyncEventArgs args)
        {
            Console.WriteLine("A new client is connected now.");
            ProcessReceive(args);
        }

        private void ProcessReceive(SocketAsyncEventArgs args)
        {
            var readsesa = new SocketAsyncEventArgs();
            var bytes = new byte[2];
            readsesa.AcceptSocket = args.AcceptSocket;
            readsesa.SetBuffer(bytes, 0, 2);
            readsesa.Completed += new EventHandler<SocketAsyncEventArgs>((sender, args) => {
                Console.WriteLine("Receiving");
                Console.WriteLine(Encoding.UTF8.GetString(args.Buffer));
                Console.WriteLine(args.AcceptSocket.ReceiveAsync(args));
                //args.SetBuffer(args.Offset, args.BytesTransferred);
                Console.WriteLine(Encoding.UTF8.GetString(args.Buffer));
                Console.WriteLine(args.AcceptSocket.ReceiveAsync(args));
            });
             Console.WriteLine(args.AcceptSocket.ReceiveAsync(readsesa));
        }
    }
}

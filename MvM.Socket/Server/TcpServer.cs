using MvM.Socket.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SysSocket = System.Net.Sockets;
using System.Linq;

namespace MvM.Socket.Server
{
    public sealed class TcpServer
    {
        internal SysSocket.Socket ListenSocket;
        internal SocketAsyncEventArgs AcceptEventArgs;
        public int MaxConnectCount { private set; get; }
        public int BufferSize { private set; get; }
        private BufferManager bufferManager;
        private int opsToPreAlloc = 2;  // read, write (don't alloc buffer space for accepts)
        internal SocketAsyncEventArgsPool argsPool;
        internal ClientCollection ClientPool;
        public TcpServer(int maxConnectCount, int bufferSize = 2)
        {
            Console.WriteLine("Welcome to MvMApplicaions");
            Console.WriteLine("Starting TCP Service...");
            ClientPool = new ClientCollection();
            if (maxConnectCount < 5) throw new InvalidOperationException("maxConnectCount must larger than 5");
            MaxConnectCount = maxConnectCount;
            BufferSize = bufferSize;
            argsPool = new SocketAsyncEventArgsPool(MaxConnectCount);
            AcceptEventArgs = new SocketAsyncEventArgs();
            AcceptEventArgs.Completed += Accept;
        }

        public void Start(IPEndPoint ipEndPoint)
        {
            Console.Write("Buffer initializing...");
            bufferManager = new BufferManager(MaxConnectCount * BufferSize * opsToPreAlloc, BufferSize);
            bufferManager.InitBuffer();
            Console.WriteLine("Complete!");
            Console.Write("ConnectionPool initializing...");
            for (int i = 0; i < MaxConnectCount; i++)
            {
                var args = new SocketAsyncEventArgs();
                bufferManager.SetBuffer(args);
                args.Completed += Receive;
                argsPool.Push(args);
            }
            Console.WriteLine("Complete!");
            ListenSocket = new SysSocket.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ListenSocket.Bind(ipEndPoint);

            ListenSocket.Listen(MaxConnectCount);
            ListenSocket.AcceptAsync(AcceptEventArgs);
            Console.WriteLine($"TcpService now listening [{ipEndPoint.Address.ToString()}:{ipEndPoint.Port}]");
            Console.WriteLine("TCP Service is runing");
        }

        private void Accept(object sender, SocketAsyncEventArgs e)
        {
            var ip = e.AcceptSocket.RemoteEndPoint;
            var ioArgs = argsPool.Pop();
            ioArgs.AcceptSocket = e.AcceptSocket;
             var userData = new UserData()
            {
                ConnectTime = DateTime.Now,
                RemoteEndPoint = ip,
                Socket = e.AcceptSocket,
                ReceiveArgs = ioArgs
            };
            e.AcceptSocket.ReceiveAsync(ioArgs);
            Console.WriteLine($"[{ip.ToString()} {userData.UserId.ToString()}] is connected");
            ioArgs.UserToken = userData;
            ClientPool[userData.UserId] = userData;
            AcceptEventArgs.AcceptSocket = null;
            ListenSocket.AcceptAsync(AcceptEventArgs);
        }

        private void PackageReceive(UserData userData, byte[] data)
        {
            var receiveData = Encoding.UTF8.GetString(data);
            Console.WriteLine($"[{userData.Socket.RemoteEndPoint.ToString()}] SendMsg:{receiveData}");
            //Send(userData.UserId, Encoding.UTF8.GetBytes($"Server received : {receiveData}"));
        }
        public void Send(object userid, byte[] data)
        {
            var client = ClientPool[userid];
            if (ClientPool[userid] == null) throw new InvalidOperationException($"{userid.ToString()} client has not been connected");
            client.Socket.SendAsync(data, SocketFlags.None);
        }

        private void Receive(object sender, SocketAsyncEventArgs e)
        {
            UserData currentUser = (UserData)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                do
                {
                    currentUser.DataBuffer.AddRange(e.Buffer.Skip(e.Offset).Take(e.BytesTransferred));
                }
                while (!e.AcceptSocket.ReceiveAsync(e));
                var data = new byte[currentUser.DataBuffer.Count];
                Array.Copy(currentUser.DataBuffer.ToArray(), 0, data, 0, currentUser.DataBuffer.Count);
                currentUser.DataBuffer.Clear();
                PackageReceive(currentUser, data);
            }
            else if (e.SocketError == SocketError.ConnectionReset)
            {
                ClientPool.Remove(currentUser.UserId);
                Console.WriteLine($"[{currentUser.UserId}] is offline");
            }
            else
            {
                Console.WriteLine(e.SocketError.ToString());
            }
        }
    }
}

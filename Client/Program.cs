using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 32765));
            var receiveBuffer = new SocketAsyncEventArgs();
            byte[] buffer = new byte[5];
            receiveBuffer.SetBuffer(buffer, 0, 5);
            List<byte> dataBytes = new List<byte>();
            receiveBuffer.Completed += new EventHandler<SocketAsyncEventArgs>((sender, args) =>
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    do
                    {
                        dataBytes.AddRange(args.Buffer.Skip(args.Offset).Take(args.BytesTransferred));
                    }
                    while (!sock.ReceiveAsync(args));
                    Console.WriteLine(Encoding.UTF8.GetString(dataBytes.ToArray()));
                    dataBytes.Clear();
                }
            });
            sock.ReceiveAsync(receiveBuffer);
            string msg = string.Empty;
            do
            {
                msg = Console.ReadLine();
                var bytes = Encoding.UTF8.GetBytes(msg);
                sock.SendBufferSize = bytes.Length;
                sock.Send(bytes);
            } while (msg != "exit");
            
              
        }
    }
}

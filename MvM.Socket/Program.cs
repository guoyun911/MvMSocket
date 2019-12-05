using MvM.Socket.Server;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace MvM.Socket
{
    class Program
    {
        static void Main(string[] args)
        {
            //AutoResetEvent autoReset = new AutoResetEvent(false);
            var server = new TcpServer(100);
            server.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 32765));
            string msg = string.Empty;
            do
            {
                msg = Console.ReadLine();
                if (msg.Contains(" "))
                {
                    var arr = msg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var userid = arr[0];
                    server.Send(userid, Encoding.UTF8.GetBytes(arr[1]));
                }
                else 
                {
                    foreach (var client in server.ClientPool)
                    {
                        server.Send(client.UserId, Encoding.UTF8.GetBytes(msg));
                    } 
                }
            } while (true);
            //autoReset.WaitOne();
        }
    }
}

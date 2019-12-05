using System;
using System.Threading;

namespace Server
{
    public class Program
    {
        static AutoResetEvent are = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            new MVMServer();
            are.WaitOne();
            
        }
    }
}

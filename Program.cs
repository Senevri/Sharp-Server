using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace EchoServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length>0 && args[0].Equals("send",StringComparison.Ordinal))
            {
                try
                {
                    IPEndPoint host = new IPEndPoint(IPAddress.Loopback, 8080);
                    Socket l_socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.IP);
                    l_socket.Connect(host);
                    String szData = Console.ReadLine();
                    Console.WriteLine("sending...");
                    //String szData = "Helloword";
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes(szData);
                    l_socket.Send(byData);
                    byte[] byBuffer = new byte[1024];
                    l_socket.Receive(byBuffer);
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(byBuffer));
                    l_socket.Close();
                }
                catch (SocketException se)
                {
                    System.Console.WriteLine(se.Message);
                }
            }
            else {
                try
                {
                    IPEndPoint host = new IPEndPoint(IPAddress.Any, 8080);
                    Socket sockListener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.IP);
                    sockListener.Bind(host);
                    sockListener.Listen(100);
                    while(true) {
                        Socket sockClient = sockListener.Accept();
            
                        Console.WriteLine("receiving...");
                        byte[] byBuffer = new byte[1024];
                        sockClient.Receive(byBuffer);
                        string output = System.Text.Encoding.ASCII.GetString(byBuffer);
                        sockClient.Send(byBuffer);
                        System.Console.WriteLine(output);
                        sockClient.Close();
                    }
                }
                catch (SocketException se)
                {
                    System.Console.WriteLine(se.Message);
                }
            }

        }
    }
}

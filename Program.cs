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
            if ((args.Length==1))
            {
                
                try
                {
                    IPEndPoint host = new IPEndPoint(IPAddress.Parse(args[0]), 8080);
                    Console.WriteLine("sending...");
                    String szData = "";
                    String dt = DateTime.Now.ToString();
                        
                    while (true && 0 != szData.CompareTo("/q"))
                    {
                        Socket l_socket = new Socket(AddressFamily.InterNetwork,
                            SocketType.Stream, ProtocolType.IP);                  
                        l_socket.Connect(host);             
                        dt = dt + ":::" + szData;
                        //Console.WriteLine(dt.);
                        byte[] byData = System.Text.Encoding.ASCII.GetBytes(dt);                        
                        l_socket.Send(byData);
                        bool reading = true;
                        while (reading)
                        {
                            byte[] byBuffer = new byte[160];                      
                            l_socket.Receive(byBuffer);
                            string s1 = System.Text.Encoding.ASCII.GetString(byBuffer).TrimEnd('\0');
                            Console.WriteLine(s1);
                            string s2 = "###END";
                            if (s1.Length >= 6 && 0 == s1.Substring(s1.Length-6, 6).CompareTo(s2))
                            { 
                                reading = false;
                                dt = DateTime.Now.ToString();
                        
                            }
                            if (reading)
                            {
                                Console.WriteLine(s1);
                            }
                        }
                        Console.Write("$ ");
                        szData = Console.ReadLine();                                             
                        l_socket.Close();
                        //System.Threading.Thread.Sleep(500);
                    }
                   
                }
                catch (SocketException se)
                {
                    System.Console.WriteLine(se.Message);
                }
            }
            else {
                try
                {
                    Console.WriteLine("Server Up");
                    IPEndPoint host = new IPEndPoint(IPAddress.Any, 8080);
                    Socket sockListener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.IP);
                    sockListener.Bind(host);
                    sockListener.Listen(100);
                    List<Message> msgstack = new List<Message>();
                    while(true) {
                        Socket sockClient = sockListener.Accept();
                        Console.WriteLine("receiving...");
                        byte[] byBuffer = new byte[160];
                        sockClient.Receive(byBuffer);
                        string output = System.Text.Encoding.ASCII.GetString(byBuffer);                        
                        Console.WriteLine(output.TrimEnd(':', '\0'));
                        int loc = output.IndexOf(":::",0);

                        string body = output.Substring(loc+3);                   
                        string ts = output.Substring(0, loc);
                        Message msg = new Message(body.TrimEnd('\0'), System.DateTime.Now);
                        bool sentmsgs = false;                        
                        if (msgstack.Count > 0)
                        {
                            foreach (Message m in msgstack)
                            {
                                if(m.tstamp > DateTime.Parse(ts)) {
                                byBuffer = System.Text.Encoding.ASCII.GetBytes(m.body+'\n');
                                Console.WriteLine("sending: " + m.body);
                                sockClient.Send(byBuffer);
                                sentmsgs = true;
                                }
                            }
                            if (sentmsgs == false)
                            {
                                byBuffer = System.Text.Encoding.ASCII.GetBytes("no new messages");
                                sockClient.Send(byBuffer);
                            }                            
                        }
                        if (msg.body.Length > 0)
                        {
                            msgstack.Add(msg);
                        }
                        byte[] endmsg = new byte[6];
                        endmsg = System.Text.Encoding.ASCII.GetBytes("###END");
                        sockClient.Send(endmsg);                        
                        System.Console.WriteLine(System.Text.Encoding.ASCII.GetString(endmsg));
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

    class Message {
        public String body;
        public DateTime tstamp;

        public Message(string output, DateTime dateTime)
        {
            // TODO: Complete member initialization
            this.body = output;
            this.tstamp = dateTime;
        }

    }
}

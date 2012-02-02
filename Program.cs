using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
//using System.Text.Encoding.ASCII;

namespace EchoServerTest
{
	class Util
	{ 
		public static bool StringsMatch(string s1, string s2){
			return (s1.Length >= s2.Length && 0 == s1.Substring(s1.Length-6, 6).CompareTo(s2));
							
		}

		public static string BytesToString(byte[] bytes) {
			return Encoding.ASCII.GetString(bytes);
		}
	}

	class Program
	{
		enum Conf: int { BUFSIZE = 160, SERVER_PORT = 8080}

		static Socket ConnectToHost(IPEndPoint host) {
			Socket l_socket = new Socket(AddressFamily.InterNetwork,
								SocketType.Stream, ProtocolType.IP);
			l_socket.Connect(host);
			return l_socket;
		}

		static int SendString(Socket s, string message) {
			return s.Send(System.Text.Encoding.ASCII.GetBytes(message));
		
		}

		static string ReceiveFromSocket(Socket s) {
			byte[] byBuffer = new byte[(int)Conf.BUFSIZE];
			s.Receive(byBuffer);
			if (0 == byBuffer[0]) {
				return "###END";
			} 
			return Util.BytesToString(byBuffer);			
		}
		static void Server() {
			Console.WriteLine("Server Up");
			IPEndPoint host = new IPEndPoint(IPAddress.Any, (int)Conf.SERVER_PORT);
			Socket sockListener = new Socket(AddressFamily.InterNetwork,
					SocketType.Stream, ProtocolType.IP);
			int sentBytes;
			sockListener.Bind(host);
			sockListener.Listen(100);
			List<Message> msgstack = new List<Message>();
			bool active = true;
			while(active) {
				Socket sockClient =sockListener.Accept();
				/* todo: how to get some data from whom we're speaking with? */
				//Console.WriteLine(sockClient.RemoteEndPoint.);
				Console.WriteLine("receiving...");			
				string output = ReceiveFromSocket(sockClient);
				Console.WriteLine(output.TrimEnd(':', '\0'));
				int loc = output.IndexOf(":::",0);

				string body = output.Substring(loc+3).TrimEnd('\0');				   
				active = (0 != body.CompareTo("/killserver")); 
				/*received timestamp = last time client received from this server*/
				string ts = output.Substring(0, loc);
				bool sentmsgs = false;						
				if (msgstack.Count > 0)
				{
					foreach (Message m in msgstack)
					{
						if(m.tstamp > DateTime.Parse(ts)) {
							Console.WriteLine("sending: " + m.body);	
							sentBytes = SendString(sockClient, m.body+'\n');
							sentmsgs = true;
						}
					}
					if (sentmsgs == false)
					{
						sentBytes = SendString(sockClient, "no new messages");								
					}							
				}
				Message msg = new Message(body.TrimEnd('\0'), DateTime.Now);						
				if (msg.body.Length > 0)
				{
					msgstack.Add(msg);
				}
				byte[] endmsg = new byte[6];
				endmsg = Encoding.ASCII.GetBytes("###END");
				sockClient.Send(endmsg);						
				//Console.WriteLine(System.Text.Encoding.ASCII.GetString(endmsg));
				sockClient.Close();
				if (msgstack.Count > 0)
				{
					foreach (Message m in msgstack)
					{
						Console.WriteLine(m.tstamp + ":::" + m.body );
					}
				}
			}
			Console.WriteLine("Terminated server");
		}

		static void Client(string ipstring) {
			IPEndPoint host = null;
			try
			{
				host = new IPEndPoint(IPAddress.Parse(ipstring), (int)Conf.SERVER_PORT);
			}
			catch (FormatException e)
			{ 
				/* try evaluating it as a domain name */
				IPAddress[] addresses = Dns.GetHostAddresses(ipstring);
				foreach (IPAddress a in addresses){
					host = new IPEndPoint(a, (int)Conf.SERVER_PORT);
					Socket temp = ConnectToHost(host);
					if (temp.Connected) {
						temp.Close();
						break;
					}
				}
			}
			Console.WriteLine("sending...");
			String szData = "";
			String dt = DateTime.Now.ToString();

			while (0 != szData.CompareTo("/q"))
			{
				Socket l_socket = ConnectToHost(host);
				dt = dt + ":::" + szData;
				//Console.WriteLine(dt.);
				SendString(l_socket, dt);
				bool reading = true;
				while (reading)
				{
					string s1 = ReceiveFromSocket(l_socket).TrimEnd('\0').TrimEnd('\n');
					//Console.WriteLine(s1);
					string s2 = "###END";
					if (Util.StringsMatch(s1.Trim(), s2))
					{ 
						reading = false;
						dt = DateTime.Now.ToString();
						Console.WriteLine(s1.TrimEnd('\0').Substring(0,s1.Length-6));					   
					}
					if (reading)
					{
						Console.WriteLine(s1.TrimEnd('\0'));
					}
				}
				Console.Write("$ ");
				szData = Console.ReadLine();											 
				l_socket.Close();
				//System.Threading.Thread.Sleep(500);
			}
		}

		static void Main(string[] args)
		{
		if ((args.Length==1))
			{		   
				try
				{
					Client(args[0]);	   
				}
				catch (SocketException se)
				{
					Console.WriteLine(se.Message);
				}
			}
			else { /* Server */
				try
				{
					Server();
				}
				catch (SocketException se)
				{
					Console.WriteLine(se.Message);
				}
			}

		}
	}

	class Message {
		public String body;
		public DateTime tstamp;
		public String from; // not used

		public Message(string output, DateTime dateTime)
		{
			// TODO: Complete member initialization
			this.body = output;
			this.tstamp = dateTime;
		}

		public Message() 
		{
			this.body = "";
			this.tstamp = DateTime.Now;
		}

	}
}

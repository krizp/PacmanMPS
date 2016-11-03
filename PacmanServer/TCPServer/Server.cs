using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace TCPServer
{
	public class Server
	{
		protected TcpListener			_tcpListener;
		protected List<ClientNode>	_connectedClients;

		public Server() 
		{
			_tcpListener = null;
			_connectedClients = new List<ClientNode>();
		}

		public void Start(IPAddress ip, int port)
		{
			if ( _tcpListener == null )
			{
				_tcpListener = new TcpListener(ip, port);
				_tcpListener.Start();
				_tcpListener.BeginAcceptTcpClient(OnCompleteAcceptTcpClient, _tcpListener);
                
				Console.WriteLine("Server started...");
				Console.WriteLine("Waiting for clients...");
			}
		}

        public void Stop()
        {
            if ( _tcpListener != null )
            {
				lock (_connectedClients)
				{
					_connectedClients.ForEach(c => c.Disconnect());
				}

				_tcpListener.Stop();
				_tcpListener = null;

                Console.WriteLine("Server stoped");
            }
        }

        public bool IsRunning()
        {
            return _tcpListener != null;
        }

		private void OnCompleteAcceptTcpClient(IAsyncResult iar)
		{
			if (!IsRunning())
				return;

			TcpListener tcpl = (TcpListener)iar.AsyncState;
			TcpClient tclient = null;
			ClientNode cNode = null;

			try
			{
				tclient = tcpl.EndAcceptTcpClient(iar);

				Console.WriteLine("Client connected...");


				tcpl.BeginAcceptTcpClient(OnCompleteAcceptTcpClient, tcpl);


				cNode = new ClientNode(tclient, this);
				
				lock (_connectedClients)
				{
					_connectedClients.Add(cNode);
				}

				cNode.BeginRead();
			}
			catch (Exception exc)
			{
				Console.WriteLine("Exception in Server.OnCompleteAcceptTcpClient:");
				Console.WriteLine(exc.Message);
				//MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void DisconnectClient(ClientNode c)
		{
			lock (_connectedClients)
			{
				_connectedClients.Remove(c);
			}
		}

		public virtual void ProcessPayload(ClientNode c, string payload)
		{
			Console.WriteLine(c.ToString() + " " + payload);

			if (payload == "D")
			{
				DisconnectClient(c);
				c.Disconnect();
			}
		}

		public List<string> GetClientsInfo()
		{
			List<string> result = new List<string>();

			_connectedClients.ForEach(c => result.Add(c.name + " " + c.strId));

			return result;
		}

        public void SendToAllClients()
        {
			lock (_connectedClients)
			{
				string msg = "rrrrrr";

				Console.WriteLine("Send to " + _connectedClients.Count + " clients: " + msg);

				_connectedClients.ForEach(cn => cn.Send(msg));
			}
        }
    }
}
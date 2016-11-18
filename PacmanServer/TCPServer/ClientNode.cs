using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Numerics;


namespace TCPServer
{
    public class ClientNode : IEquatable<string>
    {
        public Player player;

        /* Used by server to send/receive data to/from this client*/
        private TcpClient _tcpClient;
		/* transmit and receive buffer */
		const int BUFFER_SIZE = 2048;
		private byte[] _tx, _rx;
		private bool _disconected;
		private Server _server;

        public string strId { get; set; }

        public ClientNode(TcpClient client, Server s)
        {
            _tcpClient = client;
            _tx = new byte[BUFFER_SIZE];
            _rx = new byte[BUFFER_SIZE];
            /* ip address of the client? */
			strId = _tcpClient.Client.RemoteEndPoint.ToString();
			_disconected = false;
			_server = s;

			player = new Player();
        }

        bool IEquatable<string>.Equals(string other)
        {
            if (string.IsNullOrEmpty(other)) return false;

            if (_tcpClient == null) return false;

            return strId.Equals(other);
        }

        /* Read the message from this client */
		public void BeginRead()
		{
			if ( _tcpClient.Connected )
				_tcpClient.GetStream().BeginRead(_rx, 0, _rx.Length, OnCompleteReadFromTCPClientStream, _tcpClient);
		}

        /* Called when BeginRead reads the entire message from this client */
		private void OnCompleteReadFromTCPClientStream(IAsyncResult iar)
		{
			if (_disconected)
				return;

			TcpClient tcpc;
			int nCountReadBytes = 0;
			string strRecv;

			try
            {
				tcpc = (TcpClient)iar.AsyncState;
				nCountReadBytes = tcpc.GetStream().EndRead(iar);

				if (nCountReadBytes == 0) // this happens when the client is disconnected
				{
					Console.WriteLine("Client disconnected");

					_server.DisconnectClient(this);
					
					return;
				}

				strRecv = Encoding.ASCII.GetString(_rx, 0, nCountReadBytes).Trim();

				_server.ProcessPayload(this, strRecv);
				if (_disconected)
					return;

				if ( strRecv != null )
					Console.WriteLine(strRecv);

				tcpc.GetStream().BeginRead(_rx, 0, _rx.Length, OnCompleteReadFromTCPClientStream, tcpc);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception in ClientNode.OnCompleteReadFromTCPClientStream:");
				Console.WriteLine(ex.Message);

				_server.DisconnectClient(this);
			}
		}

		public void Send(string msg)
		{
			if (_tcpClient != null)
			{
				if (_tcpClient.Client.Connected)
				{
					_tx = Encoding.ASCII.GetBytes(msg);
					_tcpClient.GetStream().BeginWrite(_tx, 0, _tx.Length, OnCompleteWriteToClientStream, _tcpClient);
				}
			}
		}

		private void OnCompleteWriteToClientStream(IAsyncResult iar)
		{
			try
			{
				TcpClient tcpc = (TcpClient)iar.AsyncState;
				tcpc.GetStream().EndWrite(iar);
			}
			catch (Exception exc)
			{
				Console.WriteLine("Exception in ClientNode.OnCompleteWriteToClientStream:");
				Console.WriteLine(exc.Message);

				_server.DisconnectClient(this);
            }
		}

		public void Disconnect()
		{
			_tcpClient.GetStream().Close();
			_tcpClient.Close();
			_disconected = true;

			Console.WriteLine(player.name + ":" + strId + " disconected!");
		}
	}
}

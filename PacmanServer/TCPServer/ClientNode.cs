using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

namespace TCPServer
{
    public class ClientNode : IEquatable<string>
    {
        private TcpClient _tcpClient;
        private byte[] _tx, _rx;
		private bool _disconected;
		private Server _server;

		public string strId { get; set; }
        public string name { get; set; }

        public ClientNode(TcpClient client, Server s)
        {
            _tcpClient = client;
            _tx = new byte[512];
            _rx = new byte[512];
			strId = _tcpClient.Client.RemoteEndPoint.ToString();
            name = "a";
			_disconected = false;
			_server = s;
        }

        bool IEquatable<string>.Equals(string other)
        {
            if (string.IsNullOrEmpty(other)) return false;

            if (_tcpClient == null) return false;

            return strId.Equals(other);
        }

        override public string ToString()
        {
            return name + ": " + strId;
        }
    
		public void BeginRead()
		{
			if ( _tcpClient.Connected )
				_tcpClient.GetStream().BeginRead(_rx, 0, _rx.Length, OnCompleteReadFromTCPClientStream, _tcpClient);
		}

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
					//MessageBox.Show("Client disconnected.");

					_server.DisconnectClient(this);

					//mlClientSocks.Remove(cn);
					//lbClients.Items.Remove(cn.ToString());
					return;
				}

				strRecv = Encoding.ASCII.GetString(_rx, 0, nCountReadBytes).Trim();
				//strRecv = Encoding.ASCII.GetString(mRx, 0, nCountReadBytes);

				//Server.Instance.ProcessPayload(this, strRecv);
				_server.ProcessPayload(this, strRecv);
				//ProcessPayload(this, strRecv);
				if (_disconected)
					return;

				if ( strRecv != null )
					Console.WriteLine(name + ": " + strRecv);

				tcpc.GetStream().BeginRead(_rx, 0, _rx.Length, OnCompleteReadFromTCPClientStream, tcpc);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception in ClientNode.OnCompleteReadFromTCPClientStream:");
				Console.WriteLine(ex.Message);
				//MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				_server.DisconnectClient(this);
			}
		}

		public void ProcessPayload(ClientNode c, string payload)
		{
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
				//MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				_server.DisconnectClient(this);
            }
		}

		public void Disconnect()
		{
			_tcpClient.GetStream().Close();
			_tcpClient.Close();
			_disconected = true;

			Console.WriteLine(name + " " + strId + " disconected!");
		}
	}
}

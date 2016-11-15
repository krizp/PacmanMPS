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
        public static int[,] labyrinth;

        public class Player
        {
            public Vector2 crt_dir; // (1, 0), (-1, 0), (0, 1), (0, -1)
            public Vector2 next_dir;
            public Vector2 pos;
            public Vector2 turn_point;
            public bool is_hyde;
            public float speed;
            public int id;


            public Player(int id)
            {
                this.id = id;
                crt_dir = NONE;
                next_dir = NONE;
                speed = jekyll_speed;

            }
           

        }
        public Player player;
        public static Vector2 RIGHT, LEFT, DOWN, UP, NONE;

        const float jekyll_speed = 5.0f;
        const float hyde_speed = jekyll_speed * 2;

        /* Used by server to send/receive data to/from this client*/
        private TcpClient _tcpClient;
        /* transmit and receive buffer */
        private byte[] _tx, _rx;
		private bool _disconected;
		private Server _server;

        const int WALL = 0;
        const int PATH = 1;

        public string strId { get; set; }
        public string name { get; set; }

        public ClientNode(TcpClient client, Server s)
        {
            RIGHT = new Vector2(1, 0);
            LEFT = new Vector2(-1, 0);
            DOWN = new Vector2(0, -1);
            UP = new Vector2(0, 1);
            NONE = new Vector2(0, 0);

            _tcpClient = client;
            _tx = new byte[512];
            _rx = new byte[512];
            /* ip address of the client? */
			strId = _tcpClient.Client.RemoteEndPoint.ToString();
            name = "a";
			_disconected = false;
			_server = s;
        }

        public void treatInput(string dir)
        {
            if (dir == "UP")
            {

                player.next_dir = UP;
                if (player.crt_dir == DOWN)
                {
                    player.crt_dir = UP;
                }

                update_turning_point();
            }
            else if (dir == "DOWN")
            {

                player.next_dir = DOWN;
                if (player.crt_dir == UP)
                {
                    player.crt_dir = DOWN;
                }

                update_turning_point();
            }
            else if (dir == "LEFT")
            {
                player.next_dir = LEFT;
                if (player.crt_dir == RIGHT)
                {
                    player.crt_dir = LEFT;
                }

                update_turning_point();
            }
            if (dir == "RIGHT")
            {
                player.next_dir = RIGHT;
                if (player.crt_dir == LEFT)
                {
                    player.crt_dir = RIGHT;
                }

                update_turning_point();
            }


        }

        int get_next_idx(float pos, float dir)
        {
            if (dir < 0)
            {
                return (int)Math.Floor(pos);
            }
            else if (dir > 0)
            {
                return (int)Math.Ceiling(pos);
            }
            else return (int)pos;
        }

        public void update_turning_point()
        {


            int startX = get_next_idx(player.pos.X, player.crt_dir.X);
            int startY = get_next_idx(player.pos.Y, player.crt_dir.Y);

            if (player.crt_dir == UP)
            {
                for (int j = startY; j < labyrinth.GetLength(0); j++)
                {
                    if (labyrinth[j,startX] == WALL)
                    {
                        player.turn_point = new Vector2(startX, j - 1);
                        break;
                    }
                    else if (player.next_dir.X != 0)
                    {
                        int tryX = startX + (int)player.next_dir.X;
                        if (labyrinth[j,tryX] == PATH)
                        {
                            player.turn_point = new Vector2(startX, j);
                            break;
                        }
                    }
                }
            }
            if (player.crt_dir == DOWN)
            {
                for (int j = startY; j >= 0; j--)
                {
                    if (labyrinth[j,startX] == WALL)
                    {
                        player.turn_point = new Vector2(startX, j + 1);
                        break;
                    }
                    else if (player.next_dir.X != 0)
                    {
                        int tryX = startX + (int)player.next_dir.X;
                        if (labyrinth[j,tryX] == PATH)
                        {
                            player.turn_point = new Vector2(startX, j);
                            break;
                        }
                    }
                }
            }
            if (player.crt_dir == RIGHT)
            {
                int len = labyrinth.GetLength(1);
                for (int i = startX; i < len; i++)
                {
                    if (labyrinth[startY,i] == WALL)
                    {
                        player.turn_point = new Vector2(i - 1, startY);
                        break;
                    }
                    else if (player.next_dir.Y != 0)
                    {
                        int tryY = startY + (int)player.next_dir.Y;
                        if (labyrinth[tryY,i] == PATH)
                        {
                            player.turn_point = new Vector2(i, startY);
                            break;
                        }
                    }
                }
            }
            if (player.crt_dir == LEFT)
            {
                for (int i = startX; i >= 0; i--)
                {
                    if (labyrinth[startY,i] == WALL)
                    {
                        player.turn_point = new Vector2(i + 1, startY);
                        break;
                    }
                    else if (player.next_dir.Y != 0)
                    {
                        int tryY = startY + (int)player.next_dir.Y;
                        if (labyrinth[tryY,i] == PATH)
                        {
                            player.turn_point = new Vector2(i, startY);
                            break;
                        }
                    }
                }

            }

            if (player.crt_dir == player.next_dir)
                player.next_dir = NONE;
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

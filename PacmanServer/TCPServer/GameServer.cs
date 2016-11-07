using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
	class GameServer : Server
	{
		public GameServer() : base()
		{
		}

		public void StartGame()
		{
			Console.WriteLine("Game Started!");
			_acceptClients = false;
		}

		public override void ProcessPayload(ClientNode c, string payload)
		{
			Console.WriteLine(c.ToString() + " " + payload);

			if (payload == "D")
			{
				DisconnectClient(c);
				c.Disconnect();
			}
		}
	}
}

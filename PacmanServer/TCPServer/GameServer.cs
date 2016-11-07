using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
	class GameServer : Server
	{
		MazeGenerator mg;

		public GameServer() : base()
		{
		}

		// Called when the game starts
		public void StartGame()
		{
			Console.WriteLine("Game Started!");

			// Deny any atempt to connect at the server
			_acceptClients = false;

			mg = new MazeGenerator();
			int[,] maze = mg.computeFinalMap();

			SendToAllClients(CreateMazePayload());
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

		private string CreateMazePayload()
		{
			throw new NotImplementedException();
		}
	}
}

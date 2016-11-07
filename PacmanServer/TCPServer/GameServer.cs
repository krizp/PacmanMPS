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
		int numOfReplies;


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

			SendToAllClients(CreateMazePayload(maze));
			numOfReplies = 0;
		}

		public override void ProcessPayload(ClientNode c, string payload)
		{
			Console.WriteLine(c.ToString() + " " + payload);

			if (payload == "Disconnect")
			{
				DisconnectClient(c);
				c.Disconnect();
			}

			if (payload == "Done creating labyrinth")
			{
				numOfReplies++;

				if (numOfReplies == _connectedClients.Count)
				{
					// Send message to all clients to start the game
					SendToAllClients("Start");
				}
			}
		}

		private string CreateMazePayload(int[,] maze)
		{
			string result = "";
			int dim = mg.getDimension();

			result += dim + "|" + (dim * 2 - 1) + "|";
			
			for (int i = 0; i < dim; ++i)
			{
				for (int j = 0; j < 2 * dim - 1; ++j)
					if (maze[i, j] == 0)
						result += "1";
					else
						result += "0";
			}

			return result;
		}
	}
}

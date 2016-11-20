using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;
using System.Diagnostics;
using System.Timers;

namespace TCPServer
{
	class GameServer : Server
	{
        Vector2 RIGHT = new Vector2(1, 0);
        Vector2 LEFT = new Vector2(-1, 0);
        Vector2 DOWN = new Vector2(0, -1);
        Vector2 UP = new Vector2(0, 1);
        Vector2 NONE = new Vector2(0, 0);
        
        List<String> clientInputs;
        MazeGenerator mg;

		int numOfReplies;
		Object repliesMutex = new object();

        const float razaCerc = 2.0f;

		System.Timers.Timer transitionTimer;

        Player hydePlayer;
        Object hydeMutex = new object();

        const int GAME_ON = 0;
        const int GAME_OVER = 1;

        public GameServer() : base()
		{
            clientInputs = new List<String>();
        }

        private int scoreToWin = 10;

		// Called when the game starts
		public void StartGame(int scoreToWin)
		{
			Console.WriteLine("Game Started!");

            this.scoreToWin = scoreToWin;

			// Deny any atempt to connect at the server
			_acceptClients = false;

			mg = new MazeGenerator();
			int[,] maze = mg.computeFinalMap();
			Player.labyrinth = maze;
			Player.labyrinthDim = mg.getDimension();

			SendToAllClients(CreateMazePayload(maze));
			numOfReplies = 0;

            foreach (ClientNode c in _connectedClients)
            {
                c.player.score = 0;
            }
		}

		public override void ProcessPayload(ClientNode c, string payload)
		{
			Console.WriteLine(c.ToString() + " " + payload);

			if (payload == "Disconnect")
			{
				DisconnectClient(c);
				c.Disconnect();

				SendToAllClients("M6|" + c.player.id);
			}

			else if (payload.Substring(0, 4) == "Name")
			{
				string[] data = payload.Split('|');
				int playerID = int.Parse(data[1]);
				_connectedClients[playerID].player.name = data[2];
			}

			else if (payload == "Done creating labyrinth")
			{
				lock (repliesMutex)
				{
					numOfReplies++;
					Console.WriteLine("numOfReplies = : " + numOfReplies);
				}

				if (numOfReplies == _connectedClients.Count)
				{
					numOfReplies = 0;

					Random random = new Random();
					int hyde = random.Next(_connectedClients.Count);
					hydePlayer = _connectedClients[hyde].player;
					hydePlayer.speed = Player.HYDE_SPEED;

					SendToAllClients(CreateInitPlayersPayload(hydePlayer.id));
				}
			}

			else if (payload == "Done positioning")
			{
				lock (mg)
				{
					numOfReplies++;
				}

				if (numOfReplies == _connectedClients.Count)
				{
					Console.WriteLine("Trimit start");
					SendToAllClients("Start");

					ThreadStart childref = new ThreadStart(playGame);
					Console.WriteLine("In Main: Creating the Child thread");
					Thread childThread = new Thread(childref);
					childThread.Start();
				}
			}
			// received input from a client
			else if (payload.Substring(0, 4) == "Dir:")
			{
				lock (clientInputs)
				{
					clientInputs.Add(payload.Substring(4));
				}
			}

			else if (payload == "Respown completed")
			{
				c.player.isHit = false;

				string message = "M4|" + c.player.id + "|"
					   + c.player.pos.X + ","
					   + c.player.pos.Y + "|"
					   + c.player.crt_dir.X + ","
					   + c.player.crt_dir.Y + "|"
					   + c.player.next_dir.X + ","
					   + c.player.next_dir.Y + "|"
					   + c.player.turn_point.X + ","
					   + c.player.turn_point.Y;

				SendToAllClients(message);
			}
		}

		public void playGame()
		{
			// salvam rezultatele simularii conform mesajelor din coada de inputs de la clienti
			List<String> replies = new List<String>();
			DateTime dtStart = DateTime.Now;
			DateTime dtStop;
			double dT = 0.0f;
			SetTimer();

			while (_connectedClients.Count > 0)
			{
				dtStop = DateTime.Now;
				dT = (dtStop - dtStart).TotalMilliseconds / 1000;
				dtStart = dtStop;


				// inputurile care au venit de la clienti si nu au fost procesate
				List<String> inputs = getClientInputs();

				foreach (String input in inputs)
				{
					string[] info = input.Split(',');
					//string message = "";
					int clientId = Int32.Parse(info[0]);
					int clientIndex = GetPlayerIndex(clientId);
					_connectedClients[clientIndex].player.ChangeDirection(info[1]);

					// mesajul trimis la clienti
					// id + pos + crt_dir + next_dir + turning_point
					string message = "M4|" + clientId + "|"
					   + _connectedClients[clientIndex].player.pos.X + ","
					   + _connectedClients[clientIndex].player.pos.Y + "|"
					   + _connectedClients[clientIndex].player.crt_dir.X + ","
					   + _connectedClients[clientIndex].player.crt_dir.Y + "|"
					   + _connectedClients[clientIndex].player.next_dir.X + ","
					   + _connectedClients[clientIndex].player.next_dir.Y + "|"
					   + _connectedClients[clientIndex].player.turn_point.X + ","
					   + _connectedClients[clientIndex].player.turn_point.Y;

					replies.Add(message);
				}

				// trimitem la clienti ce a actualizat server-ul in urma ultimelor input-uri
				foreach (string reply in replies)
				{
					SendToAllClients(reply);
				}
				replies.Clear();

				if (simulate((float)dT) == GAME_OVER)
                {
                    transitionTimer.Enabled = false;
                    break;
                }

				Thread.Sleep(Math.Max((int)(1000.0f / 60.0f - dT * 1000.0f), 0));
			}
		}

		int simulate(float dT)
		{
			for (int i = 0; i < _connectedClients.Count; ++i)
			{
				_connectedClients[i].player.Update(dT);
			}

			lock (hydeMutex)
			{
				foreach (var client in _connectedClients)
				{
					if (client.player == hydePlayer)
						continue;

					if (client.player.GetLabyrinthIndex() == hydePlayer.GetLabyrinthIndex() && !client.player.isHit)
					{
                        // coliziune
                        hydePlayer.score++;

                        if (hydePlayer.score == scoreToWin)
                        {
                            SendToAllClients("M8|Game Over");
                            return GAME_OVER;
                        }

                        client.player.Respown(hydePlayer.pos);
						SendToAllClients("M7|" + client.player.id + "|" + client.player.pos.X + "," + client.player.pos.Y);
						Console.WriteLine("COLIZIUNE!");
                        
						break;
					}
				}
			}

            return GAME_ON;
		}

		void SetTimer()
        {
            // Create a timer with a two second interval.
            transitionTimer = new System.Timers.Timer(20000);
            // Hook up the Elapsed event for the timer. 
            transitionTimer.Elapsed += OnTimedEvent;
            transitionTimer.AutoReset = true;
            transitionTimer.Enabled = true;
        }

        void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            lock (hydeMutex)
            {
                Player victim;
                float distance;
                findNearestVictim(out victim, out distance);

                if (victim != null)
                {
                    hydePlayer.speed = Player.JEKYLL_SPEED;
                    hydePlayer = victim;
                    hydePlayer.speed = Player.HYDE_SPEED;

                    SendToAllClients("M5|" + hydePlayer.id);
                }
            }
        }

        void findNearestVictim(out Player victim, out float distance)
        {
            float minim = Int32.MaxValue;

            victim = null;

            foreach (ClientNode client in _connectedClients)
            {
                if (client.player.id == hydePlayer.id)
                    continue;

				distance = Vector2.Distance(client.player.pos, hydePlayer.pos);
                if (distance < minim)
                {
                    minim = distance;
                    victim = client.player;
                }
            }

            distance = minim;
        }

		private int GetPlayerIndex(int id)
		{
			for (int i = 0; i < _connectedClients.Count; i++)
			{
				if (id == _connectedClients[i].player.id)
				{
					return i;
				}
			}

			return -1;
		}

        public List<string> getClientInputs()
        {
            List<string> r;
            lock (clientInputs)
            {
                r = clientInputs;
                clientInputs = new List<string>();
            }
            return r;
        }

        private string CreateMazePayload(int[,] maze)
		{
			string result = "M2|";
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

		private string CreateInitPlayersPayload(int hydeId)
		{
			//construim  M3
			// x1,y1|x2,y2|.....|xn,yn|id
			// xi,yi -> pozitiile de start ale jucatorilor
			// id -> cine este Hyde
			string result = "M3|";

			/* numarul de zone din harta */
			int zones = 2;
			int linesPerZone = mg.getDimension() / 2;
			int columnsPerZone = 0, zonesPerLine = 0;
			int startColumnZone = 0, startLineZone = 0;
			int clientRow = 0, clientColumn = 0;
			Random random = new Random();

			while (zones < _connectedClients.Count)
			{
				zones *= 2;
			}

			columnsPerZone = (mg.getDimension() * 2 - 1) / (zones / 2);

			zonesPerLine = zones / 2;

			for (int i = 0; i < _connectedClients.Count; ++i)
			{
				startColumnZone = (i % zonesPerLine) * columnsPerZone;
				startLineZone = (i / zonesPerLine) * linesPerZone;
				do
				{
					clientRow = random.Next(startLineZone, startLineZone + linesPerZone);
					clientColumn = random.Next(startColumnZone, startColumnZone + columnsPerZone);

				} while (mg.IsWall(clientRow, clientColumn));

				_connectedClients[i].player.pos = new Vector2(clientColumn, clientRow);

				result += _connectedClients[i].player.id + "," +
							_connectedClients[i].player.name + "," +
							_connectedClients[i].player.pos.X + "," +
							_connectedClients[i].player.pos.Y + "|";
			}

			result += hydeId;

			return result;
		}
	}
}

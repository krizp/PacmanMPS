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
        const int WALL = 0;
        const int PATH = 1;

        const float razaCerc = 2.0f;

        System.Timers.Timer transitionTimer;

        ClientNode.Player Hyde;
        Object hydeMutex = new Object();

        public GameServer() : base()
		{
            clientInputs = new List<String>();

 
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
                ClientNode.Player victim;
                float distance;
                findNearestVictim(out victim, out distance);

                Hyde = victim;

                SendToAllClients("M5" + Hyde.id);
            }
        }



        // Called when the game starts
        public void StartGame()
		{
			Console.WriteLine("Game Started!");

			// Deny any atempt to connect at the server
			_acceptClients = false;

			mg = new MazeGenerator();
			int[,] maze = mg.computeFinalMap();
            ClientNode.labyrinth = maze;

            SendToAllClients("M2" + CreateMazePayload(maze));
			numOfReplies = 0;
		}

        float euclidianDistance(Vector2 a, Vector2 b)
        {
            return (float) Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        void findNearestVictim(out ClientNode.Player victim, out float distance)
        {
            float minim = Int32.MaxValue;

            victim = null;

            foreach (ClientNode client in _connectedClients)
            {
                if (client.player.id == Hyde.id)
                    continue;

                distance = euclidianDistance(client.player.pos, Hyde.pos);
                if (distance < minim)
                {
                    minim = distance;
                    victim = client.player;
                }
            }

            distance = minim;
        }

        void simulate(float dT)
        {
            for (int i = 0; i < _connectedClients.Count; ++i)
            {
                if (_connectedClients[i].player.crt_dir == NONE)
                {
                    _connectedClients[i].player.crt_dir = _connectedClients[i].player.next_dir;
                    _connectedClients[i].update_turning_point();
                }

                if (_connectedClients[i].player.crt_dir == UP && _connectedClients[i].player.pos.Y >= _connectedClients[i].player.turn_point.Y ||
                    _connectedClients[i].player.crt_dir == DOWN && _connectedClients[i].player.pos.Y <= _connectedClients[i].player.turn_point.Y ||
                    _connectedClients[i].player.crt_dir == RIGHT && _connectedClients[i].player.pos.X >= _connectedClients[i].player.turn_point.X ||
                    _connectedClients[i].player.crt_dir == LEFT && _connectedClients[i].player.pos.X <= _connectedClients[i].player.turn_point.X)
                {
                    _connectedClients[i].player.pos = _connectedClients[i].player.turn_point;
                    _connectedClients[i].player.crt_dir = _connectedClients[i].player.next_dir;
                    _connectedClients[i].update_turning_point();
                }

                _connectedClients[i].player.pos = _connectedClients[i].player.pos + 
                    dT * _connectedClients[i].player.speed * _connectedClients[i].player.crt_dir;
            }

            lock (hydeMutex)
            {
                ClientNode.Player victim;
                float distance;
                findNearestVictim(out victim, out distance);

                if (distance < razaCerc)
                {
                    // TO DO -> coliziun jucatori
                }
            }
        }

        public void playGame()
        {
            // salvam rezultatele simularii conform mesajelor din coada de inputs de la clienti
            List<String> replies = new List<String>();
            Stopwatch sw = new Stopwatch();
            float dT = 0.0f;
            sw.Start();
            SetTimer();

            while (true)
            {
                // inputurile care au venit de la clienti si nu au fost procesate
                List<String> inputs = getClientInputs();
                
                foreach (String input in inputs)
                {
                    string[] info = input.Split(',');
                    //string message = "";
                    int clientId = Int32.Parse(info[0]);
                    _connectedClients[clientId].treatInput(info[1]);

                    // mesajul trimis la clienti
                    // id + pos + crt_dir + next_dir + turning_point
                     string message = "M4" + clientId + ","
                        + _connectedClients[clientId].player.pos.X +
                        ";" +  _connectedClients[clientId].player.pos.Y +","
                        + _connectedClients[clientId].player.crt_dir.X + ";" 
                        + _connectedClients[clientId].player.crt_dir.Y + ","
                        + _connectedClients[clientId].player.next_dir.X + ";" 
                        + _connectedClients[clientId].player.next_dir.Y + ","
                        + _connectedClients[clientId].player.turn_point.X + ";"
                        + _connectedClients[clientId].player.turn_point.Y;

                    replies.Add(message);
                }

                // trimitem la clienti ce a actualizat server-ul in urma ultimelor input-uri
                foreach (String reply in replies)
                {
                    SendToAllClients(reply);
                }
                replies.Clear();
                
                simulate(dT);
                sw.Stop();
                dT = (float)sw.Elapsed.TotalSeconds;
                Console.WriteLine((float)sw.Elapsed.TotalSeconds);
                sw.Restart();

                Thread.Sleep((int)(1000.0f / 60.0f) - (int) dT * 1000);
            }
        }
        public override void ProcessPayload(ClientNode c, string payload)
        {
            Console.WriteLine(c.ToString() + " " + payload);

            if (payload == "Disconnect")
            {
                DisconnectClient(c);
                c.Disconnect();
            }

            else if (payload == "Done creating labyrinth")
            {
                lock(mg)
                {
                    numOfReplies++;
                    Console.WriteLine("numOfReplies = : " + numOfReplies);
                }
                if (numOfReplies == _connectedClients.Count)
                {
                    //construim  M3
                    // x1,y1|x2,y2|.....|xn,yn|id
                    // xi,yi -> pozitiile de start ale jucatorilor
                    // id -> cine este Hyde
                    Console.WriteLine("Toti au primit matricea");
                    numOfReplies = 0;
                    /* numarul de zone din harta */
                    int zones = 2;
                    int linesPerZone = mg.getDimension() / 2;
                    int columnsPerZone = 0, zonesPerLine = 0;
                    int startColumnZone = 0, startLineZone = 0;
                    int clientRow = 0, clientColumn = 0;
                    Random random = new Random();
                    string message = "M3";
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
                        } while (mg.finalMap[clientRow, clientColumn] == WALL);

                        _connectedClients[i].player = new ClientNode.Player(i);


                        _connectedClients[i].player.pos = new Vector2(clientColumn, clientRow);
                        message += _connectedClients[i].player.pos.X + "," + _connectedClients[i].player.pos.Y + "|";

                    }


                    int hyde = random.Next(_connectedClients.Count);
                    message += hyde;
                    Hyde = _connectedClients[hyde].player;
                    // Send message to all clients to start the game
                    SendToAllClients(message);
                    Console.WriteLine("Am trimis mesajul :  " + message);
                }
            }

            else if (payload == "Done positioning")
            {
                lock(mg)
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
            else if (payload.Substring(0,4) == "Dir:")
            {
                lock(clientInputs)
                {
                    clientInputs.Add(payload.Substring(4));
                }
            }


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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

public class GameController : MonoBehaviour
{
	public Sprite ground;
	public Sprite wall;
	private float tileSize;

	public Camera mainCamera;

	public PersistentSharpConnector conn;
	public PersistentInitGameData initGameData;

	public Sprite hydeSprite;
	public Sprite jekyllSprite;


	// labirintul primit de la server
	private int[][] labyrinth;

	// lista cu toti playerii ce joaca
	private List<Player> players;

	// id-ul playerului ce ruleaza pe aceast calculator
	private int myID;

	// id-ul playerului ce este hyde
	private int hydeID;

	// aceasta valoarea va fi afisata pe ecran
	private int timer = 19;

	Timer transitionTimer = new System.Timers.Timer(1000);

	//HUD
	public GameObject canvas;
	public Text exampleText;
	// Use this for initialization
	void Start()
	{
		// iau datele intitiale ale jocului
		GameObject persistentInitDataObj = GameObject.FindGameObjectWithTag("PersistentInitGameData") as GameObject;
		initGameData = persistentInitDataObj.GetComponent<PersistentInitGameData>();

		// iau conectorul ce imi face legatura cu serverului
		GameObject persistentSharpConnectorObj = GameObject.FindGameObjectWithTag("PersistentSharpConnector") as GameObject;
		conn = persistentSharpConnectorObj.GetComponent<PersistentSharpConnector>();


		// Hook up the Elapsed event for the timer. 
		transitionTimer.Elapsed += OnTimedEvent;
		transitionTimer.AutoReset = true;
		transitionTimer.Enabled = true;


		// salvez datele initiale ale jocului
		labyrinth = initGameData.labyrinth;
		myID = initGameData.playerID;
		hydeID = initGameData.hydeID;
		players = initGameData.players;

		
		// calculez dimensiunea unui tile
		tileSize = ground.textureRect.width / ground.pixelsPerUnit;

		// setez camera pt a vedea tot labirintul
		Vector3 cameraPos = mainCamera.transform.position;
		cameraPos.x = (labyrinth[0].Length - 1) * tileSize / 2;
		cameraPos.y = (labyrinth.Length - 1) * tileSize / 2;

		mainCamera.transform.position = cameraPos;
		mainCamera.orthographicSize = (labyrinth[0].Length + 6) * tileSize / 2 / mainCamera.aspect;


		// initializez datele statice din clasa Player
		Player.HYDE_SPRITE = hydeSprite;
		Player.JEKYLL_SPRITE = jekyllSprite;
		Player.TILE_SIZE = tileSize;
		Player.labyrinth = labyrinth;


		// desenez labirintul
		DrawLabyrinth();

		// creez playerii ca obiecte
		int count = 1;
        Text childText;

        Text timer = GameObject.FindWithTag("Timer").GetComponent<Text>();

        foreach (var player in players)
		{
			player.createGameObject();

			childText = (Text) Instantiate(timer); 
			childText.text = player.name + "-" + player.points.ToString();
            childText.tag = "Info";
            Vector3 newpos = timer.transform.position;
            newpos.y -= 10 + count * 20;
            childText.transform.position = newpos;
            childText.transform.SetParent(canvas.transform, true);
            
            player.scoreLabel = childText;

            count = count + 1;
		}


	}


	// Update is called once per frame
	void Update()
	{
		if (conn == null)
		{
			return;
		}

		TreatInput();

		ProcessPayloads(conn.ReceiveFromServer());

		foreach (var player in players)
		{
			player.Update(Time.deltaTime);
		}
		List<Player> sorted = new List<Player> (players);

		sorted.Sort (delegate(Player x, Player y)
			{
				return x.points.CompareTo(y.points);
			});
        
		foreach (Transform child in canvas.transform) 
		{
			if (child.CompareTag("Timer"))
				child.GetComponent<Text> ().text ="Next change:"+timer.ToString();
		}

        foreach (Player player in sorted)
        {
            if (player.is_hyde)
                player.scoreLabel.text = "(HYDE)" + player.name + "-" + player.points;
            else
                player.scoreLabel.text = "(JEKYLL)" + player.name + "-" + player.points;
        }


		/*
		  void Update () {

		time -= UnityEngine.Time.deltaTime;


		float minutes = time / 60; 
		float seconds = time % 60;
		float fraction = (time * 100) % 100;


		timer.text = string.Format ("{0:00} : {1:00} : {2:00}",minutes-0.5, seconds, fraction);

		int count_pos = 0;
		var sortedPlayers = new List<PlayerController>();
		sortedPlayers.AddRange(players);
		sortedPlayers.Sort (delegate(PlayerController x, PlayerController y)
			{
				if (x.player.score == null && y.player.score== null) return 0;
				else if (x.player.score == null) return -1;
				else if (y.player.score == null) return 1;
				else return x.player.score.CompareTo(y.player.score);
			});
		
		foreach (Transform child in canvas.transform) 
		{
			if (child.CompareTag ("player1-text")) {
				child.GetComponent<Text> ().text = "Player " +"Score:" + player1.player.score;
				child.GetComponent<Text> ().color=new Color(1.1f, 3.2f, 0.3f);

			}


			if (child.CompareTag ("Player2-"+count_pos.ToString())) {
				child.GetComponent<Text> ().text = sortedPlayers[count_pos].tag +  " Score:" + sortedPlayers [count_pos].player.score;
				count_pos = count_pos + 1;
			}
		}
			
		int random_pos = Random.Range (0, possx.Count);
		if (Vector2.Distance (player1.player.pos, player2.player.pos) < 1.0) {

			player1.player.score = player1.player.score + 1;
			player2.GetComponent<SpriteRenderer> ().enabled = false;
			player2.placeOnTile (possx [random_pos], possy [random_pos]);

		} 
		for (var i = 0; i < numPlayers; i++) {
			if (Vector2.Distance (players [i].player.pos, player2.player.pos) < 1.0) {
				
				players [i].player.score = players [i].player.score + 1;
				player2.GetComponent<SpriteRenderer> ().enabled = false;
				player2.placeOnTile (possx [random_pos], possy [random_pos]);

			} else {
				player2.GetComponent<SpriteRenderer> ().enabled = true;
			}
		}

		*/




	 
	}

	private void TreatInput()
	{
		int myIndex = GetPlayerIndex(myID);


		if (Input.GetKeyDown(KeyCode.UpArrow) && players[myIndex].crt_dir != Vector2.up)
		{
			conn.SendToServer("Dir:" + myID + "," + "UP");
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow) && players[myIndex].crt_dir != Vector2.down)
		{
			conn.SendToServer("Dir:" + myID + "," + "DOWN");
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow) && players[myIndex].crt_dir != Vector2.left)
		{
			conn.SendToServer("Dir:" + myID + "," + "LEFT");
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) && players[myIndex].crt_dir != Vector2.right)
		{
			conn.SendToServer("Dir:" + myID + "," + "RIGHT");
		}
	}

	private void ProcessPayloads(List<string> receivedPayload)
	{
		foreach (string command in receivedPayload)
		{
			if (command.Substring(0, 2) == "M4")		// datele unui player
			{
				string data = command.Substring(3);
				string[] datas = data.Split('|');

				int id = int.Parse(datas[0]);
				string[] pos = datas[1].Split(',');
				string[] crt_dir = datas[2].Split(',');
				string[] next_dir = datas[3].Split(',');
				string[] turn_point = datas[4].Split(',');

				int playerIndex = GetPlayerIndex(id);

				players[playerIndex].pos = new Vector2(float.Parse(pos[0]), float.Parse(pos[1]));
				players[playerIndex].crt_dir = new Vector2(float.Parse(crt_dir[0]), float.Parse(crt_dir[1]));
				players[playerIndex].next_dir = new Vector2(float.Parse(next_dir[0]), float.Parse(next_dir[1]));
				players[playerIndex].turn_point = new Vector2(float.Parse(turn_point[0]), float.Parse(turn_point[1]));
			}
			else if (command.Substring(0, 2) == "M5")			// timer stop
			{
				transitionTimer.Stop();
				ChangeMrHyde(int.Parse(command.Substring(3)));
				timer = 19;
				transitionTimer.Enabled = true;
			}
			else if (command.Substring(0, 2) == "M6")			// client disconnected
			{
				int id = int.Parse(command.Substring(3));
				Player disconnectedPlayer = players.FirstOrDefault(p => p.id == id);
				disconnectedPlayer.RemoveFromLabyrinth();
				players.Remove(disconnectedPlayer);
			}
			else if (command.Substring(0, 2) == "M7")			// collision
			{
				string data = command.Substring(3);
				string[] datas = data.Split('|');

				int id = int.Parse(datas[0]);
				string[] pos = datas[1].Split(',');

				int playerIndex = GetPlayerIndex(id);
				players[playerIndex].Respown(int.Parse(pos[0]), int.Parse(pos[1]));
				players[playerIndex].points-=1;
				players[hydeID].points += 1;
				conn.SendToServer("Respown completed");
			}
            else if (command.Substring(0, 2) == "M8")           // game over
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
            }
		}
	}

	private int GetPlayerIndex(int id)
	{
		for (int i = 0; i < players.Count; i++)
		{
			if (id == players[i].id)
			{
				return i;
			}
		}

		return -1;
	}

	private void ChangeMrHyde(int newHydeID)
	{
		int hydeIndex = GetPlayerIndex(hydeID);
		players[hydeIndex].ChangeBehavior();

		hydeIndex = GetPlayerIndex(newHydeID);
		players[hydeIndex].ChangeBehavior();

		hydeID = newHydeID;
	}

	void OnTimedEvent(System.Object source, ElapsedEventArgs e)
	{

		//Debug.Log(timer);
		if (timer > 0)
			--timer;

	}

	private void DrawLabyrinth()
	{
		Sprite crtSprite;

		for (var i = 0; i < labyrinth.Length; i++)
		{
			for (var j = 0; j < labyrinth[i].Length; j++)
			{
				if (labyrinth[i][j] == 1)
				{
					crtSprite = wall;
				}
				else
				{
					crtSprite = ground;
				}

				GameObject newObj = new GameObject(crtSprite.name + "(" + i + "," + j + ")");
				SpriteRenderer renderer = newObj.AddComponent<SpriteRenderer>();
				renderer.sprite = crtSprite;
				renderer.sortingLayerName = "World";
				newObj.transform.position = new Vector3(j * tileSize, i * tileSize, 0);
			}
		}
	}
}

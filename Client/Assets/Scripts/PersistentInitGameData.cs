using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PersistentInitGameData : MonoBehaviour
{
	public int[][] labyrinth;

	public string playerName;
	public int playerID;
	public int hydeID;
	
	public List<Player> players;

    public string strRanking = "";
    bool isWinner = false;

	void Awake()
	{
		// make this object persistent
		DontDestroyOnLoad(this);
	}

	void Start()
	{
	}

	public void CreateInitPlayers(string s)
	{
		players = new List<Player>();

		string[] playersInfo = s.Split('|');

		for (int i = 0; i < playersInfo.Length - 1; ++i)
		{
			string[] playerInfo = playersInfo[i].Split(',');

			Player p = new Player();
			p.id = int.Parse(playerInfo[0]);
			p.name = playerInfo[1];
			p.pos.x = int.Parse(playerInfo[2]);
			p.pos.y = int.Parse(playerInfo[3]);

			players.Add(p);
		}


		hydeID = int.Parse(playersInfo[playersInfo.Length - 1]);

		Player hyde = players.FirstOrDefault(p => p.id == hydeID);
		hyde.is_hyde = true;
		hyde.speed = Player.HYDE_SPEED;
	}
	
	public void CreateLabyrinth(string s)
	{
		Debug.Log(s);

		string[] res = s.Split('|');

		int nl = int.Parse(res[0]);
		int nc = int.Parse(res[1]);

		labyrinth = new int[nl][];
		int k = 0;
		for (var i = 0; i < nl; i++)
		{
			labyrinth[i] = new int[nc];
			for (var j = 0; j < nc; j++)
			{
				labyrinth[i][j] = res[2][k++] - '0';
			}
		}
	}
}

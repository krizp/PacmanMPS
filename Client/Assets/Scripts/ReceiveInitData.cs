using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Timers;

public class ReceiveInitData : MonoBehaviour
{
    public PersistentSharpConnector conn;
	public PersistentInitGameData initGameData;

    public bool isFirstTime = false;

    // Use this for initialization
    void Start ()
	{
        if (!isFirstTime)
        {
            GameObject persistentInitDataObj = GameObject.FindGameObjectWithTag("PersistentInitGameData") as GameObject;
            initGameData = persistentInitDataObj.GetComponent<PersistentInitGameData>();

            GameObject persistentSharpConnectorObj = GameObject.FindGameObjectWithTag("PersistentSharpConnector") as GameObject;
            conn = persistentSharpConnectorObj.GetComponent<PersistentSharpConnector>();
        }
    }

	// Update is called once per frame
	void Update()
	{
		if (conn == null)
		{
			return;
		}

		ProcessPayloads(conn.ReceiveFromServer());
	}

	private void ProcessPayloads(List<string> receivedPayload)
    {
        foreach (string command in receivedPayload)
        {
            if (command == "Start")
            {
                // received the start game command
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }

            else if (command.Substring(0, 2) == "M1") // M1 -> primesc id
            {
				string[] data = command.Split('|');

				initGameData.playerID = int.Parse(data[1]);
                Debug.Log("Pasul 1");

				// trimit numele meu
				conn.SendToServer("Name|" + initGameData.playerID + "|" + initGameData.playerName);
            }

            else if (command.Substring(0, 2) == "M2") // M2 -> primesc matrice
            {
                // received the labyrinth
                initGameData.CreateLabyrinth(command.Substring(3));
                conn.SendToServer("Done creating labyrinth");
                Debug.Log("Pasul 2");
            }

            else if (command.Substring(0, 2) == "M3") // M3 -> primesc pozitiile initiale
            {
				initGameData.CreateInitPlayers(command.Substring(3));               
                conn.SendToServer("Done positioning");
                Debug.Log("Pasul 3");
            }
            //else if (command[1] == '4')
            //{
            //    string data = command.Substring(2);
            //    string[] datas = data.Split(',');
            //    int id = Int32.Parse(datas[0]);
            //    string[] pos = datas[1].Split(';');
            //    string[] crt_dir = datas[2].Split(';');
            //    string[] next_dir = datas[3].Split(';');
            //    string[] turn_point = datas[4].Split(';');


            //    playerController.players[id].pos = new Vector2(float.Parse(pos[0]), float.Parse(pos[1]));
            //    playerController.players[id].crt_dir = new Vector2(float.Parse(crt_dir[0]), float.Parse(crt_dir[1]));
            //    playerController.players[id].next_dir = new Vector2(float.Parse(next_dir[0]), float.Parse(next_dir[1]));
            //    playerController.players[id].turn_point = new Vector2(float.Parse(turn_point[0]), float.Parse(turn_point[1]));

            //}
            //else if (command[1] == '5')
            //{
            //    transitionTimer.Stop();
            //    playerController.transition(Int32.Parse(command.Substring(2)));
            //    timer = 19;
            //    transitionTimer.Enabled = true;

            //}


            /*if (initGameData.DoneCreatingLabyrinth())
            {
                
            }*/

        }
    }
}

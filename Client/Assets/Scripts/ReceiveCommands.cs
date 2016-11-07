using UnityEngine;
using System.Collections;

public class ReceiveCommands : MonoBehaviour {

    public PersistentSharpConnector conn;
	public PersistentInitGameData initGameData;

	bool receivedLabyrinth;

	// Use this for initialization
	void Start ()
	{
		receivedLabyrinth = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (conn == null)
        {
            return;
        }

        string receivedPayload;
        receivedPayload = conn.ReceveFromServer();
		
		if (receivedPayload == string.Empty)
		{
			// doesn't receive anything
			return;
		}
		
        if (receivedPayload == "Start")
        {
			// received the start game command
			UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
		else
		{
			// received the labyrinth
			initGameData.CreateLabyrinth(receivedPayload);
		}

		if (initGameData.DoneCreatingLabyrinth())
		{
			conn.SendToServer("Done creating labyrinth");
		}
	}
}

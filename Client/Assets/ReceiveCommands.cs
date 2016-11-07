using UnityEngine;
using System.Collections;

public class ReceiveCommands : MonoBehaviour {

    public PersistentSharpConnector conn;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (conn == null)
        {
            return;
        }

        string receivedPayload;

        receivedPayload = conn.ReceveFromServer();

        if (receivedPayload == "Start")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}

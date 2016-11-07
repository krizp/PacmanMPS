using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChangeLevel : MonoBehaviour
{
	public Text serverIpAddress;
	public Text userName;

	public PersistentSharpConnector conn;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void startGame()
    {
		if (conn.ConnectToServer(serverIpAddress.text, 2737))
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
		}
		else
		{
			Debug.Log("Connect to server FAILED! :(");
		}
    }
}

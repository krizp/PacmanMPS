using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChangeLevel : MonoBehaviour
{
	public Text serverIpAddress;
	public Text playerName;

	public PersistentSharpConnector conn;
	public PersistentInitGameData initData;

    public GameObject canvasMain;
    public GameObject canvasWaiting;

	// Use this for initialization
	void Start () {
        canvasWaiting.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void startGame()
    {
		if (conn.ConnectToServer(serverIpAddress.text, 2737))
		{
            Debug.Log("Connect to server succeeded!");

			initData.playerName = playerName.text;

			canvasMain.SetActive(false);
            canvasWaiting.SetActive(true);
		}
		else
		{
			Debug.Log("Connect to server FAILED! :(");
		}
    }
}

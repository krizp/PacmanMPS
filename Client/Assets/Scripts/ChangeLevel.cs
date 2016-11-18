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

    public bool isFirstTime = false;

	// Use this for initialization
	void Start () {
        canvasWaiting.SetActive(false);
        if (!isFirstTime)
        {
            GameObject persistentInitDataObj = GameObject.FindGameObjectWithTag("PersistentInitGameData") as GameObject;
            initData = persistentInitDataObj.GetComponent<PersistentInitGameData>();

            GameObject persistentSharpConnectorObj = GameObject.FindGameObjectWithTag("PersistentSharpConnector") as GameObject;
            conn = persistentSharpConnectorObj.GetComponent<PersistentSharpConnector>();
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void startGame()
    {
		if (isFirstTime && conn.ConnectToServer(serverIpAddress.text, 2737))
		{
            Debug.Log("Connect to server succeeded!");

			initData.playerName = playerName.text;

			canvasMain.SetActive(false);
            canvasWaiting.SetActive(true);
		}
        else if (!isFirstTime)
        {
            canvasMain.SetActive(false);
            canvasWaiting.SetActive(true);
        }
		else
		{
			Debug.Log("Connect to server FAILED! :(");
		}
    }

    public void exitGame()
    {
        Application.Quit();
    }
}

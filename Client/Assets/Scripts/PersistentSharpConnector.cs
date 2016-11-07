using UnityEngine;
using System.Collections;

public class PersistentSharpConnector : MonoBehaviour
{
	SharpConnector conn;

	void Awake()
	{
		// Stops object from automatically destroyed on loading a scene
		DontDestroyOnLoad(this);
	}

	// Use this for initialization
	void Start ()
	{
		conn = new SharpConnector();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	// called when the "join game" button is pressed
	public bool ConnectToServer(string ip, int port)
	{
		return conn.Connect(ip, port);
	}

	public void SendToServer(string payload)
	{
		conn.Send(payload);
	}

	public string ReceveFromServer()
	{
		return conn.Rececive();
	}

	void OnApplicationQuit()
	{
		try
		{
			conn.Disconnect();
		}
		catch (System.Exception ex)
		{
			Debug.Log(ex.Message);
		}
	}
}

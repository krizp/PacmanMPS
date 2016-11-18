using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PersistentSharpConnector : MonoBehaviour
{
    SharpClient conn;
    private bool connected;

	void Awake()
	{
		// Stops object from automatically destroyed on loading a scene
		DontDestroyOnLoad(this);
	}

	// Use this for initialization
	void Start ()
	{
		conn = new SharpClient();
        connected = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	// called when the "join game" button is pressed
	public bool ConnectToServer(string ip, int port)
	{
        connected = true;
		return conn.Connect(ip, port);
	}

	public void SendToServer(string payload)
	{
		conn.Send(payload);
	}

    public bool isConnected()
    {
        return connected;
    }

    public List<string> ReceiveFromServer()
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
        connected = false;
	}
}

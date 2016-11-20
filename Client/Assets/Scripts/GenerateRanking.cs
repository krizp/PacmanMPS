using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GenerateRanking : MonoBehaviour {

    private Text txt;
	// Use this for initialization
	void Start () {
        txt = GetComponent<Text>();

        GameObject persistentInitDataObj = GameObject.FindGameObjectWithTag("PersistentInitGameData") as GameObject;
        PersistentInitGameData initGameData = persistentInitDataObj.GetComponent<PersistentInitGameData>();

        txt.text = initGameData.strRanking;
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GenerateRanking : MonoBehaviour {

    public GameObject titleYouWon;
    public GameObject titleYouLost;

    private Text txt;
	// Use this for initialization
	void Start () {
        txt = GetComponent<Text>();

        GameObject persistentInitDataObj = GameObject.FindGameObjectWithTag("PersistentInitGameData") as GameObject;
        PersistentInitGameData initGameData = persistentInitDataObj.GetComponent<PersistentInitGameData>();

        txt.text = initGameData.strRanking;

        titleYouLost.SetActive(false);
        titleYouWon.SetActive(false);

        if (initGameData.isWinner)
        {
            titleYouWon.SetActive(true);
        } else
        {
            titleYouLost.SetActive(true);
        }
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}

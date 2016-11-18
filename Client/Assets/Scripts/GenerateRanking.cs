using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GenerateRanking : MonoBehaviour {

    private Text txt;
	// Use this for initialization
	void Start () {
        txt = GetComponent<Text>();
        int n = 12;
        for (int i = 1; i <= n; ++i)
        {
            if (i == 1)
                txt.text = i.ToString() + ". Player" + i.ToString() + "     " + (100 / i).ToString() + "\n";
            else
                txt.text += i.ToString() + ". Player" + i.ToString() + "     " + (100 / i).ToString() + "\n";
        }
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}

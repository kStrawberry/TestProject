using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CMainTitle : MonoBehaviour {
	
	Texture bg;
	GameObject battleroom;
	
	
	
	// Use this for initialization
	void Start () {
		this.bg = Resources.Load("images/title_blue") as Texture;
		this.battleroom = GameObject.Find("BattleRoom");
		this.battleroom.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (Input.GetMouseButtonDown(0))
		{
            this.battleroom.SetActive(true);
            gameObject.SetActive(false);
		}

        // [6/30/2015 kain0024] «√∑ø∆˚¿Ã æ»µÂ∑Œ¿ÃµÂ∏È
        if(Application.platform == RuntimePlatform.Android)
        {
            if(Input.GetKeyDown(KeyCode.Escape) )
                Application.Quit();
        }
	}
	
	void OnGUI()
	{
		GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), this.bg);
	}
}

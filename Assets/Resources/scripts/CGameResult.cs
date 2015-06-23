using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CGameResult : MonoBehaviour {

    List<Texture> img_players;
    GameObject battleRoom;
    Texture img_bg;
    Texture button_playagain;
    Texture button_exitgame;

    int red_count;
    int blue_count;

    float ratio = 1.0f;
    
    void Awake()
    {
        img_players = new List<Texture>();
        img_players.Add(Resources.Load("images/big_blue") as Texture);
        img_players.Add(Resources.Load("images/big_red") as Texture);

        img_bg = Resources.Load("images/graycell") as Texture;
        button_playagain = Resources.Load("images/playagain") as Texture;
        button_exitgame = Resources.Load("images/exitgame") as Texture;

        this.gameObject.SetActive(false);

    }
	// Use this for initialization
	void Start () {        
	
	}   
	
    public void SetPlayerCellCount(int red, int blue)
    {
        red_count = red;
        blue_count = blue;
    }
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), img_bg);

        ratio = Screen.width / 800.0f;

        if (GUI.Button(new Rect(10, 10, 80 * ratio, 80 * ratio), this.button_playagain))
        {
            StopAllCoroutines();
            battleRoom.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}

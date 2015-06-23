using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CGameResult : MonoBehaviour {

    List<Texture> img_players;
    GameObject battleRoom;

    int red_count;
    int blue_count;
    
    void Awake()
    {
        img_players = new List<Texture>();
        img_players.Add(Resources.Load("images/big_blue") as Texture);
        img_players.Add(Resources.Load("images/big_red") as Texture);
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
}

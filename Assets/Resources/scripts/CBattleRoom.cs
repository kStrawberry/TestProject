using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CBattleRoom : MonoBehaviour {
	
	public static readonly int COL_COUNT = 7;
	List<CPlayer> players;
	List<short> board;
	List<short> table_board;

	List<Texture> img_players;
	Texture background;
	Texture game_board;
	GUISkin blank_skin;
	
	Texture focus_cell;
	Texture button_playagain;
	
	List<short> available_attack_cells;
	byte current_player_index;
	byte step;

    GameObject gameResult;

    GUIStyle listStyle = null;

	void Awake()
	{
		table_board = new List<short>();
		available_attack_cells = new List<short>();
		focus_cell = Resources.Load("images/border") as Texture;
		
		blank_skin = Resources.Load("blank_skin") as GUISkin;
		game_board = Resources.Load("images/gameboard") as Texture;
		background = Resources.Load("images/gameboard_bg") as Texture;
		img_players = new List<Texture>();
		img_players.Add(Resources.Load("images/blue") as Texture);
		img_players.Add(Resources.Load("images/red") as Texture);
		
		button_playagain = Resources.Load("images/playagain") as Texture;
		
		players = new List<CPlayer>();
		for (byte i=0; i<2; ++i)
		{
			GameObject obj = new GameObject(string.Format("player{0}", i));
			CPlayer player = obj.AddComponent<CPlayer>();
			player.initialize(i);
			players.Add(player);
		}

		board = new List<short>();
        gameResult = GameObject.Find("GameResult");
        if(gameResult != null)
            gameResult.SetActive(false);

		reset ();       
        
	}    
	
	void reset()
	{
		players.ForEach (obj => obj.clear());
		players[0].add(6);
		players[0].add(42);
		//players[0].change_to_agent();
		
		players[1].add(0);
		players[1].add(48);
		players[1].change_to_agent();
		
		board.Clear();
		table_board.Clear();
		for (int i=0; i<COL_COUNT * COL_COUNT; ++i)
		{
			board.Add(short.MaxValue);
			table_board.Add((short)i);
		}
		
		players.ForEach(obj =>
        {
			obj.cell_indexes.ForEach(cell =>
            {
				board[cell] = obj.player_index;
			});
		});
		
		current_player_index = 0;
		step = 0;
	}
	
	float ratio = 1.0f;
	void OnGUI()
	{
		ratio = Screen.width / 800.0f;
		
		GUI.skin = blank_skin;
		draw_board();
        GUI.skin = null;

        if (listStyle == null)
        {
            listStyle = new GUIStyle(GUI.skin.box);
            listStyle.fontSize = 15;
            listStyle.normal.textColor = Color.white;
        }

        listStyle.fontSize = (int)(15.0f *ratio);

        GUI.TextField(new Rect(40 * ratio, 335, 90 * ratio, 30 * ratio), "Red : " + players[1].cell_indexes.Count.ToString(), listStyle);
        GUI.TextField(new Rect(660 * ratio, 335, 90 * ratio, 30 * ratio), "Blue : " + players[0].cell_indexes.Count.ToString(), listStyle);

        GUILayout.BeginHorizontal();
		if (GUI.Button(new Rect(10,10,80 * ratio, 80 * ratio), button_playagain))
		{
			StopAllCoroutines();
			reset();
		}
        
        //GUILayout.Label("", GUILayout.Width(100));

        GUILayout.Space(100 * ratio);
        if (GUILayout.Button("Undo", GUILayout.Width(80 * ratio), GUILayout.Height(80 * ratio)))
        {
            UndoPlay();
        }

        GUILayout.EndHorizontal();
	}
	
	void draw_board()
	{
		float scaled_height = 480.0f * ratio;
		float gap_height = Screen.height - scaled_height;
		
		float outline_top = gap_height * 0.5f;
		float outline_width = Screen.width;
		float outline_height = scaled_height;
		
		float hor_center = outline_width * 0.5f;
		float ver_center = outline_height * 0.5f;

		GUI.BeginGroup(new Rect(0, 0, outline_width, Screen.height));
		
		// Draw background to full of the screen.  
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);
		
		// Draw a board(alignment : center).
		GUI.DrawTexture(new Rect(0, outline_top, outline_width, outline_height), game_board);

		int width = (int)(60 * ratio);
		int celloutline_width = width * CBattleRoom.COL_COUNT;
		float half_celloutline_width = celloutline_width*0.5f;
		
		GUI.BeginGroup(new Rect(hor_center-half_celloutline_width, 
			ver_center-half_celloutline_width + outline_top, celloutline_width, celloutline_width));
		
		short index = 0;
		for (int row=0; row < CBattleRoom.COL_COUNT; ++row)
		{
			int gap_y = 0;
			for (int col=0; col < CBattleRoom.COL_COUNT; ++col)
			{
				int gap_x = 0;
				
				Rect cell_rect = new Rect(col * width + gap_x, row * width + gap_y, width, width);
				if (GUI.Button(cell_rect, ""))
				{
					on_click(index);
				}
				
				if (board[index] != short.MaxValue)
				{
					int player_index = board[index];
					GUI.DrawTexture(cell_rect, img_players[player_index]);
					
					if (current_player_index == player_index)
					{
						GUI.DrawTexture(cell_rect, focus_cell);
					}
				}
				
				if (available_attack_cells.Contains(index))
				{
					GUI.DrawTexture(cell_rect, focus_cell);
				}
				
				++index;
			}
		}
		GUI.EndGroup();
		GUI.EndGroup();
	}

	short selected_cell = short.MaxValue;
	void on_click(short cell)
	{
		//Debug.Log(cell);
		
		switch(this.step)
		{
		case 0:
			if (validate_begin_cell(cell))
			{
				this.selected_cell = cell;
				Debug.Log("go to step2");
				this.step = 1;
				
				refresh_available_cells(this.selected_cell);
			}
			break;
			
		case 1:
		{
			// When you touched your cell again.
			if (this.players[this.current_player_index].cell_indexes.Exists(obj => obj == cell))
			{
				this.selected_cell = cell;
				refresh_available_cells(this.selected_cell);
				break;
			}
			
			// Cannot touch other player's cell.
			foreach(CPlayer player in this.players)
			{
				if (player.cell_indexes.Exists(obj => obj == cell))
				{
					return;
				}
			}
			
			this.step = 2;
			StartCoroutine(on_selected_cell_to_attack(cell));
		}
		break;
			
		case 2:
			// Playin AI now.
			break;
			
		}
	}
	
	IEnumerator on_selected_cell_to_attack(short cell)
	{
		byte distance = CHelper.howfar_from_clicked_cell(this.selected_cell, cell);
		if (distance == 1)
		{
			// copy to cell
			yield return StartCoroutine(reproduce(cell));
			phase_end();
		}
		else if (distance == 2)
		{
			// move
			this.board[this.selected_cell] = short.MaxValue;
			this.players[this.current_player_index].remove(this.selected_cell);
			yield return StartCoroutine(reproduce(cell));
			phase_end();
		}
		
		yield return 0;
	}
	
	void game_over()
	{
        if (gameResult != null)
        {
            gameResult.SetActive(true);
            gameResult.GetComponent<CGameResult>().SetPlayerCellCount(players[0].cell_indexes.Count, players[1].cell_indexes.Count);

            // [6/23/2015 kain0024] 다시 시작하기 편하게 초기화 해 놓고 active(false) 한다.
            reset();
            this.gameObject.SetActive(false);
        }
        
		//Debug.Log("GameOver!");
	}
	
	void phase_end()
	{
		CPlayer victim_player = this.players[this.current_player_index];
		
		if (this.current_player_index == 0)
		{
			this.current_player_index = 1;
		}
		else
		{
			this.current_player_index = 0;
		}
		
		
		if (!CHelper.can_play_more(this.table_board, this.players, this.current_player_index))
		{
			game_over();
			return;
		}
		
		CPlayer attacker_player = this.players[this.current_player_index];
		if (attacker_player.state == PLAYER_STATE.AI)
		{
			this.step = 2;
			StartCoroutine(play_agent(attacker_player, victim_player));
		}
		else
		{
			this.step = 0;
		}
		
		this.available_attack_cells.Clear();
	}
	
	IEnumerator play_agent(CPlayer attacker_player, CPlayer victim_player)
	{
		CellInfo choice = attacker_player.run_agent(this.table_board, this.players, victim_player.cell_indexes);
		yield return new WaitForSeconds(0.5f);

		Debug.Log(string.Format("{0} -> {1} = {2}", choice.from_cell, choice.to_cell, choice.score));
		this.selected_cell = choice.from_cell;
		StartCoroutine(on_selected_cell_to_attack(choice.to_cell));
	}
	
	void refresh_available_cells(short cell)
	{
		this.available_attack_cells = CHelper.find_available_cells(cell, this.table_board, this.players);
	}
	
	void clear_available_attacking_cells()
	{
		this.available_attack_cells.Clear();
	}
	
	IEnumerator reproduce(short cell)
	{
		CPlayer current_player = this.players[this.current_player_index];
		CPlayer other_player = this.players.Find(obj => obj.player_index != this.current_player_index);
		
		clear_available_attacking_cells();
		yield return new WaitForSeconds(0.5f);
		
		this.board[cell] = current_player.player_index;
		current_player.add(cell);

		yield return new WaitForSeconds(0.5f);
		
		// eat.
		List<short> neighbors = CHelper.find_neighbor_cells(cell, other_player.cell_indexes, 1);
		foreach (short obj in neighbors)
		{
			this.board[obj] = current_player.player_index;
			current_player.add(obj);
			
			other_player.remove(obj);
			
			yield return new WaitForSeconds(0.2f);
		}
	}
	
	bool validate_begin_cell(short cell)
	{
		return this.players[this.current_player_index].cell_indexes.Exists(obj => obj == cell);
	}

    //--------------------------------------------------------------------------------------------------------
    void UndoPlay()
    {

    }
}

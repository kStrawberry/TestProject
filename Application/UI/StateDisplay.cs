using UnityEngine;
using System.Collections;

[AddComponentMenu("Display/State")]
public class StateDisplay : MonoBehaviour {

    public float FPSfrequency = 0.5f;
    float AccumTime = 0;
    int frames = 0;
    int MaxFPS = 0;
    int MinFPS = 1000;
    int CurrentFPS = 0;
    public bool allowDrag = true;
    public bool updateColor = true;
    private Color color = Color.white;
    private GUIStyle style;
    public Rect startRect = new Rect(10, 10, 150, 60);

    public int FramePerSec { get; protected set; }
	// Use this for initialization
	void Start () {
        StartCoroutine(FPSCount());
	}
	
	// Update is called once per frame
	void Update () {
        AccumTime += Time.timeScale / Time.deltaTime;
        ++frames;
	}

    IEnumerator FPSCount()
    {
        while(true)
        {
            if (frames == 0 || AccumTime == 0)
                yield return null;

            CurrentFPS = (int)AccumTime / frames;
            
            AccumTime = 0;
            frames = 0;

            if (CurrentFPS < MinFPS)
                MinFPS = CurrentFPS;

            if (MaxFPS < CurrentFPS)
                MaxFPS = CurrentFPS;

            yield return new WaitForSeconds(FPSfrequency);
        }        
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.white;
        }

        GUI.color = updateColor ? color : Color.white;
        startRect = GUI.Window(0, startRect, DoMyWindow, "");
    }

    void DoMyWindow(int windowID)
    {
        GUI.Label(new Rect(5, 3, startRect.width, startRect.height), "FPS : " + CurrentFPS + " fps", style);
        GUI.Label(new Rect(5, 18, startRect.width, startRect.height), "MAX FPS : " + MaxFPS + " fps" , style);
        GUI.Label(new Rect(5, 33, startRect.width, startRect.height), "MIN FPS : " + MinFPS + " fps ", style);

        if (allowDrag)
            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
    }
}

using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour {

	void Awake()
    {
        // [5/20/2015 kain0024] 비 활성화 되어 있는 오브젝트를 활성화 시킨다.
        //gameObject.SetActiveRecursively(true);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
	}
}

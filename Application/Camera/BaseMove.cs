using UnityEngine;
using System.Collections;

public class BaseMove : MonoBehaviour {

    public float MoveSpeed = 50.0f;
	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * MoveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * MoveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.Space) )
        {
            MoveSpeed *= 2.0f;

            if (200.0f < MoveSpeed)
                MoveSpeed = 50.0f;
        }
        
	}
}

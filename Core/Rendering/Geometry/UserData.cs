using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserData : MonoBehaviour {

    //[HideInInspector]
    public List<string> propertyList;       
	
    public void InitPropList()
    {
        propertyList = new List<string>();
    }

    public void SetUserData(string data)
    {
        propertyList.Add(data);
    }
   
}

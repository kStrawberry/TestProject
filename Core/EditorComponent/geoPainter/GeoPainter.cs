using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PainterObjectRegisterInfo;

public class GeoPainter : MonoBehaviour {

    public GameObject borderObject;
	public int paintLayer = 0;
	//ArrayList myGroups = new ArrayList();
	public GameObject[] myGroupsBuiltIn;
	public int nbrGroupsCreated;
	public float myDistance = 0.5f;
	public float mySpray = 1;
	public float myDelete = 2;
	public bool useNormal = true;
	public int groupSelIndex = 1;    

	public int bibSortIndex = 0;
	public int bibSoloSelect = 0;
        
	public bool rndAuto = false;

    // [6/3/2015 kain0024] 새로 추가된 변수들 넣어 줘야 함.    
    [SerializeField]
    public PainterObjectGroupInfo[] selectGroups;    
    
}

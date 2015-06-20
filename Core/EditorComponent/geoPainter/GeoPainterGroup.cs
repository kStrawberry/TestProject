using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PainterObjectRegisterInfo;

public class GeoPainterGroup : MonoBehaviour {

    public GameObject[] myLibraryBuiltIn = null;
    public int rndSeed = 1;    

    [SerializeField]
    public List<GeoPainterPoint> myPointsList = new List<GeoPainterPoint>();

    // [6/3/2015 kain0024] 오브젝트 설정
    [SerializeField]
    public PainterObjectInfo[] selectObject; 

    
    //Position
	public float offPosX = 0.0f;
	public float offPosY = 0.0f;
	public float offPosZ = 0.0f;
	public float rndPosMinX = 0.0f;
    public float rndPosMinY = 0.0f;
    public float rndPosMinZ = 0.0f;
    public float rndPosMaxX = 0.0f;
    public float rndPosMaxY = 0.0f;
    public float rndPosMaxZ = 0.0f;
	
	//Rotation
    public float offRotX = 0.0f;
    public float offRotY = 0.0f;
    public float offRotZ = 0.0f;
    public float rndRotMinX = 0.0f;
    public float rndRotMinY = 0.0f;
    public float rndRotMinZ = 0.0f;
    public float rndRotMaxX = 0.0f;
    public float rndRotMaxY = 0.0f;
    public float rndRotMaxZ = 0.0f;
	
	//Scale
    public bool scaleUniform = false;
    public float offSclX = 0.0f;
    public float offSclY = 0.0f;
    public float offSclZ = 0.0f;
    public float rndSclMinX = 0.0f;
    public float rndSclMinY = 0.0f;
    public float rndSclMinZ = 0.0f;
    public float rndSclMaxX = 0.0f;
    public float rndSclMaxY = 0.0f;
    public float rndSclMaxZ = 0.0f;

	public void addObject(GameObject _go, Vector3 _pos, Vector3 _scale, Vector3 _normal, bool _useNormal)
	{
		GeoPainterPoint myNewPoint = new GeoPainterPoint();
		myNewPoint.go = _go;
		myNewPoint.pos = _pos;
		myNewPoint.scale = _scale;
		myNewPoint.normal = _normal;
		myNewPoint.useNormal = _useNormal;
		myPointsList.Add(myNewPoint);
	}
}

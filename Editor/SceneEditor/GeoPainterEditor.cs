using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using PainterObjectRegisterInfo;

/************************************************************************/
/* 기본 UX는 레이어 선택 -> 레이어에 등록된 머트리얼 선택 및 group 셋팅
 * Library에서 브러쉬 할 오브젝트 선택 및 셋팅 ->
 * 브러쉬 질을 해도 되고 Fire로 오브젝트 뿌려도 됨!
 */
/************************************************************************/
[CustomEditor(typeof(GeoPainter))]
public class GeoPainterEditor : Editor {

/*
    public delegate void randomObjectPlacer();
    public static event randomObjectPlacer batcher;
*/

    static string appTitle = "GSwing Painter Tools";
    bool isPainting = false;
    List<GameObject> myGroups;
    List<GameObject> myLibrary = null;
    List<GameObject> myObjToInstArray;

    GeoPainter geoPainter;

    // [6/5/2015 kain0024] border rect
    //[SerializeField]
    //public static GameObject borderRect;

    bool showGroups = true;
    bool showPaint = true;
    bool showBiblio = true;
    bool showRandom = true;
    
    GameObject currentGroup;
    GeoPainterGroup currentGroupScript;
    
    int copyFromIndex = 0;
    List<string> myCopyFromPop;// = new List<string>();

    // 드랍 다운 메뉴
    public static string[] list = { "obin", "obout", "rough", "mrough", "hrough" };
    GUIStyle style = null;
    GUIStyle listStyle = null;
    // [6/3/2015 kain0024] select material index(인스펙터가 변경되면 변수가 null로 초기화 되서 static로 변경 하였다.)
    List<PainterObjectGroupInfo> selectGroups = null;
    List<PainterObjectInfo> selectObject = null;

    void SetObjectInfo()
    {
        if (geoPainter.borderObject == null)
            return;

        // [6/15/2015 kain0024] geopainter에서 설정한 정보를 randomplacer에 넣어 주자.
        RandomPlacer placer = geoPainter.borderObject.GetComponent<RandomPlacer>();
        if (placer != null)
        {
            // 오브젝트 리스트, distance, persent
            PainterObjectGroupInfo groupInfo = selectGroups[geoPainter.groupSelIndex - 1];
            placer.SetBatchInfo(groupInfo.objectCounts, myLibrary.ToArray(), selectObject.ToArray());
        }       
    }

    public override void OnInspectorGUI()
    {
        // Check if an object has been deleted
        if(currentGroupScript != null && currentGroupScript.myPointsList.Count != 0)
        {
            for(int i = currentGroupScript.myPointsList.Count - 1; 0 <= i; --i)
            {
                var element = currentGroupScript.myPointsList[i];
                if (element.go == null)
                    currentGroupScript.myPointsList.RemoveAt(i);
            }
        }

        if(geoPainter == null)
            geoPainter = target as GeoPainter;

        if (geoPainter == null)
            return;

        if(myGroups == null)
        {
            myGroups = new List<GameObject>();
            if (geoPainter.myGroupsBuiltIn != null)
            {
                if (selectGroups == null)
                    selectGroups = new List<PainterObjectGroupInfo>();            
                
                // [6/3/2015 kain0024] 씬을 로딩 하거나 인스펙터를 새로 갱신 했을 때 넣어 주는 루틴
                for (int j = 0; j < geoPainter.myGroupsBuiltIn.Length; ++j )
                {
                    GameObject obj = geoPainter.myGroupsBuiltIn[j];
                    if (obj != null)
                    {
                        GeoPainterGroup painterGroup = obj.GetComponent<GeoPainterGroup>();
                        if(painterGroup.selectObject != null && j == 0)
                        {
                            if (selectObject == null)
                                selectObject = new List<PainterObjectInfo>(painterGroup.selectObject);                            
                        }
                        
                        myGroups.Add(obj);
                    }                    
                    
                    PainterObjectGroupInfo groupInfo = geoPainter.selectGroups[j];
                    if (groupInfo != null)
                        selectGroups.Add(groupInfo);                
                }
            }

            geoPainter.groupSelIndex = 1;
        }

        // [6/5/2015 kain0024] 메인신 등록
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Border Rect : ", GUILayout.Width(80));
        geoPainter.borderObject = (GameObject)EditorGUILayout.ObjectField(geoPainter.borderObject, typeof(GameObject), false, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        showGroups = EditorGUILayout.Foldout(showGroups, "GROUPS", EditorStyles.boldLabel);

        // [6/4/2015 kain0024] 그래픽팀 요청으로 layer를 최 상위로 올려 놓았음.
        geoPainter.paintLayer = EditorGUILayout.LayerField("Paint Layer : ", geoPainter.paintLayer);  //original
        //geoPainter.paintLayer = EditorGUILayout.MaskField("Paint Layer(s) : ", geoPainter.paintLayer, LayerNames() );
        EditorGUILayout.Space();
        if(showGroups && !isPainting)
        {
            // [5/22/2015 kain0024] add, remove groups
#region Add/Remove
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+", GUILayout.Width(150), GUILayout.MinWidth(100)))
            {
                addGroup();
                geoPainter.myGroupsBuiltIn = myGroups.ToArray() as GameObject[];
                // [6/3/2015 kain0024] groupinfo 정보 갱신
                geoPainter.selectGroups = selectGroups.ToArray() as PainterObjectGroupInfo[];
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(40));
            EditorGUILayout.LabelField("Object Name", GUILayout.Width(180));
            EditorGUILayout.LabelField("Material", GUILayout.Width(100));
            EditorGUILayout.LabelField("Number", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
               
            if(0 < myGroups.Count)
            {
                // [5/22/2015 kain0024] groups Display
                int nbrTemp = 1;
                if (style == null)
                    style = new GUIStyle(GUI.skin.label);

                //foreach(GameObject obj in myGroups)
                for (int i = 0; i < myGroups.Count; ++i)
                {
                    GameObject obj = myGroups[i];
                    var myLabel = "" + nbrTemp + ": ";
                    EditorGUILayout.BeginHorizontal();
                    if (nbrTemp == geoPainter.groupSelIndex)
                        style.normal.textColor = Color.red;
                    else
                        style.normal.textColor = Color.black;

                    EditorGUILayout.TextField(myLabel + " " + obj.name, style, GUILayout.Width(200));
                    
                    if (listStyle == null)
                    {                        
                        listStyle = new GUIStyle(GUI.skin.button);
                        listStyle.fontSize = 12;                        
                    }

                    if (selectGroups != null)
                    {
                        PainterObjectGroupInfo groupInfo = selectGroups[nbrTemp - 1];
                        groupInfo.selectMaterials = EditorGUILayout.Popup(groupInfo.selectMaterials, list, listStyle, GUILayout.Width(100));
                        groupInfo.objectCounts = EditorGUILayout.IntField(groupInfo.objectCounts, GUILayout.Width(100));
                        if (groupInfo.objectCounts <= 0)
                            groupInfo.objectCounts = 0;

                        geoPainter.selectGroups = selectGroups.ToArray() as PainterObjectGroupInfo[];
                    }

                    if (GUILayout.Button("EDIT", GUILayout.Width(100)) )
                    {
                        geoPainter.groupSelIndex = nbrTemp;
                        currentGroup = myGroups[nbrTemp - 1];
                        currentGroupScript = currentGroup.GetComponent<GeoPainterGroup>();
                        myLibrary = new List<GameObject>(currentGroupScript.myLibraryBuiltIn);
                        var position = currentGroup.transform.position;
                        SceneView.lastActiveSceneView.pivot = position;
                        SceneView.lastActiveSceneView.Repaint();

                        // [6/3/2015 kain0024] library 오브젝트 속성을 가져 온다.
                        //selectObject.Clear();
                        selectObject = new List<PainterObjectInfo>(currentGroupScript.selectObject);
                    }

                    if (GUILayout.Button("REMOVE", GUILayout.Width(100)))
                    {
                        removeGroup(nbrTemp, false);
                        selectGroups.RemoveAt(nbrTemp - 1);
                        geoPainter.myGroupsBuiltIn = myGroups.ToArray() as GameObject[];

                        // [6/3/2015 kain0024] groupinfo 정보 갱신
                        geoPainter.selectGroups = selectGroups.ToArray() as PainterObjectGroupInfo[];
                    }              
                    
                    EditorGUILayout.EndHorizontal();
                    ++nbrTemp;
                }
            }
#endregion
        }

            //PANELS
        if (0 < myGroups.Count)
        {
            currentGroup = myGroups[geoPainter.groupSelIndex - 1];
            currentGroupScript = currentGroup.GetComponent<GeoPainterGroup>();

            #region BIBLIO
            if (myLibrary == null)
                myLibrary = new List<GameObject>(currentGroupScript.myLibraryBuiltIn);

            // [6/3/2015 kain0024] 
            if(selectObject == null)
            {
                if(currentGroupScript.selectObject != null)
                    selectObject = new List<PainterObjectInfo>(currentGroupScript.selectObject);
                else
                    selectObject = new List<PainterObjectInfo>();
            }

            EditorGUILayout.Space();

            showBiblio = EditorGUILayout.Foldout(showBiblio, "LIBRARY", EditorStyles.boldLabel);
            if (showBiblio && !isPainting)
            {               
                EditorGUILayout.Space();
                //elements
                if (GUILayout.Button("ADD OBJECT", GUILayout.Width(200)))
                {
                    myLibrary.Add(null);
                    currentGroupScript.myLibraryBuiltIn = myLibrary.ToArray() as GameObject[];

                    PainterObjectInfo objInfo = new PainterObjectInfo();
                    objInfo.Init();
                    selectObject.Add(objInfo);
                    currentGroupScript.selectObject = selectObject.ToArray() as PainterObjectInfo[];
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(50));
                EditorGUILayout.LabelField("Object Name", GUILayout.Width(210));
                EditorGUILayout.LabelField("Distance", GUILayout.Width(100));
                EditorGUILayout.LabelField("Persent", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                for (int x = 0; x < myLibrary.Count; ++x)
                {
                    EditorGUILayout.BeginHorizontal();
                    //EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("#" + x + " : ", GUILayout.Width(30));
                    myLibrary[x] = (GameObject)EditorGUILayout.ObjectField(myLibrary[x], typeof(GameObject), false, GUILayout.Width(200));
                    //EditorGUILayout.EndVertical();
                    if (selectObject != null)
                    {
                        selectObject[x].distance = EditorGUILayout.FloatField(selectObject[x].distance, GUILayout.Width(100));
                        selectObject[x].perSent = EditorGUILayout.IntField(selectObject[x].perSent, GUILayout.Width(100));
                    }
                    EditorGUILayout.BeginVertical();
                    if (GUILayout.Button("REMOVE", GUILayout.Width(150)))
                    {
                        myLibrary.RemoveAt(x);                        
                        currentGroupScript.myLibraryBuiltIn = myLibrary.ToArray() as GameObject[];
                        // [6/3/2015 kain0024] 
                        selectObject.RemoveAt(x);
                        currentGroupScript.selectObject = selectObject.ToArray() as PainterObjectInfo[];
                        break;
                    }

                    EditorGUILayout.EndVertical();

                    if (myLibrary[x] != null)
                    {
                        Texture2D previewTexture = AssetPreview.GetAssetPreview(myLibrary[x]);
                        GUILayout.Button(previewTexture, GUILayout.Width(50), GUILayout.Height(50));
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }

                if (GUI.changed)
                    currentGroupScript.myLibraryBuiltIn = myLibrary.ToArray() as GameObject[];

                GUI.changed = false;
                if (myLibrary.Count != 0)
                {
                    if (myLibrary.Count - 1 < geoPainter.bibSoloSelect)
                        geoPainter.bibSoloSelect = myLibrary.Count - 1;
                }

                if (geoPainter.bibSoloSelect < 0)
                    geoPainter.bibSoloSelect = 0;

                EditorGUILayout.Space();
                if (GUILayout.Button("Update Prefabs", GUILayout.Height(30), GUILayout.Width(200)))
                {
                    updatePrefab();
                    randomize();
                }
                if (GUILayout.Button("Replace Prefab", GUILayout.Height(30), GUILayout.Width(200)))
                    replacePrefab();
            }
            #endregion
            #region PAINT
            // [5/22/2015 kain0024] 이전 컨트롤과 다음 사이에 작은 공간을 생성
            EditorGUILayout.Space();
            showPaint = EditorGUILayout.Foldout(showPaint, "PAINT", EditorStyles.boldLabel);

            if (showPaint)
            {
                EditorGUILayout.Space();        

                if (!isPainting)
                {
                    // [6/3/2015 kain0024] 선택된 머트리얼에 library object 뿌리는 버튼..
                    if (GUILayout.Button("Fire Object", GUILayout.Height(40), GUILayout.Width(200)))
                    {
                        // 체크할 항목은 number != 0, persent != 0, prefab != null, persent != 100 아니면 뿌리자!!
                        if (selectGroups[geoPainter.groupSelIndex - 1].objectCounts == 0)
                        {
                            // [6/4/2015 kain0024] 인게임 모드에서는 당연히 버튼이 작동 되니 상관 없음
                            Debug.Log("Current Group Number is 0");
                        }

                        int totalPersent = 0;
                        for(int i = 0; i < myLibrary.Count; ++i)
                        {
                            GameObject obj = myLibrary[i];
                            PainterObjectInfo objInfo = selectObject[i];

                            if (obj == null || objInfo.distance == 0 || objInfo.perSent == 0)
                            {
                                Debug.Log("obj == null or objInfo.distance == 0 or objInfo.perSent == 0");
                                return;
                            }

                            totalPersent += objInfo.perSent;
                        }

                        if (100 != totalPersent)
                        {
                            Debug.Log("total persent over 100");
                        }                        
                        
                        // 등록된 오브젝트에서 layername과 같은 자식을 찾는다.
                        GameObject targetObj = GameObject.FindGameObjectWithTag("mainScene") as GameObject;
                        
                        if(targetObj == null)   //자식을 찾지 못하면
                        {
                            Debug.Log("layername target object is null");
                            return;
                        }

                        string layerName = LayerMask.LayerToName(geoPainter.paintLayer);
                        Transform transObj = targetObj.transform.FindChild(layerName);

                        // [6/5/2015 kain0024] 레이어 이름을 가져 와서 오브젝트를 뿌릴 메쉬를 선택하고
                        if (geoPainter.borderObject != null && transObj != null)
                        {                            
                            Mesh mesh = transObj.GetComponent<MeshFilter>().sharedMesh;
                            // [6/15/2015 kain0024] 오브젝트 심을 평면을 만들고,
                            geoPainter.borderObject.transform.position = transObj.position;
                            geoPainter.borderObject.transform.Translate(new Vector3(0, 100.0f, 0.0f));
                            geoPainter.borderObject.transform.localScale = new Vector3(mesh.bounds.size.x, 1.0f, mesh.bounds.size.y);

                            // [6/15/2015 kain0024] 오브젝트 배치를 불러주기 전에 설정을 해 줘야 한다.

                            SetObjectInfo();                            
                        }
                    }

                    if (GUILayout.Button("Start Painting", GUILayout.Height(40), GUILayout.Width(200)))
                    {
                        geoPainter.rndAuto = false;
                        myObjToInstArray = new List<GameObject>();
                        isPainting = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Painting (CTRL + Click) = Paint (SHIFT + Click) = Erase", GUILayout.Height(40), GUILayout.Width(200)))
                    {
                        geoPainter.myGroupsBuiltIn = myGroups.ToArray() as GameObject[];
                        isPainting = false;
                    }
                }

                if (GUILayout.Button("Clean Painting", GUILayout.Height(30), GUILayout.Width(200)))
                {
                    for (int i = currentGroupScript.myPointsList.Count - 1; 0 <= i; --i)
                    {
                        var element = currentGroupScript.myPointsList[i];
                        DestroyImmediate(element.go);
                        currentGroupScript.myPointsList.RemoveAt(i);
                    }

                    currentGroupScript.transform.position = new Vector3(0, 0, 0);
                    currentGroupScript.transform.Rotate(0, 0, 0);
                }

                //Distance Radius
                EditorGUILayout.BeginHorizontal();  // 수평 그룹 편집 시작(end와 쌍으로 와야 함)
                EditorGUILayout.PrefixLabel("Distance Radius(D, SHIFT+D): ");
                geoPainter.myDistance = EditorGUILayout.FloatField(geoPainter.myDistance, GUILayout.Width(100), GUILayout.MaxWidth(200));
                if (geoPainter.myDistance <= 0)
                    geoPainter.myDistance = 0;
                EditorGUILayout.EndHorizontal();

                //Spray Radius
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Spray Radius(S, SHIFT+S): ");
                geoPainter.mySpray = EditorGUILayout.FloatField(geoPainter.mySpray, GUILayout.Width(100), GUILayout.MaxWidth(200));
                if (geoPainter.mySpray <= 0)
                    geoPainter.mySpray = 0;
                EditorGUILayout.EndHorizontal();

                //Delete Radius
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Delete Radius(C, SHIFT+C): ");
                geoPainter.myDelete = EditorGUILayout.FloatField(geoPainter.myDelete, GUILayout.Width(100), GUILayout.MaxWidth(200));
                if (geoPainter.myDelete <= 0)
                    geoPainter.myDelete = 0;
                EditorGUILayout.EndHorizontal();

                geoPainter.useNormal = EditorGUILayout.Toggle("Use Normal ?", geoPainter.useNormal);
            }
            #endregion
            // RandomSection
            #region RandomSection
            EditorGUILayout.Space();
            showRandom = EditorGUILayout.Foldout(showRandom, "RANDOMIZE", EditorStyles.boldLabel);
            if (showRandom && !isPainting)
            {
                EditorGUILayout.Space();

                currentGroupScript.rndSeed = EditorGUILayout.IntSlider("Seed: ", currentGroupScript.rndSeed, 1, 12600);

                // [5/26/2015 kain0024] position
                EditorGUILayout.Space();
                GUILayout.Label("POSITION", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("", "", GUILayout.Width((3)));
                EditorGUILayout.LabelField("x", "", GUILayout.Width((3)));
                EditorGUILayout.LabelField("y", "", GUILayout.Width((3)));
                EditorGUILayout.LabelField("z", "", GUILayout.Width((3)));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("", "Offset", GUILayout.Width(200));
                currentGroupScript.offPosX = EditorGUILayout.FloatField("", currentGroupScript.offPosX, GUILayout.Width(200));
                currentGroupScript.offPosY = EditorGUILayout.FloatField("", currentGroupScript.offPosY, GUILayout.Width(200));
                currentGroupScript.offPosZ = EditorGUILayout.FloatField("", currentGroupScript.offPosZ, GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("", "Random Min", GUILayout.Width(200));
                currentGroupScript.rndPosMinX = EditorGUILayout.FloatField("", currentGroupScript.rndPosMinX, GUILayout.Width(200));
                currentGroupScript.rndPosMinY = EditorGUILayout.FloatField("", currentGroupScript.rndPosMinY, GUILayout.Width(200));
                currentGroupScript.rndPosMinZ = EditorGUILayout.FloatField("", currentGroupScript.rndPosMinZ, GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("", "Random Max", GUILayout.Width(200));
                currentGroupScript.rndPosMaxX = EditorGUILayout.FloatField("", currentGroupScript.rndPosMaxX, GUILayout.Width(200));
                currentGroupScript.rndPosMaxY = EditorGUILayout.FloatField("", currentGroupScript.rndPosMaxY, GUILayout.Width(200));
                currentGroupScript.rndPosMaxZ = EditorGUILayout.FloatField("", currentGroupScript.rndPosMaxZ, GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                // [5/26/2015 kain0024] rotation
                EditorGUILayout.Space();
                GUILayout.Label("ROTATION", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("", "", GUILayout.Width(3));
                EditorGUILayout.LabelField("x", "", GUILayout.Width(3));
                EditorGUILayout.LabelField("y", "", GUILayout.Width(3));
                EditorGUILayout.LabelField("z", "", GUILayout.Width(3));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("", "Offset", GUILayout.Width(200));
                currentGroupScript.offRotX = EditorGUILayout.FloatField("", currentGroupScript.offRotX, GUILayout.Width(200));
                currentGroupScript.offRotY = EditorGUILayout.FloatField("", currentGroupScript.offRotY, GUILayout.Width(200));
                currentGroupScript.offRotZ = EditorGUILayout.FloatField("", currentGroupScript.offRotZ, GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("", "Random Min", GUILayout.Width(200));
                currentGroupScript.rndRotMinX = EditorGUILayout.FloatField("", currentGroupScript.rndRotMinX, GUILayout.Width(200));
                currentGroupScript.rndRotMinY = EditorGUILayout.FloatField("", currentGroupScript.rndRotMinY, GUILayout.Width(200));
                currentGroupScript.rndRotMinZ = EditorGUILayout.FloatField("", currentGroupScript.rndRotMinZ, GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("", "Random Max", GUILayout.Width(200));
                currentGroupScript.rndRotMaxX = EditorGUILayout.FloatField("", currentGroupScript.rndRotMaxX, GUILayout.Width(200));
                currentGroupScript.rndRotMaxY = EditorGUILayout.FloatField("", currentGroupScript.rndRotMaxY, GUILayout.Width(200));
                currentGroupScript.rndRotMaxZ = EditorGUILayout.FloatField("", currentGroupScript.rndRotMaxZ, GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                //Scale

                EditorGUILayout.Space();
                GUILayout.Label("SCALE", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                currentGroupScript.scaleUniform = EditorGUILayout.Toggle("Uniform:", currentGroupScript.scaleUniform);

                if (!currentGroupScript.scaleUniform)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("", "", GUILayout.Width(3));
                    EditorGUILayout.LabelField("x:", "", GUILayout.Width(3));
                    EditorGUILayout.LabelField("y:", "", GUILayout.Width(3));
                    EditorGUILayout.LabelField("z:", "", GUILayout.Width(3));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("", "Offset", GUILayout.Width(200));
                    currentGroupScript.offSclX = EditorGUILayout.FloatField("", currentGroupScript.offSclX, GUILayout.Width(200));
                    currentGroupScript.offSclY = EditorGUILayout.FloatField("", currentGroupScript.offSclY, GUILayout.Width(200));
                    currentGroupScript.offSclZ = EditorGUILayout.FloatField("", currentGroupScript.offSclZ, GUILayout.Width(200));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("", "Random Min", GUILayout.Width(200));
                    currentGroupScript.rndSclMinX = EditorGUILayout.FloatField("", currentGroupScript.rndSclMinX, GUILayout.Width(200));
                    currentGroupScript.rndSclMinY = EditorGUILayout.FloatField("", currentGroupScript.rndSclMinY, GUILayout.Width(200));
                    currentGroupScript.rndSclMinZ = EditorGUILayout.FloatField("", currentGroupScript.rndSclMinZ, GUILayout.Width(200));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("", "Random Max", GUILayout.Width(200));
                    currentGroupScript.rndSclMaxX = EditorGUILayout.FloatField("", currentGroupScript.rndSclMaxX, GUILayout.Width(200));
                    currentGroupScript.rndSclMaxY = EditorGUILayout.FloatField("", currentGroupScript.rndSclMaxY, GUILayout.Width(200));
                    currentGroupScript.rndSclMaxZ = EditorGUILayout.FloatField("", currentGroupScript.rndSclMaxZ, GUILayout.Width(200));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", "", GUILayout.Width(30));
                    EditorGUILayout.LabelField("", "Offset", GUILayout.Width(200));
                    EditorGUILayout.LabelField("", "Random Min", GUILayout.Width(200));
                    EditorGUILayout.LabelField("", "Random Max", GUILayout.Width(200));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("All:", "", GUILayout.Width(30));
                    currentGroupScript.offSclX = EditorGUILayout.FloatField("", currentGroupScript.offSclX, GUILayout.Width(200));
                    currentGroupScript.rndSclMinX = EditorGUILayout.FloatField("", currentGroupScript.rndSclMinX, GUILayout.Width(200));
                    currentGroupScript.rndSclMaxX = EditorGUILayout.FloatField("", currentGroupScript.rndSclMaxX, GUILayout.Width(200));


                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                //AUTO
                EditorGUILayout.Space();
                GUILayout.Label("UPDATE", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                geoPainter.rndAuto = GUILayout.Toggle(geoPainter.rndAuto, "Auto Update", "button");
                if (GUILayout.Button("Randomize", GUILayout.Height(30)))
                {
                    randomize();
                }

                if (GUI.changed && geoPainter.rndAuto)
                {
                    randomize();
                    GUI.changed = false;
                }

                myCopyFromPop = new List<string>();
                for (int i = 0; i < myGroups.Count; i++)
                {
                    myCopyFromPop.Add(myGroups[i].name);
                }

                EditorGUILayout.Space();
                GUILayout.Label("COPY & PASTE FROM GROUP", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                // myGroups.ToArray() as GameObject[];
                copyFromIndex = EditorGUILayout.Popup(copyFromIndex, myCopyFromPop.ToArray() as string[]);
                if (GUILayout.Button("COPY", GUILayout.Width(100)))
                    copySettings();

                EditorGUILayout.EndHorizontal();

            }
            #endregion
        }
    }

    //--------------------------------------------------------------------------------------------------------
    void copySettings()
    {
        GeoPainterGroup myTempScript = myGroups[copyFromIndex].GetComponent<GeoPainterGroup>();
        if (myTempScript == null)
            return;

        currentGroupScript.rndSeed = myTempScript.rndSeed;
        currentGroupScript.offPosX = myTempScript.offPosX;
        currentGroupScript.offPosY = myTempScript.offPosY;
        currentGroupScript.offPosZ = myTempScript.offPosZ;
        currentGroupScript.rndPosMinX = myTempScript.rndPosMinX;
        currentGroupScript.rndPosMinY = myTempScript.rndPosMinY;
        currentGroupScript.rndPosMinZ = myTempScript.rndPosMinZ;
        currentGroupScript.rndPosMaxX = myTempScript.rndPosMaxX;
        currentGroupScript.rndPosMaxY = myTempScript.rndPosMaxY;
        currentGroupScript.rndPosMaxZ = myTempScript.rndPosMaxZ;
        currentGroupScript.offRotX = myTempScript.offRotX;
        currentGroupScript.offRotY = myTempScript.offRotY;
        currentGroupScript.offRotZ = myTempScript.offRotZ;
        currentGroupScript.rndRotMinX = myTempScript.rndRotMinX;
        currentGroupScript.rndRotMinY = myTempScript.rndRotMinY;
        currentGroupScript.rndRotMinZ = myTempScript.rndRotMinZ;
        currentGroupScript.rndRotMaxX = myTempScript.rndRotMaxX;
        currentGroupScript.rndRotMaxY = myTempScript.rndRotMaxY;
        currentGroupScript.rndRotMaxZ = myTempScript.rndRotMaxZ;
        currentGroupScript.scaleUniform = myTempScript.scaleUniform;
        currentGroupScript.offSclX = myTempScript.offSclX;
        currentGroupScript.offSclY = myTempScript.offSclY;
        currentGroupScript.offSclZ = myTempScript.offSclZ;
        currentGroupScript.rndSclMinX = myTempScript.rndSclMinX;
        currentGroupScript.rndSclMinY = myTempScript.rndSclMinY;
        currentGroupScript.rndSclMinZ = myTempScript.rndSclMinZ;
        currentGroupScript.rndSclMaxX = myTempScript.rndSclMaxX;
        currentGroupScript.rndSclMaxY = myTempScript.rndSclMaxY;
        currentGroupScript.rndSclMaxZ = myTempScript.rndSclMaxZ;
    }

    //--------------------------------------------------------------------------------------------------------
    void addGroup()
    {        
        if(geoPainter == null)
            return;

        GameObject go = new GameObject("GeoPainter_Group_" + geoPainter.nbrGroupsCreated.ToString());
		geoPainter.nbrGroupsCreated = geoPainter.nbrGroupsCreated + 1;
		go.AddComponent<GeoPainterGroup>();
		myGroups.Add(go);
        geoPainter.groupSelIndex = myGroups.Count;
        currentGroup = myGroups[geoPainter.groupSelIndex - 1];
		currentGroupScript = currentGroup.GetComponent<GeoPainterGroup>();

        if (currentGroupScript.myLibraryBuiltIn != null)
            myLibrary = new List<GameObject>(currentGroupScript.myLibraryBuiltIn);
        else
            myLibrary = new List<GameObject>();

        if (currentGroupScript.selectObject != null)
            selectObject = new List<PainterObjectInfo>(currentGroupScript.selectObject);
        else
            selectObject = new List<PainterObjectInfo>();

        // [6/3/2015 kain0024] 머트리얼 리스트 생성, 생성 될 때 가지는 머트리얼 인덱스는 0번
        if (selectGroups == null)
            selectGroups = new List<PainterObjectGroupInfo>();

        PainterObjectGroupInfo groupInfo = new PainterObjectGroupInfo();
        groupInfo.Init();
        selectGroups.Add(groupInfo);       
        
    }

    //--------------------------------------------------------------------------------------------------------
    void removeGroup(int _index, bool _release)
    {        
        var index = (_index - 1);

        // [5/26/2015 kain0024] remove = false, release = ture
        if (_release == false)
        {
            var go = myGroups[index];
            DestroyImmediate(go);
        }

        //GeoPainter geoPainter = target as GeoPainter;
        if (geoPainter == null)
            return;

        myGroups.RemoveAt(index);
        geoPainter.groupSelIndex = myGroups.Count;
        if (myGroups.Count != 0)
        {
            currentGroup = myGroups[geoPainter.groupSelIndex - 1];
            currentGroupScript = currentGroup.GetComponent<GeoPainterGroup>();
            
            if (currentGroupScript.myLibraryBuiltIn != null)
                myLibrary = new List<GameObject>(currentGroupScript.myLibraryBuiltIn);
            else
                myLibrary = new List<GameObject>();

            // [6/3/2015 kain0024] 오브젝트 정보도 갱신
            if (currentGroupScript.selectObject != null)
                selectObject = new List<PainterObjectInfo>(currentGroupScript.selectObject);
            else
                selectObject = new List<PainterObjectInfo>();
        }    

    }

    //--------------------------------------------------------------------------------------------------------
    void updatePrefab()
    {       
        for (int i = 0; i < currentGroupScript.myPointsList.Count; ++i)
        {
            GeoPainterPoint element = currentGroupScript.myPointsList[i];
            PrefabUtility.ReconnectToLastPrefab(element.go);
            PrefabUtility.ResetToPrefabState(element.go);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    void randomize()
    {
        Random.seed = currentGroupScript.rndSeed;
        for (int i = 0; i < currentGroupScript.myPointsList.Count; ++i)
        {
            GeoPainterPoint element = currentGroupScript.myPointsList[i];
            randomizeSolo(element);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    void randomizeSolo(GeoPainterPoint element)
	{
        Transform obj = element.go.transform;
		
		var myRot = Quaternion.identity;
		if(element.useNormal)
			myRot = Quaternion.FromToRotation(obj.up, element.normal) * obj.rotation;

        obj.position = element.pos;
		obj.rotation = myRot;
		obj.localScale = element.scale;

		//Position        
		float tmpPosX = currentGroupScript.offPosX + Random.Range(currentGroupScript.rndPosMinX, currentGroupScript.rndPosMaxX);
		float tmpPosY = currentGroupScript.offPosY + Random.Range(currentGroupScript.rndPosMinY, currentGroupScript.rndPosMaxY);
        float tmpPosZ = currentGroupScript.offPosZ + Random.Range(currentGroupScript.rndPosMinZ, currentGroupScript.rndPosMaxZ);
		obj.Translate(tmpPosX, tmpPosY, tmpPosZ);
		
		//Rotation
        float tmpRotX = currentGroupScript.offRotX + Random.Range(currentGroupScript.rndRotMinX, currentGroupScript.rndRotMaxX);
        float tmpRotY = currentGroupScript.offRotY + Random.Range(currentGroupScript.rndRotMinY, currentGroupScript.rndRotMaxY);
        float tmpRotZ = currentGroupScript.offRotZ + Random.Range(currentGroupScript.rndRotMinZ, currentGroupScript.rndRotMaxZ);
		obj.Rotate(tmpRotX, tmpRotY, tmpRotZ);
		
		//Scale

        float tmpSclX = currentGroupScript.offSclX + Random.Range(currentGroupScript.rndSclMinX, currentGroupScript.rndSclMaxX);
        float tmpSclY = currentGroupScript.offSclY + Random.Range(currentGroupScript.rndSclMinY, currentGroupScript.rndSclMaxY);
        float tmpSclZ = currentGroupScript.offSclZ + Random.Range(currentGroupScript.rndSclMinZ, currentGroupScript.rndSclMaxZ);
		if(!currentGroupScript.scaleUniform)
			obj.localScale += new Vector3(tmpSclX, tmpSclY, tmpSclZ);
		else
			obj.localScale += new Vector3(tmpSclX, tmpSclX, tmpSclX);
    }

    //--------------------------------------------------------------------------------------------------------
    void replacePrefab()
    {
        for (int i = 0; i < currentGroupScript.myPointsList.Count; ++i)
        {
            GeoPainterPoint element = currentGroupScript.myPointsList[i];
			DestroyImmediate(element.go);
			
			var myRandom = Random.Range(0, currentGroupScript.myLibraryBuiltIn.Length);
			var objToInst = currentGroupScript.myLibraryBuiltIn[myRandom];
			//var myNewObject = EditorUtility.InstantiatePrefab(objToInst);
			GameObject myNewObject = PrefabUtility.InstantiatePrefab(objToInst) as GameObject;
			myNewObject.transform.position = element.pos;
			myNewObject.transform.rotation = Quaternion.identity;
			element.scale = myNewObject.transform.localScale;
			element.go = myNewObject;
			randomizeSolo( element);
			if(currentGroup.transform.childCount == 0)
			{
				currentGroup.transform.position = myNewObject.transform.position;
				myNewObject.transform.parent = currentGroup.transform;
			}
            else
				myNewObject.transform.parent = currentGroup.transform;

		}
    }

    void drawHandles()
    {
        RaycastHit hit;
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        //GeoPainter geoPainter = target as GeoPainter;
        if (geoPainter == null)
            return;

        //int layerMask = 1 << geoPainter.paintLayer;
        int layerMask = geoPainter.paintLayer;

        if(Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
        {
            if(geoPainter.mySpray != 0)
            {
                Handles.color = Color.green;
                Handles.CircleCap(1, hit.point, Quaternion.LookRotation(hit.normal), geoPainter.mySpray);
            }

            if(geoPainter.myDistance != 0)
            {
                Handles.color = Color.blue;
                Handles.CircleCap(0, hit.point, Quaternion.LookRotation(hit.normal), geoPainter.myDistance);
            }

            if(geoPainter.myDelete != 0)
            {
                Handles.color = Color.red;
                Handles.CircleCap(0, hit.point, Quaternion.LookRotation(hit.normal), geoPainter.myDelete);
            }
        }
    }

    //--------------------------------------------------------------------------------------------------------
    void OnSceneGUI()
    {
        int ctrlID = GUIUtility.GetControlID(appTitle.GetHashCode(), FocusType.Passive);
        if(isPainting)
        {
            var e = Event.current;
            drawHandles();

            //GeoPainter geoPainter = target as GeoPainter;
            if (geoPainter == null)
                return;

            if (e.keyCode == KeyCode.D && !e.shift)
            {
                geoPainter.myDistance += 0.01f;
                Repaint();
                HandleUtility.Repaint();
            }
            if (e.keyCode == KeyCode.D && e.shift)
            {
                geoPainter.myDistance -= 0.01f;
                if (geoPainter.myDistance <= 0)
                    geoPainter.myDistance = 0;

                Repaint();
                HandleUtility.Repaint();
            }
            if (e.keyCode == KeyCode.S && !e.shift)
            {
                geoPainter.mySpray += 0.01f;
                Repaint();
                HandleUtility.Repaint();
            }
            if (e.keyCode == KeyCode.S && e.shift)
            {
                geoPainter.mySpray -= 0.01f;
                if (geoPainter.mySpray <= 0)
                    geoPainter.mySpray = 0;

                Repaint();
                HandleUtility.Repaint();
            }
            if (e.keyCode == KeyCode.C && !e.shift)
            {
                geoPainter.myDelete += 0.01f;
                Repaint();
                HandleUtility.Repaint();
            }
            if (e.keyCode == KeyCode.C && e.shift)
            {
                geoPainter.myDelete -= 0.01f;
                if (geoPainter.myDelete <= 0)
                    geoPainter.myDelete = 0;

                Repaint();
                HandleUtility.Repaint();
            }

            //Mouse Event
            switch (e.type)
            {
                case EventType.mouseDrag:
                    if (e.button == 0)  // [6/2/2015 kain0024] 마우스 L버튼을 눌렀을 때만 그리기 가능
                    {
                        if (e.control)
                        {
                            paint();
                            e.Use();
                        }
                        else if (e.shift)
                        {
                            paintRemove();
                            e.Use();
                        }
                    }
                    break;
                case EventType.mouseDown :
                    if(e.button == 1)       // 마우스 오른쪽 버튼을 눌렀을 경우
                    {
                        // 선택된 지형에 오브젝트 채우기

                        e.Use();
                    }
                    break;
                case EventType.mouseUp:
                    if (e.button == 0)  // [6/2/2015 kain0024] 마우스 L버튼을 눌렀을 때만 그리기 가능
                    {
                        if (e.control)
                        {
                            paint();
                            //	Undo.RegisterUndo(myObjToInstArray.ToBuiltin(GameObject),"New Paint Object");
                            //myObjToInstArray = new Array();
                            // [6/4/2015 kain0024] 마우스 버튼을 때면 리스트 초기화!!!
                            myObjToInstArray = new List<GameObject>();
                            e.Use();
                        }
                        else if (e.shift)
                        {
                            paintRemove();
                            e.Use();
                        }
                    }
                    break;
                case EventType.layout:
                    HandleUtility.AddDefaultControl(ctrlID);
                    break;
                case EventType.MouseMove:
                    HandleUtility.Repaint();
                    break;
                //if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
                
            }
        }
    }

    //--------------------------------------------------------------------------------------------------------
    void paint()
    {
        if(currentGroupScript.myLibraryBuiltIn.Length == 0 || currentGroupScript.myLibraryBuiltIn[0] == null)
            return;

        //GeoPainter geoPainter = target as GeoPainter;
        if (geoPainter == null)
            return;

		RaycastHit hit;
		var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        //var layerMask = 1 << geoPainter.paintLayer;
        var layerMask = geoPainter.paintLayer;
		if (Physics.Raycast (ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
		{			
            GameObject objToInst = null;
			//Spray
            if (geoPainter.mySpray > 0)
			{
                var randomCircle = Random.insideUnitCircle * geoPainter.mySpray;
				var rayDirection = (hit.point + new Vector3(randomCircle.x, 0, randomCircle.y))  - ray.origin;
				RaycastHit newHit;
				if (Physics.Raycast (ray.origin, rayDirection, out newHit, Mathf.Infinity, layerMask))
					hit = newHit;

			}
			
			//Check Distance
            var dist = Mathf.Infinity;
			if(currentGroup.transform.childCount != 0)
            {
                for (int i = 0; i < myObjToInstArray.Count; ++i)
                {
                    GameObject obj = myObjToInstArray[i];
                    var tempDist = Vector3.Distance(hit.point, obj.transform.position);
                    if (tempDist < dist)
                        dist = tempDist;
                }
			}

            if (dist >= geoPainter.myDistance)
			{
				//Biblio Method
                if (geoPainter.bibSortIndex == 0)
				{
					//Random
					var myRandom = Random.Range(0, currentGroupScript.myLibraryBuiltIn.Length);
                    objToInst = currentGroupScript.myLibraryBuiltIn[myRandom];
				}

                if (geoPainter.bibSortIndex == 1)
                    objToInst = currentGroupScript.myLibraryBuiltIn[geoPainter.bibSoloSelect];
				
				//Check is we're using normal placement
                Quaternion myRot = Quaternion.identity;
                if (geoPainter.useNormal)
					myRot = Quaternion.FromToRotation(objToInst.transform.up, hit.normal) * objToInst.transform.rotation;

				//Create the Object
                GameObject newObj = PrefabUtility.InstantiatePrefab(objToInst) as GameObject;
				newObj.transform.position = hit.point;
				newObj.transform.rotation = myRot;
				myObjToInstArray.Add(newObj);
				
				//Update Points Array
                currentGroupScript.addObject(newObj.gameObject, hit.point, newObj.transform.localScale, hit.normal, geoPainter.useNormal);
				
				//Update Position Pivot
				if(currentGroup.transform.childCount == 0)
				{
					currentGroup.transform.position = newObj.transform.position;
					newObj.transform.parent = currentGroup.transform;
				}
                else
					newObj.transform.parent = currentGroup.transform;

                GeoPainterPoint obj = currentGroupScript.myPointsList[currentGroupScript.myPointsList.Count - 1];
                randomizeSolo( obj);			
				
			}
		}
    }

    //--------------------------------------------------------------------------------------------------------
    void paintRemove()
    {
		RaycastHit hit;
		var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (geoPainter == null)
            return;
		
		if (Physics.Raycast (ray.origin, ray.direction, out hit, Mathf.Infinity))
		{
			for(int i=0; i < currentGroupScript.myPointsList.Count; i++)
			{
				var element = currentGroupScript.myPointsList[i];
                if (Vector3.Distance(hit.point, element.go.transform.position) <= geoPainter.myDelete)
				{
					DestroyImmediate(element.go);
					currentGroupScript.myPointsList.RemoveAt(i);
				}
			}
		}
    }

    string[] LayerNames()
    {
        string[] layerNames = new string[32];
        for( int i = 0; i < 32; ++i)
        {
            string layerName = LayerMask.LayerToName(i);
            if(layerName == "")
                layerName = "Layer " + i;

            layerNames[i] = layerName;
        }

        return layerNames;
    }

    //--------------------------------------------------------------------------------------------------------
    //********************************************************************************************
	//*** Menu Item
	//********************************************************************************************
	[MenuItem("GSwing Painter/Add GeoPainter")]
	static void CreateMenu()
    {
		// Get existing open window or if none, make a new one:
		if(!GameObject.Find("GeoPainterSys"))
		{
			GameObject go = new GameObject ("GeoPainterSys");
			go.AddComponent<GeoPainter>();
			Selection.activeGameObject = go;
			
			
		}
	}
   
    //--------------------------------------------------------------------------------------------------------
}

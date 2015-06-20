using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using PainterObjectRegisterInfo;

public class RandomPlacer : MonoBehaviour
{
    public GameObject prefab;

    private float rangeX;
    private float rangeZ;
    private Vector3 randomPos;

    private float randomX;
    private float randomZ;
    private float groundLevel;

    public int instanceNum;
    private Vector3 randomScale;
    public float scaleMin;
    public float scaleMax;

    // [6/15/2015 kain0024] 주변 충돌 처리 체크
    public bool intersectObject = true;

    // [6/15/2015 kain0024] 특정 머트리얼에만 배치
    public int selectMaterialID = -1;

    static GameObject tempParent;

    private List<Transform> prefabInstances = new List<Transform>();

    // [6/15/2015 kain0024] geopainter에서 넘어온 오브젝트 리스트
    private List<GameObject> registerObject;
    private List<PainterObjectInfo> registerObjInfo;

    //--------------------------------------------------------------------------------------------------------
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    [ContextMenu("Place")]
    public void Place()
    {        
        rangeX = transform.localScale.x * 0.5f;
        rangeZ = transform.localScale.z * 0.5f;
       
        tempParent = new GameObject();

        for (int i = 0; i < instanceNum; i++)
        {
            randomX = Random.Range(rangeX * -1f, rangeX);
            randomZ = Random.Range(rangeZ * -1f, rangeZ);

            //GameObject prefabInstance = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation) as GameObject;
            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            randomScale = new Vector3(Random.Range(scaleMin, scaleMax),
                                      Random.Range(scaleMin, scaleMax),
                                      Random.Range(scaleMin, scaleMax));

            prefabInstance.transform.localScale = randomScale;

            groundLevel = transform.position.y;
            randomPos = new Vector3(randomX + transform.position.x, groundLevel, randomZ + transform.position.z);
            prefabInstance.transform.localPosition = randomPos;

            prefabInstance.transform.parent = tempParent.transform;
        }

    }

    [ContextMenu("Remove")]
    public void Remove()
    {
        /*
        foreach (Transform instance in tempParent.transform)
        {
            prefabInstances.Add(instance);
        }

        foreach (Transform prefabInstance in prefabInstances)
        {
            if (prefabInstance != null)
                DestroyImmediate(prefabInstance.gameObject);
        }

        prefabInstances.Clear();
         */
        DestroyImmediate(tempParent);
    }

    [ContextMenu("AttachObject")]
    public void AttachObject()
    {
        if (tempParent == null)
            return;

        RaycastHit hit;
        GameObject AttachRoot = new GameObject();
        GameObject parentObject = null;

        AttachRoot.name = "Root_" + prefab.name;
        for (int i = 0; i < tempParent.transform.childCount; ++i)
        {
            Transform trans = tempParent.transform.GetChild(i);

            // [6/1/2015 kain0024] 만약 지형과 충돌 하지 않는다면 continue
            if (!Physics.Raycast(trans.position, -trans.forward, out hit, Mathf.Infinity))
                continue;

            // [6/1/2015 kain0024] materialid가 -1이 아니면 머트리얼 판별 작업을 한다.
            if (selectMaterialID != -1)
            {
                if(checkMaterial(hit) != selectMaterialID)
                    continue;
            }
            
            if(intersectObject == true)
            {
                // [6/1/2015 kain0024] 만약 선택된 위치에 누군가 있다면 패스
                bool intersect = false;
                for (int j = 0; j < prefabInstances.Count; ++j)
                {
                    Transform objTrans = prefabInstances[j];
                    float dist = Vector3.Distance(objTrans.position, hit.point);
                    Mesh mesh = objTrans.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    // [6/1/2015 kain0024] 메쉬 반경에 scale를 곱해서 계산 한다.
                    if (dist < mesh.bounds.size.magnitude * objTrans.localScale.x)
                    {
                        intersect = true;
                        break;
                    }
                }

                if (intersect == true)
                    continue;
            }

            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            prefabInstance.transform.localScale = prefab.transform.localScale;
            prefabInstance.transform.localPosition = hit.point;
            prefabInstance.transform.parent = AttachRoot.transform;

            prefabInstances.Add(prefabInstance.transform);

            // [6/15/2015 kain0024] 충돌체크 후 부모 오브젝트를 저장한다.
            if (parentObject == null)
                parentObject = hit.collider.gameObject;
        }

        AttachRoot.transform.parent = parentObject.transform;
        prefabInstances.Clear();
        
        if(tempParent != null)
            DestroyImmediate(tempParent);
    }

    int checkMaterial(RaycastHit hit)
    {
        MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (renderer == null || renderer.sharedMaterial == null || meshCollider == null)
            return -1;

        Mesh mesh = hit.collider.gameObject.GetComponent<MeshFilter>().sharedMesh;
        int[] hittedTriangle = new int[] 
        {
            mesh.triangles[hit.triangleIndex * 3], 
            mesh.triangles[hit.triangleIndex * 3 + 1], 
            mesh.triangles[hit.triangleIndex * 3 + 2] 
        };

        for (int i = 0; i < mesh.subMeshCount; ++i)
        {
            int[] tr = mesh.GetTriangles(i);
            for(int j = 0; j < tr.Length; j += 3)
            {
                if (tr[j] == hittedTriangle[0] && tr[j + 1] == hittedTriangle[1] && tr[j + 2] == hittedTriangle[2])
                    return i;
            }
        }

        return -1;
    }

    //--------------------------------------------------------------------------------------------------------
    // [6/15/2015 kain0024] 랜덤 배치할 오브젝트의 바운딩 박스를 조사 후 스케일 조절 한다.
    [ContextMenu("checkObjectSize")]
    void checkObjectSize()
    {
        GameObject obj = GameObject.Find("sceneTest") as GameObject;
        if(obj != null)
        {
            Mesh mesh = obj.gameObject.GetComponent<MeshFilter>().sharedMesh;
            Debug.Log("mesh.bounds.min : " + mesh.bounds.min);
            Debug.Log("mesh.bounds.max : " + mesh.bounds.max);
            Vector3 boundBoxSize = new Vector3(Mathf.Abs(mesh.bounds.min.x) + Mathf.Abs(mesh.bounds.max.x), 1, Mathf.Abs(mesh.bounds.min.y) + Mathf.Abs(mesh.bounds.max.y) );
            transform.localScale = boundBoxSize;
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public void SetBatchInfo(int objInstanceCount, GameObject[] _goList, PainterObjectInfo[] _objInfo)
    {
        instanceNum = objInstanceCount;
        registerObject = new List<GameObject>(_goList);
        registerObjInfo = new List<PainterObjectInfo>(_objInfo);

        Place();
        AttachObject();
    }

}

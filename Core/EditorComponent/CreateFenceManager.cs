using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

[System.Serializable]
public struct FenceData
{
    public GameObject fencePrefab;
    public GameObject _dummyPrefab;
}

//***********************************************************************************************
// [5/29/2015 kain0024] 만약 여기에 변수를 추가 하거나 삭제 할 경우(인스펙터의 내용이 변경이 될 경우)
// 반드시 FenceInspectorEditor.cs에 업데이트를 해 줘야 한다.
//***********************************************************************************************
public class CreateFenceManager : MonoBehaviour {
        
    public bool ViewSize = true;
    public List<FenceData> FenceList = new List<FenceData>();

    [ContextMenu("CreateFence")]
    public void Start () {
        StartCoroutine(CreateFence());

        Debug.Log("Version : " + Environment.Version.ToString());
	}

    public IEnumerator CreateFence()
    {
        // [5/21/2015 kain0024] 리스트에 아무것도 없으면 리턴
        if (FenceList.Count == 0)
            yield break;

        FenceData[] fenceArray = FenceList.ToArray();

        for (int k = 0; k < fenceArray.Length; ++k)
        {
            FenceData Fence = fenceArray[k];

            // [5/21/2015 kain0024] 리스트에 할당은 했지만 정보가 없다면 리턴..
            if (Fence.fencePrefab == null || Fence._dummyPrefab == null)
                yield break;

            // [5/22/2015 kain0024] 그래픽팀 요청으로 fence 위치를 이름으로 찾지 않고 더미 오브젝트를 받아서 넣기로 변경
            if (Fence._dummyPrefab != null)
            {
                Transform Rootdummy = Fence._dummyPrefab.GetComponent<Transform>();

                // [5/29/2015 kain0024] 더미에 자식이 없다면 리턴
                if (Rootdummy.childCount == 0)
                    yield break;

                // [5/29/2015 kain0024] 새로운 오브젝트를 만들고 붙여 넣지 않으면 너무 지저분해 진다. 빈 오브젝트 하나 생성해서 prefab를 넣어 준다.
                GameObject subjectDummy = new GameObject();
                subjectDummy.name = Fence._dummyPrefab.name;
                subjectDummy.transform.parent = transform;

                // [5/21/2015 kain0024] 더미 리스트에서 읽어온 좌표와 회전 값을 넣어서 오브젝트를 생성 한다.
                for (int i = 0; i < Rootdummy.childCount; ++i)
                {
                    Transform dummy = Rootdummy.GetChild(i);
                    //GameObject prefabInstance = Instantiate(Fence.fencePrefab, dummy.position, dummy.rotation) as GameObject;
                    GameObject prefabInstance = PrefabUtility.InstantiatePrefab(Fence.fencePrefab) as GameObject;
                    prefabInstance.transform.localPosition = dummy.position;
                    prefabInstance.transform.localRotation = dummy.rotation;
                    prefabInstance.transform.parent = subjectDummy.transform;                  
                }

                yield return null;
            }
        }       
    }    
}

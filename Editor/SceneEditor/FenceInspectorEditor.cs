using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// [5/22/2015 kain0024] 사용자 정의 리스트 만들기
// 체크 박스를 만들어서 체크 박스에 표시 했을 때만 list의 사이즈를 수정 할 수 있게 변경
[CustomEditor(typeof(CreateFenceManager) )]
public class FenceInspectorEditor : Editor {

	public override void OnInspectorGUI()
    {
        CreateFenceManager fenceScript = target as CreateFenceManager;
        serializedObject.Update();
        // [5/22/2015 kain0024] ViewSize flag 인스펙터에 노출
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ViewSize"));
        // [5/22/2015 kain0024] viewsize flag를 인자로 넘기면서 list 인스펙터에 노출
        EditorList.Show(serializedObject.FindProperty("FenceList"), fenceScript.ViewSize);        
        // [5/22/2015 kain0024] 변경된 프로퍼티 적용
        serializedObject.ApplyModifiedProperties();        
    }    
}

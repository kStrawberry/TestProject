using UnityEngine;
using UnityEditor;

public static class EditorList {
    
	public static void Show (SerializedProperty list, bool updateFlag)
    {
        // [5/22/2015 kain0024] 프로퍼티 리스트를 받고
        EditorGUILayout.PropertyField(list);        
        EditorGUI.indentLevel += 1;
        //만약 확장을 했으면 보여준다.
        if (list.isExpanded)
        {
            // [5/22/2015 kain0024] 사이즈 업데이트 플레그가 체크가 되면 list의 size를 표시해 준다.
            if(updateFlag)
                EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));

            for (int i = 0; i < list.arraySize; ++i)
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), true, GUILayout.Width(300), GUILayout.MinWidth(100));
        }
        EditorGUI.indentLevel -= 1;
    }
}

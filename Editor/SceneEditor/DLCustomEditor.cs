using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TimeOfDay))]
[CanEditMultipleObjects]
public class DLCustomEditor : Editor
{
    SerializedProperty TimeProperty;
    SerializedProperty SunriseTimeProperty;
    SerializedProperty SunsetTimeProperty;

    public void OnEnable()
    {
        TimeProperty = serializedObject.FindProperty("m_Time");
        SunriseTimeProperty = serializedObject.FindProperty("m_SunriseTime");
        SunsetTimeProperty = serializedObject.FindProperty("m_SunsetTime");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TimeProperty.floatValue = EditorGUILayout.FloatField("Time", TimeProperty.floatValue);
        SunriseTimeProperty.floatValue = EditorGUILayout.FloatField("SunriseTime", SunriseTimeProperty.floatValue);
        SunsetTimeProperty.floatValue = EditorGUILayout.FloatField("SunsetTime", SunsetTimeProperty.floatValue);
        serializedObject.ApplyModifiedProperties();

        (serializedObject.targetObject as TimeOfDay).CalculationTime();
    }



}

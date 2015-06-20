
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class GSTerrainMaterialInspector : ShaderGUI
{
    /*
    protected override void CreateToggleList()
    {
        Toggles.Add(new FeatureToggle("Rim Light Sec Enabled", "RimSecColor", "RIMLIGHTSEC_ON", "RIMLIGHTSEC_OFF"));
        Toggles.Add(new FeatureToggle("Rim Light Enabled", "rim", "RIMLIGHT_ON", "RIMLIGHT_OFF"));
        Toggles.Add(new FeatureToggle("Light Enabled", "dfsdff", "LIGHTMAP_ON", "LIGHTMAP_OFF"));
    }
    */
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        if (!materialEditor.isVisible)
            return;

        Material targetMat = materialEditor.target as Material;
        string[] keyWords = targetMat.shaderKeywords;

        bool RimLight = keyWords.Contains("RIMLIGHT_ON");
        bool RimLightSec = keyWords.Contains("RIMLIGHTSEC_ON");

        EditorGUI.BeginChangeCheck();
        RimLight = EditorGUILayout.Toggle("Rim Light Enabled", RimLight);
        RimLightSec = EditorGUILayout.Toggle("Rim Light Sec Enabled", RimLightSec);
        if (EditorGUI.EndChangeCheck())
        {
            List<string> keywords = new List<string> {  RimLightSec ? "RIMLIGHTSEC_ON" : "RIMLIGHTSEC_OFF", RimLight ? "RIMLIGHT_ON" : "RIMLIGHT_OFF"};
            targetMat.shaderKeywords = keywords.ToArray();
            EditorUtility.SetDirty(targetMat);

           // EditorUtility.SetDirty(targetMat);

        }
    }
    
}

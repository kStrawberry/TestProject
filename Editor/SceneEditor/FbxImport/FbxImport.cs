using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class FbxImport : AssetPostprocessor
{   
    public void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        String name = importer.assetPath.ToLower();
        if(name.Substring( (name.Length - 4), 4) == ".fbx")
        {
            importer.globalScale = 1.0f;
            
        }
    }

    public void OnPostprocessGameObjectWithUserProperties(GameObject go, String[] propNames, System.Object[] values)
    {
#if UNITY_EDITOR
        Debug.Log("------------OnPostprocessGameObjectWithUserProperties for " + go.name);
        Debug.Log("------------values.Length " + values.Length);
        Debug.Log("------------propNames.Length " + propNames.LongLength);
#endif
        //for (var i = 0; i < propNames.Length; i++)
        // [2015/04/15/kain0024] 보통 propnames.length를 사용하는 0번째에 userdata가 모두 들어 온다.
        for (var i = 0; i < 1; i++)
        {
			var value = values[i];
#if UNITY_EDITOR
            var propName = propNames[i];
            Debug.Log("Propname: " + propName + " value: " + value);
#endif

            if (value is string)
            {					
                Debug.Log("string: " + (string)value);
                UserData Prop = go.AddComponent<UserData>();
                Prop.InitPropList();
                string userdata = (string)value;
                
                char[] delimiterchars = { '\t', '\n', '\r' };
                // 위의 문자들을 문자열에서 걸러 낸다.
                string[] words = userdata.Split(delimiterchars);
                for(int j = 0; j < words.Length; ++j)
                {
                    if(words[j] != "")
                        Prop.SetUserData(words[j]);
                }
            }
				
            if(value is Vector4)
            {
                Debug.Log("string: " + (Vector4)value);
				// do something useful.
			}
				
            if (value is Color)
            {
                Debug.Log("string: " + (Color)value);
				// do something useful.
			}
				
			if (value is int)
            {
                Debug.Log("string: " + (int)value);
				// do something useful.
			}
				
			if (value is float)
            {
                Debug.Log("string: " + (float)value);
				// do something useful.
			}
        }
    }

    /*
    public Material OnAssignMaterialModel(Material material, Renderer renderer)
    {
#if UNITY_EDITOR
        Debug.Log(" material name: " + material.name);
#endif
        var materialPath = "Assets/Resources/Materials" + material.name + ".mat";
        var main = AssetDatabase.GetAssetPath(material.mainTexture);

        if (AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)))
            return (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));

        // create a new material asset using the shader
        // 여기서 원하는 쉐이더 이름 넣어 줘야 할 듯..
        material.shader = Shader.Find("Specular");
        AssetDatabase.CreateAsset(material, "Assets/" + material.name + ".mat");      
        return material;

        //TextureImporter textureImporter = assetImporter as TextureImporter;       
    }
    */
	
}

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GSTerrain : MonoBehaviour {

    protected TimeOfDay timeOfDay = null;
    protected OutdoorLightScattering OLS = null;
    private static GSTerrain m_singleton = null;

    public static bool IsReady()
    {
        return (null != m_singleton);
    }

    void Awake()
    {
        m_singleton = this;
    }

	// Use this for initialization
	void Start () {
        Light[] WorldLight = Light.GetLights(LightType.Directional, 0);
        //Vector3 lightPosition = WorldLight[0].gameObject.transform.position;
        timeOfDay = (TimeOfDay)WorldLight[0].gameObject.GetComponent(typeof(TimeOfDay));
        OLS = (OutdoorLightScattering)WorldLight[0].gameObject.GetComponent(typeof(OutdoorLightScattering));
	}
	
	// Update is called once per frame
	void Update () {
        //Light[] WorldLight = Light.GetLights(LightType.Directional, 0);
        //Vector3 lightPosition = WorldLight[0].gameObject.transform.position;

        Material[] curMaterials = gameObject.GetComponent<Renderer>().materials;

        foreach (Material curMaterial in curMaterials )
        {
            curMaterial.SetTexture("_PrecomputeNetDensityToAtmTop", OLS.PrecomputeNetDensityToAtmTop);
            curMaterial.SetTexture("_AmbientSkyLight", OLS.AmbientSkyLight);
            curMaterial.SetFloat("g_f1EarthRadius", OLS.EarthRadius);
            curMaterial.SetFloat("g_f1AtmTopHeight", OLS.AtmTopHeight);
            curMaterial.SetFloat("g_f1DirectLightScale", OLS.DirectLightScale);
            curMaterial.SetFloat("g_f1Saturation", OLS.Saturation);
            curMaterial.SetVector("g_f4MieExtinctionCoeff", OLS.MieExtinctionCoeff);
            curMaterial.SetVector("g_f4RayleighExtinctionCoeff", OLS.RayleighExtinctionCoeff);
            curMaterial.SetVector("g_f3LightDirection", new Vector4(timeOfDay.LightDirection.x, timeOfDay.LightDirection.y, timeOfDay.LightDirection.z, 1.0f));
        }
	}
}

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class OutdoorLightScattering : MonoBehaviour {

    public Shader curShader;
    private Material curMaterial;
    public float ScatteringScale = 10.0f;
    public float DirectLightScale = 1.0f;
    public float Saturation = 0.5f;
    protected TimeOfDay timeOfDay = null;

    private Texture2D m_tex2DRandomsphereSampling = null;
    private RenderTexture m_rtPrecomputeNetDensityToAtmTop = null;
    private Texture3D m_tex3DSingleScaterring = null;
    private RenderTexture m_rt2DAmbientSkyLight = null;

    protected Vector4 m_AngularRayleighSctrCoeff = new Vector4();
    protected Vector4 m_AngularMieSctrCoeff = new Vector4();
    protected Vector4 m_TotalRayleighSctrCoeff = new Vector4();
    protected Vector4 m_RayleighExtinctionCoeff = new Vector4();
    protected Vector4 m_TotalMieSctrCoeff = new Vector4();
    protected Vector4 m_MieExtinctionCoeff = new Vector4();
    protected Vector4 m_TotalExtinctionCoeff = new Vector4();
    protected Vector4 m_CS_g = new Vector4();
    protected Vector4 m_ParticleScaleHeight = new Vector4(7994.0f, 1200.0f);
    protected float m_EarthRadius = 6360000.0f;
    protected float m_AtmTopHeight = 80000.0f;
    protected float m_AtmTopRadius = 6360000.0f + 80000.0f;

    public Texture2D RandomsphereSampling { get { return m_tex2DRandomsphereSampling;  } }
    public RenderTexture PrecomputeNetDensityToAtmTop { get { return m_rtPrecomputeNetDensityToAtmTop; } }
    public Texture3D SingleScaterring { get { return m_tex3DSingleScaterring; } }
    public RenderTexture AmbientSkyLight { get { return m_rt2DAmbientSkyLight; } }

    public Vector4 AngularRayleighSctrCoeff { get { return m_AngularRayleighSctrCoeff; } }
    public Vector4 AngularMieSctrCoeff { get { return m_AngularMieSctrCoeff; } }
    public Vector4 TotalRayleighSctrCoeff { get { return m_TotalRayleighSctrCoeff; } }
    public Vector4 RayleighExtinctionCoeff { get { return m_RayleighExtinctionCoeff; } }
    public Vector4 TotalMieSctrCoeff { get { return m_TotalMieSctrCoeff; } }
    public Vector4 MieExtinctionCoeff { get { return m_MieExtinctionCoeff; } }
    public Vector4 TotalExtinctionCoeff { get { return m_TotalExtinctionCoeff; } }
    public Vector4 CS_g { get { return m_CS_g; } }
    public Vector4 ParticleScaleHeight { get { return m_ParticleScaleHeight; } }
    public float EarthRadius { get { return m_EarthRadius; } }
    public float AtmTopHeight { get { return m_AtmTopHeight; } }
    public float AtmTopRadius { get { return m_AtmTopRadius; } }
  
    protected Material material
    {
        get
        {
            if (curMaterial == null)
            {
                curMaterial = new Material(curShader);
                curMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return curMaterial;
        }
    }

    void ComputeScatteringCoefficients()
    {
        double[] dWaveLengths = { 680e-9, 550e-9, 440e-9 };

        double n = 1.0003;
        double N = 2.545e+25;
        double Pn = 0.035;

        double dRayleighConst = 8.0 * Mathf.PI * Mathf.PI * Mathf.PI * (n * n - 1.0) * (n * n - 1.0) / (3.0 * N) * (6.0 + 3.0 * Pn) / (6.0 - 7.0 * Pn);
        for (int WaveNum = 0; WaveNum < 3; WaveNum++)
        {
            double dSctrCoeff;
            double Lambda2 = dWaveLengths[WaveNum] * dWaveLengths[WaveNum];
            double Lambda4 = Lambda2 * Lambda2;
            dSctrCoeff = dRayleighConst / Lambda4;
            m_TotalRayleighSctrCoeff[WaveNum] = (float)(dSctrCoeff);
            m_AngularRayleighSctrCoeff[WaveNum] = (float)(3.0 / (16.0 * Mathf.PI) * dSctrCoeff);
        }

        m_RayleighExtinctionCoeff = m_TotalRayleighSctrCoeff;
        const float fMieBethaBN08 = 2e-5f * 1.0f;

        m_TotalMieSctrCoeff.x = fMieBethaBN08; m_TotalMieSctrCoeff.y = fMieBethaBN08; m_TotalMieSctrCoeff.z = fMieBethaBN08; m_TotalMieSctrCoeff.w = 0.0f;
        for (int WaveNum = 0; WaveNum < 3; WaveNum++)
        {
            m_AngularMieSctrCoeff[WaveNum] = TotalMieSctrCoeff[WaveNum] / (float)(4.0 * Mathf.PI);
            m_MieExtinctionCoeff[WaveNum] = TotalMieSctrCoeff[WaveNum] * (1.0f + 0.1f);
        }

        float f_g = 0.76f;
        m_CS_g.x = 3 * (1.0f - f_g * f_g) / (2.0f * (2.0f + f_g * f_g));
        m_CS_g.y = 1.0f + f_g * f_g;
        m_CS_g.z = -2.0f * f_g;
        m_CS_g.w = 1.0f;
    }

    void Awake()
    {
        ComputeScatteringCoefficients();
        material.SetVector("g_f4ExtraterrestrialSunColor", new Vector4(ScatteringScale, ScatteringScale, ScatteringScale, ScatteringScale));
        material.SetVector("g_f4TotalRayleighSctrCoeff", TotalRayleighSctrCoeff);
        material.SetVector("g_f4TotalMieSctrCoeff", TotalMieSctrCoeff);
        material.SetVector("g_f4AngularRayleighSctrCoeff", AngularRayleighSctrCoeff);
        material.SetVector("g_f4AngularMieSctrCoeff", AngularMieSctrCoeff);
        material.SetVector("g_f4MieExtinctionCoeff", MieExtinctionCoeff);
        material.SetVector("g_f4RayleighExtinctionCoeff", RayleighExtinctionCoeff);
        material.SetVector("g_f4CS_g", CS_g);

        material.SetFloat("g_f1EarthRadius", EarthRadius);
        material.SetFloat("g_f1AtmTopHeight", AtmTopHeight);
        material.SetFloat("g_f1AtmTopRadius", AtmTopRadius);
        material.SetVector("g_f2ParticleScaleHeight", ParticleScaleHeight);

        material.SetVector("g_f4PrecomputeSCTRLUTDIM", new Vector4(32.0f, 128.0f, 64.0f, 16.0f));
        material.SetFloat("g_f1SafetyHeightMargin", 16.0f);
        material.SetFloat("g_f1ViewZenithPower", 0.2f);
        material.SetFloat("g_f1SunViewPower", 1.5f);
        material.SetFloat("g_f1HeightPower", 0.5f);

        Random.seed = (int)System.DateTime.Now.Ticks;

        m_tex2DRandomsphereSampling = new Texture2D(128, 1, TextureFormat.RGBAFloat, false, true);

        Color[] SphereSampling = new Color[128];
        for (int x = 0; x < 128; ++x)
        {
            SphereSampling[x].b = Random.value * 2.0f - 1.0f;
            float t = Random.value * 2.0f * Mathf.PI;
            float r = Mathf.Sqrt(Mathf.Max(1.0f - SphereSampling[x].b * SphereSampling[x].b, 0.0f));
            SphereSampling[x].r = r * Mathf.Cos(t);
            SphereSampling[x].g = r * Mathf.Sin(t);
            SphereSampling[x].a = 1.0f;
        }

        m_tex2DRandomsphereSampling.SetPixels(SphereSampling);
        m_tex2DRandomsphereSampling.Apply();

        m_rtPrecomputeNetDensityToAtmTop = RenderTexture.GetTemporary(1024, 1024, 0, RenderTextureFormat.RGFloat);
        m_rtPrecomputeNetDensityToAtmTop.generateMips = false;
        Graphics.Blit(null, m_rtPrecomputeNetDensityToAtmTop, material, 0);

        m_tex3DSingleScaterring = new Texture3D(32, 128, 1024, TextureFormat.RGBA32, false);
        RenderTexture rt3DSingleScaterring = new RenderTexture(32, 128, 0, RenderTextureFormat.ARGB32);
        rt3DSingleScaterring.generateMips = false;
        Texture2D tex2DSingleScaterring = new Texture2D(32, 128, TextureFormat.ARGB32, false);

        Color[] SingleScaterringColors = new Color[32 * 128 * 1024];

        for (uint i = 0; i < 1024; ++i)
        {
            uint uiW = i % 64;
            uint uiQ = i / 64;

            material.SetVector("g_f2WQ", new Vector4(((float)uiW + 0.5f) / 64.0f, ((float)uiQ + 0.5f) / 16.0f, 0, 0));
            material.SetTexture("g_tex2DOccludedNetDensityToAtmTop", m_rtPrecomputeNetDensityToAtmTop);
            Graphics.Blit(null, rt3DSingleScaterring, material, 1);

            RenderTexture.active = rt3DSingleScaterring;
            tex2DSingleScaterring.ReadPixels(new Rect(0, 0, 32, 128), 0, 0);
            tex2DSingleScaterring.Apply();
            Color[] colors = tex2DSingleScaterring.GetPixels();

            System.Array.Copy(colors, 0, SingleScaterringColors, i * 4096, 4096);
        }

        m_tex3DSingleScaterring.SetPixels(SingleScaterringColors);
        m_tex3DSingleScaterring.Apply();

        m_rt2DAmbientSkyLight = RenderTexture.GetTemporary(1024, 1, 0, RenderTextureFormat.ARGBHalf);
        material.SetTexture("g_tex2DSphereRandomSampling", m_tex2DRandomsphereSampling);
        material.SetTexture("g_tex3DSingleSctrLUT", m_tex3DSingleScaterring);
        Graphics.Blit(null, m_rt2DAmbientSkyLight, material, 2);



    }

    void OnEnable()
    {
    }

	// Use this for initialization
	void Start () {
        timeOfDay = GameObject.Find("Directional Light").GetComponent<TimeOfDay>();
	}


	// Update is called once per frame
	void Update () {

	}
}

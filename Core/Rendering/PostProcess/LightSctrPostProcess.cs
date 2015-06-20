using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class LightSctrPostProcess : MonoBehaviour {
    public ComputeShader curComputeShader;
    public Shader curShader;
    public float m_f1MiddleGray = 0.18f;
    public float m_f1WhitePoint = 3.0f;
    public float m_f1LuminanceSaturation = 1.0f;

    private Material curMaterial;
    protected OutdoorLightScattering OLS = null;
    protected TimeOfDay timeOfDay = null;
    private RenderTexture m_rt2DInterpolationSource = null;
    private RenderTexture [] m_rt2DLowResLuminance = new RenderTexture [7];
    private RenderTexture [] m_rt2DAverageLuminance = new RenderTexture [2];

    private int _kernel;
    public enum eTonemapType
    {
        EXP,
        UNCHARTED2
    }
    public eTonemapType curTonemapType;

    // 프랍퍼티
    private int m_iNumEpipolarSlices = 512;
    private int m_iMaxSamplesInSlice = 256;
    private float m_f1AerosolDensityScale = 1.0f;
    //private float m_f1AerosolDensityScale = 1.0f;
    //private float m_f1AerosolDensityScale = 1.0f;
    private int m_iAverageFilp = 0;
    void Update()
    {

    }

    void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;   
    }

    protected Material material
    {
        get
        {
            if ( curMaterial == null )
            {
                curMaterial = new Material(curShader);
                curMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return curMaterial;
        }
    }

	void Start () {
        if ( !SystemInfo.supportsComputeShaders )
        {
        }

        if (!SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) )
        {
            enabled = false;
            return;
        }

        if ( !curShader && !curShader.isSupported )
        {
            enabled = false;
        }

        timeOfDay = GameObject.Find("Directional Light").GetComponent<TimeOfDay>();
        OLS = GameObject.Find("Directional Light").GetComponent<OutdoorLightScattering>();

        material.SetVector("g_f4ExtraterrestrialSunColor", new Vector4(OLS.ScatteringScale, OLS.ScatteringScale, OLS.ScatteringScale, OLS.ScatteringScale));
        material.SetVector("g_f4TotalRayleighSctrCoeff", OLS.TotalRayleighSctrCoeff);
        material.SetVector("g_f4TotalMieSctrCoeff", OLS.TotalMieSctrCoeff);
        material.SetVector("g_f4AngularRayleighSctrCoeff", OLS.AngularRayleighSctrCoeff);
        material.SetVector("g_f4AngularMieSctrCoeff", OLS.AngularMieSctrCoeff);
        material.SetVector("g_f4MieExtinctionCoeff", OLS.MieExtinctionCoeff);
        material.SetVector("g_f4RayleighExtinctionCoeff", OLS.RayleighExtinctionCoeff);
        material.SetVector("g_f4CS_g", OLS.CS_g);

        material.SetFloat("g_f1EarthRadius", OLS.EarthRadius);
        material.SetFloat("g_f1AtmTopHeight", OLS.AtmTopHeight);
        material.SetFloat("g_f1AtmTopRadius", OLS.AtmTopRadius);
        material.SetVector("g_f2ParticleScaleHeight", OLS.ParticleScaleHeight);

        material.SetVector("g_f4PrecomputeSCTRLUTDIM", new Vector4(32.0f, 128.0f, 64.0f, 16.0f));
        material.SetFloat("g_f1SafetyHeightMargin", 16.0f);
        material.SetFloat("g_f1ViewZenithPower", 0.2f);
        material.SetFloat("g_f1SunViewPower", 1.5f);
        material.SetFloat("g_f1HeightPower", 0.5f);
        material.SetFloat("g_f1RefinementThreshold", 0.03f);
        material.SetFloat("g_f1MiddleGray", m_f1MiddleGray);
        material.SetFloat("g_f1WhitePoint", m_f1WhitePoint);
        material.SetFloat("g_f1LuminanceSaturation", m_f1LuminanceSaturation);
        material.SetInt("g_i1NumEpiplarSlices", m_iNumEpipolarSlices);
        material.SetInt("g_i1MaxSamplesInSlice", m_iMaxSamplesInSlice);


        _kernel = curComputeShader.FindKernel("CSMain");
        m_rt2DInterpolationSource = new RenderTexture(m_iMaxSamplesInSlice, m_iNumEpipolarSlices, 0, RenderTextureFormat.RGInt);
        m_rt2DInterpolationSource.enableRandomWrite = true;
        m_rt2DInterpolationSource.Create();

        int LowResLuminance = 64;
        for (int i = 0; i < 7; ++i )
        {
            m_rt2DLowResLuminance[i] = new RenderTexture(LowResLuminance, LowResLuminance, 0, RenderTextureFormat.RHalf);
            LowResLuminance /= 2;
        }

        m_rt2DAverageLuminance[0] = new RenderTexture(1, 1, 0, RenderTextureFormat.RHalf);
        m_rt2DAverageLuminance[1] = new RenderTexture(1, 1, 0, RenderTextureFormat.RHalf);
	}

    Vector4 Vec4TransformNormal(Vector3 V, Matrix4x4 M)
    {
        Vector4 A = new Vector4(V.x, V.y, V.z, 0.0f);

        return new Vector4( (A.x * M.m00) + (A.y * M.m10) + (A.z * M.m20) + (A.w * M.m30),
                        (A.x * M.m01) + (A.y * M.m11) + (A.z * M.m21) + (A.w * M.m31),
                        (A.x * M.m02) + (A.y * M.m12) + (A.z * M.m22) + (A.w * M.m32),
                        (A.x * M.m03) + (A.y * M.m13) + (A.z * M.m23) + (A.w * M.m33) );  
    }

    [ImageEffectOpaque]
    void OnRenderImage( RenderTexture sourceTexture, RenderTexture destTexture )
    {
        float near = Camera.main.nearClipPlane;
        //near = 100;
        float height = Mathf.Tan(3.141592654f / 180.0f * Camera.main.fov * 0.5f);
        float width = height * Camera.main.aspect;
        float diff = Camera.main.farClipPlane - near;
        float div = Camera.main.farClipPlane / diff;
        material.SetFloat("g_f1Div", div);

        Light[] WorldLight = Light.GetLights(LightType.Directional, 0);
        Vector3 lightPosition = WorldLight[0].gameObject.transform.position;
        lightPosition.Normalize();
        lightPosition = timeOfDay.LightDirection.normalized;

        Matrix4x4 CameraProj = new Matrix4x4();
        CameraProj.m00 = 1.0f / width;
        CameraProj.m11 = 1.0f / height;
        CameraProj.m22 = div;
        CameraProj.m32 = -near * div;
        CameraProj.m23 = 1.0f;

        Matrix4x4 View = new Matrix4x4();
        View.SetTRS(Camera.main.gameObject.transform.position, Camera.main.gameObject.transform.localRotation, Camera.main.gameObject.transform.localScale);

        Vector4 ScreenPosition = Vec4TransformNormal(lightPosition, View.transpose.inverse * CameraProj);
        ScreenPosition.x /= ScreenPosition.w;
        ScreenPosition.y /= ScreenPosition.w;
        ScreenPosition.z /= ScreenPosition.w;

        Vector2 LightLen = new Vector2(ScreenPosition.x, ScreenPosition.y);
        float DistToLightOnScreen = LightLen.magnitude;
        if ( DistToLightOnScreen > 100 )
        {
            float InvDistToLightOnScreen = 100 / DistToLightOnScreen;
            ScreenPosition.x *= InvDistToLightOnScreen;
            ScreenPosition.y *= InvDistToLightOnScreen;
        }
        bool LightOnScreen = Mathf.Abs(ScreenPosition.x) <= 1.0f - 1.0f / (float)sourceTexture.width &&
                            Mathf.Abs(ScreenPosition.y) <= 1.0f - 1.0f / (float)sourceTexture.height;
  
        material.SetVector("g_f2ScreenLightPos", new Vector4(ScreenPosition.x, ScreenPosition.y, 0, 0));
        material.SetInt("g_b1IsLightOnScreen", LightOnScreen ? 1 : 0);
        material.SetMatrix("g_mInvViewProj", (View.transpose.inverse * CameraProj).inverse);
        material.SetVector("g_f4DirInLight", new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 0.0f));

        float [] BlendFactor = {0.0f, 0.0f, 0.0f, 0.0f};
	    float InvalidCoordinate = -1e+30f;
        float flt16max = 65504.0f;
 
        Color InvalidCoords = new Color(InvalidCoordinate, InvalidCoordinate, InvalidCoordinate, InvalidCoordinate);
	    Color InvalidInsctr = new Color(-flt16max, -flt16max, -flt16max, -flt16max);
        Color One = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Color Zero = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        if (curShader != null && curComputeShader != null)
        {
            RenderTexture rt2DCameraSpaceZ = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.RFloat);
            rt2DCameraSpaceZ.generateMips = false;  // 이것은 나중에 빼보고 테스트 해보자
            Graphics.Blit(null, rt2DCameraSpaceZ, material, 3);

            RenderTexture rt2DSliceEndpoints = RenderTexture.GetTemporary(m_iNumEpipolarSlices, 1, 0, RenderTextureFormat.ARGBFloat);
            rt2DSliceEndpoints.generateMips = false; // 이것은 나중에 빼보고 테스트 해보자
            Graphics.Blit(null, rt2DSliceEndpoints, material, 4);

            RenderTexture rt2DCoordinateTexture = RenderTexture.GetTemporary(m_iMaxSamplesInSlice, m_iNumEpipolarSlices, 0, RenderTextureFormat.RGFloat);
            rt2DCoordinateTexture.generateMips = false; // 이것은 나중에 빼보고 테스트 해보자
            RenderTexture rt2DEpipolarCamSpaceZ = RenderTexture.GetTemporary(m_iMaxSamplesInSlice, m_iNumEpipolarSlices, 24, RenderTextureFormat.RFloat);
            rt2DEpipolarCamSpaceZ.generateMips = false; // 이것은 나중에 빼보고 테스트 해보자

            RenderBuffer [] rbGenerateCoordinate = { rt2DCoordinateTexture.colorBuffer, rt2DEpipolarCamSpaceZ.colorBuffer };
            Graphics.SetRenderTarget(rbGenerateCoordinate, rt2DEpipolarCamSpaceZ.depthBuffer); // 렌더타겟 설정하고 blit에서 desttarget 없으면 렌더타겟이 사용된다.
            GL.Clear(true, true, InvalidCoords, 1.0f); // 깊이값 초기화시 스텐실 자동으로 0으로 초기화 된다.

            material.SetTexture("g_tex2DCamSpaceZ", rt2DCameraSpaceZ);
            material.SetTexture("g_tex2DSliceEndPoints", rt2DSliceEndpoints);
            Graphics.Blit(null, material, 5);


            RenderTexture rt2DEpipolarExtinction = RenderTexture.GetTemporary(m_iMaxSamplesInSlice, m_iNumEpipolarSlices, 0, RenderTextureFormat.ARGB32);
            rt2DEpipolarExtinction.generateMips = false; // 이것은 나중에 빼보고 테스트 해보자
            RenderTexture rt2DEpipolarInscattering = RenderTexture.GetTemporary(m_iMaxSamplesInSlice, m_iNumEpipolarSlices, 0, RenderTextureFormat.ARGBHalf);
            rt2DEpipolarInscattering.generateMips = false; // 이것은 나중에 빼보고 테스트 해보자

            Graphics.SetRenderTarget(rt2DEpipolarExtinction);
            GL.Clear(false, true, One, 1.0f);
            Graphics.SetRenderTarget(rt2DEpipolarInscattering);
            GL.Clear(false, true, InvalidInsctr, 1.0f);

            RenderBuffer[] rtCoarseUnshadowedInsctr = { rt2DEpipolarInscattering.colorBuffer, rt2DEpipolarExtinction.colorBuffer };
            Graphics.SetRenderTarget(rtCoarseUnshadowedInsctr, rt2DEpipolarCamSpaceZ.depthBuffer);

            material.SetTexture("g_tex2DCoordinates", rt2DCoordinateTexture);
            material.SetTexture("g_tex2DEpipolarCamSpaceZ", rt2DEpipolarCamSpaceZ);
            material.SetTexture("g_tex3DSingleSctrLUT", OLS.SingleScaterring);
            Graphics.Blit(null, material, 6);

            curComputeShader.SetVector("g_f2ScreenLightPos", new Vector4(ScreenPosition.x, ScreenPosition.y, 0, 0));
            curComputeShader.SetFloat("g_f1RefinementThreshold", 0.3f);
            curComputeShader.SetFloat("g_f1MiddleGray", 0.18f);
            curComputeShader.SetInt("g_n1MaxSamplesInSlice", m_iMaxSamplesInSlice);
            curComputeShader.SetInt("g_n1EpipoleSamplingDensityFactor", 2);
            curComputeShader.SetTexture(_kernel, "g_tex2DCoordinates", rt2DCoordinateTexture);
            curComputeShader.SetTexture(_kernel, "g_tex2DEpipolarCamSpaceZ", rt2DEpipolarCamSpaceZ);
            curComputeShader.SetTexture(_kernel, "g_tex2DScatteredColor", rt2DEpipolarInscattering);
            curComputeShader.SetTexture(_kernel, "g_tex2DAverageLuminance", m_rt2DAverageLuminance[1 - m_iAverageFilp] );

            curComputeShader.SetTexture(_kernel, "g_rwtex2DInterpolationSource", m_rt2DInterpolationSource);
            curComputeShader.Dispatch(_kernel, 2, m_iNumEpipolarSlices, 1);

            RenderTexture rtDummy = RenderTexture.GetTemporary(m_iMaxSamplesInSlice, m_iNumEpipolarSlices, 0, RenderTextureFormat.ARGB32);
            Graphics.SetRenderTarget(rtDummy.colorBuffer, rt2DEpipolarCamSpaceZ.depthBuffer);
            material.SetTexture("g_tex2DInterpolationSource", m_rt2DInterpolationSource);
            Graphics.Blit(null, material, 7);


            /* 스케터링 안함
            RenderTexture rt2DSliceUVDirAndOrigin = RenderTexture.GetTemporary(m_iNumEpipolarSlices, 4, 0, RenderTextureFormat.ARGBFloat);

            material.SetTexture("g_tex2DCamSpaceZ", rt2DCoordinateTexture);
            material.SetTexture("g_tex2DSliceEndPoints", rt2DSliceEndpoints);

            RenderTexture.active = rt2DSliceUVDirAndOrigin;
            GL.PushMatrix();
            material.SetPass(8);
            GL.LoadOrtho();
            GL.Viewport( new Rect(0.0f, 1.0f, 512.0f, 3.0f) );

            GL.Begin(GL.QUADS);

            GL.Vertex3(0.0f, 0.0f, 0.1f);
            GL.TexCoord2(1.0f, 0.0f + (1.0f/4.0f));
            GL.Vertex3(1.0f, 0.0f, 0.1f);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 0.1f);
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.1f);
            GL.TexCoord2(0.0f, 0.0f + (1.0f / 4.0f));

            GL.End();
            GL.PopMatrix();
           */
            material.SetTexture("g_tex2DCoordinates", rt2DCoordinateTexture);
            material.SetTexture("g_tex2DEpipolarCamSpaceZ", rt2DEpipolarCamSpaceZ);
            material.SetTexture("g_tex3DSingleSctrLUT", OLS.SingleScaterring);
            
            RenderTexture rt2DInitialScatteredLight = RenderTexture.GetTemporary(m_iMaxSamplesInSlice, m_iNumEpipolarSlices, 0, RenderTextureFormat.ARGBHalf);
            Graphics.SetRenderTarget(rt2DInitialScatteredLight.colorBuffer, rt2DEpipolarCamSpaceZ.depthBuffer);
            GL.Clear(false, true, Zero, 1.0f);
            Graphics.Blit(null, material, 11);

 
            material.SetTexture("g_tex2DInitialInsctrIrradiance", rt2DInitialScatteredLight);
            material.SetTexture("g_tex2DInterpolationSource", m_rt2DInterpolationSource);
            Graphics.Blit(sourceTexture, material, 12);


            
                        material.SetTexture("_2DRandomsphereSampling", OLS.RandomsphereSampling);
                        material.SetTexture("_PrecomputeNetDensityToAtmTop", OLS.PrecomputeNetDensityToAtmTop);
                        material.SetTexture("_PrecomputeSingleScattering", OLS.SingleScaterring);
                        material.SetTexture("_AmbientSkyLight", OLS.AmbientSkyLight);
                        material.SetTexture("_2DCameraSpaceZ", rt2DCameraSpaceZ);   
                        material.SetTexture("_2DSliceEndpoints", rt2DSliceEndpoints);
                        material.SetTexture("_2DCoordinateTexture", rt2DCoordinateTexture);
                        material.SetTexture("_2DEpipolarCamSpaceZ", rt2DEpipolarCamSpaceZ);
                        material.SetTexture("_2DEpipolarInscattering", rt2DEpipolarInscattering);
                        material.SetTexture("_2DEpipolarExtinction", rt2DEpipolarExtinction);
                        material.SetTexture("_2DInterpolationSource", m_rt2DInterpolationSource);
                        material.SetTexture("_2DInitialScatteredLight", rt2DInitialScatteredLight);

                      //  Graphics.Blit(sourceTexture, destTexture, material, 0);
            
          
            
                    material.SetTexture("g_tex2DEpipolarInscattering", rt2DEpipolarInscattering);
                    material.SetTexture("g_tex2DEpipolarExtinction", rt2DEpipolarExtinction);
                    material.SetTexture("g_tex2DSliceEndPoints", rt2DSliceEndpoints);
                    material.SetTexture("g_tex2DEpipolarCamSpaceZ", rt2DEpipolarCamSpaceZ);

            
                   // Graphics.Blit(sourceTexture, destTexture, material, 13);// 루미넨스

                    for (int i = 0; i < 6; ++i)
                    {
                        material.SetTexture("g_tex2DLowResLuminance", m_rt2DLowResLuminance[i]);
                        Graphics.Blit(null, m_rt2DLowResLuminance[i+1], material, 14);
                    }

                    material.SetTexture("g_tex2DLowResLuminance", m_rt2DLowResLuminance[6]);
                    material.SetTexture("g_tex2DAverageLuminance", m_rt2DAverageLuminance[1 - m_iAverageFilp]);
                    Graphics.Blit(null, m_rt2DAverageLuminance[m_iAverageFilp], material, 15);// 루미넨스
                    
           
            material.SetTexture("g_tex2DEpipolarInscattering", rt2DEpipolarInscattering);
            material.SetTexture("g_tex2DEpipolarExtinction", rt2DEpipolarExtinction);
            material.SetTexture("g_tex2DSliceEndPoints", rt2DSliceEndpoints);
            material.SetTexture("g_tex2DEpipolarCamSpaceZ", rt2DEpipolarCamSpaceZ);

            RenderTexture rtFinalColor = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);
            Graphics.Blit(sourceTexture, rtFinalColor, material, 13);

            material.SetFloat("g_f1MiddleGray", m_f1MiddleGray);
            material.SetFloat("g_f1WhitePoint", m_f1WhitePoint);
            material.SetFloat("g_f1LuminanceSaturation", m_f1LuminanceSaturation);
            Graphics.Blit(rtFinalColor, destTexture, material, 16 + (int)curTonemapType);
            m_iAverageFilp = 1 - m_iAverageFilp;
            
            RenderTexture.ReleaseTemporary(rt2DSliceEndpoints);
            RenderTexture.ReleaseTemporary(rt2DCameraSpaceZ);
            RenderTexture.ReleaseTemporary(rt2DCoordinateTexture);
            RenderTexture.ReleaseTemporary(rt2DEpipolarCamSpaceZ);
            RenderTexture.ReleaseTemporary(rt2DEpipolarExtinction);
            RenderTexture.ReleaseTemporary(rt2DEpipolarInscattering);
            RenderTexture.ReleaseTemporary(rtDummy);
            //RenderTexture.ReleaseTemporary(rt2DSliceUVDirAndOrigin);
            RenderTexture.ReleaseTemporary(rt2DInitialScatteredLight);
           RenderTexture.ReleaseTemporary(rtFinalColor);
        }
        else
        {
            Graphics.Blit(sourceTexture, destTexture);
        }
    }

    void OnDisable()
    {
       // if (m_rtPrecomputeNetDensityToAtmTop)
         //   RenderTexture.ReleaseTemporary(m_rtPrecomputeNetDensityToAtmTop);

        if (curMaterial)
        {
            DestroyImmediate(curMaterial);
        }
    }
}



/*
void RenderCoarseUnshadowedInsctrPS(SScreenSizeQuadVSOutput In,
                                    out float3 f3Inscattering : SV_Target0
#if EXTINCTION_EVAL_MODE == EXTINCTION_EVAL_MODE_EPIPOLAR
                                  , out float3 f3Extinction   : SV_Target1
#endif
                                  ) 
{
    // Compute unshadowed inscattering from the camera to the ray end point using few steps
    float fCamSpaceZ =  g_tex2DEpipolarCamSpaceZ.Load( uint3(In.m_f4Pos.xy, 0) );
    float2 f2SampleLocation = g_tex2DCoordinates.Load( uint3(In.m_f4Pos.xy, 0) );
#if EXTINCTION_EVAL_MODE != EXTINCTION_EVAL_MODE_EPIPOLAR
    float3 f3Extinction = 1;
#endif

    ComputeUnshadowedInscattering(f2SampleLocation, fCamSpaceZ, 
                                  7, // Use hard-coded constant here so that compiler can optimize the code
                                     // more efficiently
                                  f3Inscattering, f3Extinction);
    f3Inscattering *= g_LightAttribs.f4ExtraterrestrialSunColor.rgb;
}*/
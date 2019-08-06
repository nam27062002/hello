using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockEffectMenu : MonoBehaviour {

    public Shader m_replaceShader;
    public Material m_effectMaterial;
    public Material m_effectDilateMaterial;

    public int m_iterations = 8;
    public float m_maskDecay = 0.9f;

    private RenderTexture m_replaceRenderTexture;
    private RenderTexture m_backupTexture;

    private Camera m_origCamera;
    private Camera m_shaderCamera;


    private void setPreCameraData(Camera origCamera)
    {
        m_shaderCamera.CopyFrom(origCamera);
        m_shaderCamera.backgroundColor = Color.clear;
        m_shaderCamera.clearFlags = CameraClearFlags.SolidColor;
        m_shaderCamera.renderingPath = RenderingPath.Forward;
        m_shaderCamera.targetTexture = m_replaceRenderTexture;
//        m_shaderCamera.rect = normalizedRect;
//        m_shaderCamera.cullingMask = shaderCullingMask;
    }


    void Awake()
    {
        m_origCamera = GetComponent<Camera>();
        if (!FeatureSettingsManager.instance.IsLockEffectEnabled)
        {
            Destroy(this);
        }
    }


    // Use this for initialization
    void Start () {
        // Disable if we don't support image effects
        if (!SystemInfo.supportsImageEffects)
        {
            Debug.Log("Disabling the Glow Effect. Image effects are not supported (do you have Unity Pro?)");
            enabled = false;
        }

    }


    public void OnEnable()
    {
        m_replaceRenderTexture = new RenderTexture((int)(m_origCamera.pixelWidth), (int)(m_origCamera.pixelHeight), 16, RenderTextureFormat.ARGB32);
        m_replaceRenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_replaceRenderTexture.useMipMap = false;
        m_replaceRenderTexture.filterMode = FilterMode.Bilinear;
        m_replaceRenderTexture.Create();

        m_backupTexture = new RenderTexture((int)(m_origCamera.pixelWidth), (int)(m_origCamera.pixelHeight), 16, RenderTextureFormat.ARGB32);
        m_backupTexture.wrapMode = TextureWrapMode.Clamp;
        m_backupTexture.useMipMap = false;
        m_backupTexture.filterMode = FilterMode.Bilinear;
        m_backupTexture.Create();

        m_effectMaterial.SetTexture("_Lock", m_replaceRenderTexture);

        m_shaderCamera = new GameObject("Lock Effect", typeof(Camera)).GetComponent<Camera>();
        m_shaderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;

        setPreCameraData(m_origCamera);

    }


    public void OnDisable()
    {
        m_effectMaterial.mainTexture = null;
        m_origCamera.targetTexture = null;
        DestroyObject(m_shaderCamera);
    }




    public void OnPreRender()
    {
        setPreCameraData(m_origCamera);
        m_shaderCamera.RenderWithShader(m_replaceShader, "Lock");
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
//        m_effectDilateMaterial
        RenderTexture ta = m_replaceRenderTexture;
        RenderTexture tb = m_backupTexture;

        m_effectDilateMaterial.SetFloat("_DilateDecay", m_maskDecay);

        for (int c = 0; c < m_iterations; c++)
        {
            Graphics.Blit(ta, tb, m_effectDilateMaterial);
            RenderTexture tmp = ta;
            ta = tb;
            tb = tmp;
        }

        Graphics.Blit(source, destination, m_effectMaterial);
    }


    void setFreezeMaterial(Material mat)
    {
        mat.EnableKeyword(GameConstants.Materials.Keyword.FRESNEL);
        mat.EnableKeyword(GameConstants.Materials.Keyword.FREEZE);
        mat.EnableKeyword(GameConstants.Materials.Keyword.MATCAP);
        mat.SetColor(GameConstants.Materials.Property.FRESNEL_COLOR, new Color(114.0f / 255.0f, 248.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f));
        mat.SetColor(GameConstants.Materials.Property.FRESNEL_COLOR_2, new Color(186.0f / 255.0f, 144.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f));
        mat.SetFloat(GameConstants.Materials.Property.FRESNEL_POWER, 0.91f);
        mat.SetColor(GameConstants.Materials.Property.GOLD_COLOR, new Color(179.0f / 255.0f, 250.0f / 255.0f, 254.0f / 255.0f, 64.0f / 255.0f));
    }

}

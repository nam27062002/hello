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

    private Camera origCamera;
    private Camera shaderCamera;

    void Awake()
    {
        origCamera = GetComponent<Camera>();
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
        m_replaceRenderTexture = new RenderTexture((int)(origCamera.pixelWidth), (int)(origCamera.pixelHeight), 16, RenderTextureFormat.ARGB32);
        m_replaceRenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_replaceRenderTexture.useMipMap = false;
        m_replaceRenderTexture.filterMode = FilterMode.Bilinear;
        m_replaceRenderTexture.Create();

        m_backupTexture = new RenderTexture((int)(origCamera.pixelWidth), (int)(origCamera.pixelHeight), 16, RenderTextureFormat.ARGB32);
        m_backupTexture.wrapMode = TextureWrapMode.Clamp;
        m_backupTexture.useMipMap = false;
        m_backupTexture.filterMode = FilterMode.Bilinear;
        m_backupTexture.Create();

        m_effectMaterial.SetTexture("_Lock", m_replaceRenderTexture);

        shaderCamera = new GameObject("Lock Effect", typeof(Camera)).GetComponent<Camera>();
        shaderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
    }


    public void OnDisable()
    {
        m_effectMaterial.mainTexture = null;
        origCamera.targetTexture = null;
        DestroyObject(shaderCamera);
    }



    public void OnPreRender()
    {
        shaderCamera.CopyFrom(origCamera);
        shaderCamera.backgroundColor = Color.clear;
        shaderCamera.clearFlags = CameraClearFlags.SolidColor;
        shaderCamera.renderingPath = RenderingPath.Forward;
        shaderCamera.targetTexture = m_replaceRenderTexture;
//        shaderCamera.rect = normalizedRect;
//        shaderCamera.cullingMask = shaderCullingMask;
        shaderCamera.RenderWithShader(m_replaceShader, "Lock");
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

}

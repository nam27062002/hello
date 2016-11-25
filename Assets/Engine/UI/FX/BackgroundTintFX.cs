using UnityEngine;
using System.Collections;

public class BackgroundTintFX : MonoBehaviour {

    public Color m_Tint = Color.white;
    public Color m_Tint2 = Color.white;
    public Shader m_shader = null;

//    private Camera m_mapCamera;
    private Material m_material;

    private bool m_tintActive;

    private Camera m_originalCamera = null;
    private Camera m_renderCamera = null;
    private RenderTexture m_renderTexture = null;
    private RenderTexture m_buffer = null;

    void Awake()
    {

    }
    // Use this for initialization
    void Start()
    {
        m_material = new Material(m_shader);
        setTint(m_Tint, m_Tint2);

        m_originalCamera = GetComponent<Camera>();

        m_renderTexture = new RenderTexture((int)m_originalCamera.pixelWidth, (int)m_originalCamera.pixelHeight, 24, RenderTextureFormat.ARGB32);
//        m_renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        m_renderTexture.wrapMode = TextureWrapMode.Clamp;
        m_renderTexture.useMipMap = false;
        m_renderTexture.filterMode = FilterMode.Bilinear;
        m_renderTexture.Create();

        m_buffer = new RenderTexture((int)m_originalCamera.pixelWidth, (int)m_originalCamera.pixelHeight, 24, RenderTextureFormat.ARGB32);

        m_renderCamera = new GameObject("Background tint camera", typeof(Camera)).GetComponent<Camera>();
//        m_renderCamera.transform.SetParent(transform);

        m_renderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
        m_renderCamera.targetTexture = m_renderTexture;
        m_renderCamera.backgroundColor = Color.clear;
        m_renderCamera.clearFlags = CameraClearFlags.SolidColor;
        m_renderCamera.renderingPath = RenderingPath.Forward;
        m_renderCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);


        Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
    }

    public void OnDestroy()
    {
        m_renderCamera.targetTexture = null;
        DestroyObject(m_renderCamera);
    }



    void OnFuryToggled(bool active, DragonBreathBehaviour.Type type)
    {
        m_tintActive = active;
    }

    public void setTint(Color col)
    {
        m_material.SetColor("_Tint", col);
        m_material.SetColor("_Tint2", col);
    }
    public void setTint(Color col, Color col2)
    {
        m_material.SetColor("_Tint", col);
        m_material.SetColor("_Tint2", col2);
    }

    // Postprocess the image
    void OnPostRender()
    {
        if (m_tintActive)
        {
            Graphics.SetRenderTarget(m_buffer.colorBuffer, m_renderTexture.depthBuffer);
            Graphics.Blit(m_renderTexture, m_buffer);
            RenderTexture.active = null;
            Graphics.Blit(m_buffer, m_material);
        }
        else
        {
        }

    }


    void OnPreRender()
    {
        m_renderCamera.CopyFrom(m_originalCamera);
//        m_renderCamera.transform.CopyFrom(m_originalCamera.transform, true);
        m_renderCamera.backgroundColor = Color.clear;
        m_renderCamera.clearFlags = CameraClearFlags.SolidColor;
        m_renderCamera.renderingPath = RenderingPath.Forward;
        m_renderCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        m_renderCamera.targetTexture = m_renderTexture;
        m_renderCamera.Render();

    }
}

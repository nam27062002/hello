using UnityEngine;
using System.Collections;

public class BackgroundTintFX : MonoBehaviour {

    public Color m_Tint = Color.white;
    public Color m_Tint2 = Color.white;
    public float timeDecay = 2.0f;
    public Shader m_shader = null;

//    private Camera m_mapCamera;
    private Material m_material;

    private bool m_tintActive;
    private bool m_fade = true;
    private float m_startTime;

    private Camera m_originalCamera = null;
    private Camera m_renderCamera = null;
    private RenderTexture m_renderTexture = null;
    private RenderTexture m_buffer = null;

//    private int m_cullingMask;

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
		m_buffer.wrapMode = TextureWrapMode.Clamp;
		m_buffer.useMipMap = false;
		m_buffer.filterMode = FilterMode.Bilinear;
		m_buffer.Create();

        m_renderCamera = new GameObject("Background tint camera", typeof(Camera)).GetComponent<Camera>();
//        m_renderCamera.transform.SetParent(transform);

        m_renderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
        m_renderCamera.targetTexture = m_renderTexture;
        m_renderCamera.backgroundColor = Color.clear;
        m_renderCamera.clearFlags = CameraClearFlags.SolidColor;
        m_renderCamera.renderingPath = RenderingPath.Forward;
        //      Shader rShader = Shader.Find("Hidden/VoidReplacement");
        //      m_renderCamera.SetReplacementShader(rShader, "RenderType");
        m_renderCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);


        setRenderCameraActive(false);
        m_tintActive = false;
        m_fade = true;

        Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
    }

    public void OnDestroy()
    {
        Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);

        m_renderCamera.targetTexture = null;
        DestroyObject(m_renderCamera);

    }

    void setRenderCameraActive(bool active)
    {
        m_renderCamera.gameObject.SetActive(active);
    }

    public void Update()
    {
        if (m_tintActive)
        {
            float dTime = (Time.time - m_startTime) / timeDecay;

            if (!m_fade)
            {
                dTime = 1.0f - dTime;
                if (dTime < 0.0f)
                {
                    m_tintActive = false;
                    setRenderCameraActive(false);

                }
            }

            dTime = Mathf.Clamp(dTime, 0.0f, 1.0f);
            setTint(Color.Lerp(Color.white, m_Tint, dTime), Color.Lerp(Color.white, m_Tint2, dTime));
        }
    }

    void OnFuryToggled(bool active, DragonBreathBehaviour.Type type)
    {
        m_startTime = Time.time;
        if (active)
        {
            m_tintActive = active;
            m_fade = true;
            setRenderCameraActive(true);
        }
        else
        {
            m_fade = false;
        }
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

    }


    void OnPreRender()
    {

        if (m_tintActive)
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
}

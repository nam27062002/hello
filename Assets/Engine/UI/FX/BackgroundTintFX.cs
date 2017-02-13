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

    private bool m_wasEnabled = false;
	private Shader m_replacementShader = null;

    [Range(1.0f, 10.0f)]
    public float m_TexelOffset = 2.0f;

    [Range(0.0f, 1.0f)]
    public float m_LensOffset = 0.5f;
    //    private int m_cullingMask;

    // Use this for initialization
    void Start()
    {
        m_material = new Material(m_shader);

        m_originalCamera = GetComponent<Camera>();
        m_originalCamera.depthTextureMode = DepthTextureMode.Depth;

/*
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
        m_replacementShader = Shader.Find("Hidden/VoidReplacement");

        m_renderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
        m_renderCamera.targetTexture = m_renderTexture;
        m_renderCamera.backgroundColor = Color.clear;
        m_renderCamera.clearFlags = CameraClearFlags.SolidColor;
        m_renderCamera.renderingPath = RenderingPath.UsePlayerSettings;     //RenderingPath.Forward;
//		m_renderCamera.SetReplacementShader(m_replacementShader, "RenderType");
        m_renderCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);


        setRenderCameraActive(false);
*/
        m_tintActive = false;
        m_fade = true;

//        Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
//        m_wasEnabled = true;

//        OnFuryToggled(true, DragonBreathBehaviour.Type.Super);

    }

    public void OnDestroy()
    {
//        if (m_wasEnabled)
//        {
//            Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
//        }
        if (m_renderCamera != null)
        {
            m_renderCamera.targetTexture = null;
            DestroyObject(m_renderCamera);
        }

    }

    void setRenderCameraActive(bool active)
    {
        m_renderCamera.gameObject.SetActive(active);
    }

    public void Update()
    {
        m_material.SetFloat("_TexelOffset", m_TexelOffset);
        m_material.SetFloat("_LensOffset", m_LensOffset);
    }

    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        Graphics.Blit(source, dest, m_material);

    }
    /*

            // Postprocess the image
            void OnPostRender()
            {
                if (m_tintActive)
                {
                    //            Graphics.SetRenderTarget(m_buffer.colorBuffer, m_renderTexture.depthBuffer);

        //            Graphics.SetRenderTarget(m_buffer.colorBuffer, m_renderTexture.depthBuffer);
        //            Graphics.Blit(m_renderTexture, m_buffer);
        //            m_material.SetTexture("_Depth", m_buffer);
        //            RenderTexture.active = null;
                    Graphics.Blit(m_buffer, m_material);
                }

            }

            void OnPreRender()
            {

                if (m_tintActive)
                {
                    m_renderCamera.CopyFrom(m_originalCamera);
        //          m_renderCamera.transform.CopyFrom(m_originalCamera.transform, true);
                    m_renderCamera.backgroundColor = Color.clear;
                    m_renderCamera.clearFlags = CameraClearFlags.SolidColor;
                    m_renderCamera.renderingPath = RenderingPath.Forward;
                    m_renderCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                    m_renderCamera.depthTextureMode = DepthTextureMode.Depth;
                    m_renderCamera.targetTexture = m_renderTexture;
                    m_renderCamera.Render();
        //			m_renderCamera.RenderWithShader(m_replacementShader, "RenderType");
                }

            }
        */
}

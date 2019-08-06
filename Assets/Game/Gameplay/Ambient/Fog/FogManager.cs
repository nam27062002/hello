using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class FogManager : MonoBehaviour, IBroadcastListener {

    private int m_maxGradientTextures = 64;

    [System.Serializable]
    public class FogAttributes// : ISerializationCallbackReceiver
    {
        public const int TEXTURE_SIZE = 128;

        public Gradient m_fogGradient;
        public float m_fogStart;
        public float m_fogEnd;
        private Texture2D m_texture;
        public Texture2D texture {
            get { return m_texture; }
            set { m_texture = value; }
        }


        /*
		public List<float> m_alphaTimes = new List<float>();
		public List<float> m_alphaValues = new List<float>();
		public List<float> m_colorTimes = new List<float>();
		public List<Color> m_colorValues = new List<Color>();
		*/
        public void CreateTexture() {
            m_texture = new Texture2D(TEXTURE_SIZE, 1, TextureFormat.RGBA32, false);
            m_texture.filterMode = FilterMode.Bilinear;
            m_texture.wrapMode = TextureWrapMode.Clamp;
        }

        public void DestroyTexture(bool inmediate = false) {
            if (m_texture != null) {
                if (inmediate)
                    DestroyImmediate(m_texture);
                else
                    Destroy(m_texture);
                m_texture = null;
            }
        }

        public void RefreshTexture() {
            for (int i = 0; i < TEXTURE_SIZE; i++) {
                m_texture.SetPixel(i, 0, m_fogGradient.Evaluate(i / (float)TEXTURE_SIZE));
            }
            m_texture.Apply(false);
        }

        /// <summary>
        /// WARNING: This only should be done if there is no fog manager
        /// Fogs the setup. Sets the shaders variables to this fog
        /// </summary>
        public void FogSetup() {
            Shader.SetGlobalFloat(GameConstants.Materials.Property.FOG_START, m_fogStart);
            Shader.SetGlobalFloat(GameConstants.Materials.Property.FOG_END, m_fogEnd);
            Shader.SetGlobalTexture(GameConstants.Materials.Property.FOG_TEXTURE, texture);
        }
    }

    // For Area Mode
    public FogAttributes m_defaultAreaFog;
    public float m_transitionDuration = 1.0f;

    // Runtime variables
    List<FogAttributes> m_generatedAttributes = new List<FogAttributes>();
    List<FogArea> m_activeFogAreaList = new List<FogArea>();
    // While in fire Fogs
    List<FogArea> m_activeFireFogAreaList = new List<FogArea>();

    float m_start;
    float m_end;
    // Texture to apply to the shader
    Texture2D m_texture;
    Color[] m_textureColors = new Color[FogAttributes.TEXTURE_SIZE];

    float m_tmpStart;
    float m_tmpEnd;
    Color[] m_tmpTexture = new Color[FogAttributes.TEXTURE_SIZE];
    bool m_updateTmpTexture = false;

    bool m_firstTime = true;
    public bool firstTime { get { return m_firstTime; } set { m_firstTime = value; } }

    float m_transitionTimer = 0;
    FogAttributes m_lastSelectedAttributes;
    FogAttributes m_selectedAttributes = null;
    Color[] m_selectedTextureColors = new Color[FogAttributes.TEXTURE_SIZE];
    FogArea m_lastSelectedArea = null;
    bool m_updateValues = true;


    FogAttributes m_forcedAttributes = null;
    bool m_forceUpdate = false;
    bool m_usingFire = false;
    bool m_wasUsingFire = false;


    void Awake() {
        if (Application.isPlaying) {
            InstanceManager.fogManager = this;
        }

        m_texture = new Texture2D(FogAttributes.TEXTURE_SIZE, 1, TextureFormat.RGBA32, false);
        m_texture.filterMode = FilterMode.Bilinear;
        m_texture.wrapMode = TextureWrapMode.Clamp;

        // Register default attributes
        CheckTextureAvailability(m_defaultAreaFog);

        if (!Application.isPlaying) {
            RefreshFog();
        }
    }

    void Start() {
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
    }

    void OnDestroy() {
        if (Application.isPlaying && ApplicationManager.IsAlive) {
            InstanceManager.fogManager = null;

            Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_EXIT, this);
            Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
        }
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.GAME_AREA_EXIT: {
                    OnAreaExit();
                }
                break;
            case BroadcastEventType.FURY_RUSH_TOGGLED: {
                    FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                    OnFury(furyRushToggled.activated, furyRushToggled.type);
                }
                break;
        }
    }

    void OnAreaExit() {
        // Clean all for areas?
        for (int i = m_generatedAttributes.Count - 1; i >= 0; i--) {
            if (m_generatedAttributes[i].texture != m_defaultAreaFog.texture) {
                bool toDestroy = true;
                // if this generated is not from the activated area list
                for (int j = 0; j < m_activeFogAreaList.Count && toDestroy; j++) {
                    // if we are using it we dont destory it
                    if (m_generatedAttributes[i].texture == m_activeFogAreaList[j].m_attributes.texture) {
                        toDestroy = false;
                    }
                }

                if (toDestroy) {
                    // Destroy Texture and remove attributes
                    m_generatedAttributes[i].DestroyTexture();
                    m_generatedAttributes[i].texture = null;
                    m_generatedAttributes.RemoveAt(i);
                }
            }
        }
    }

    public void Update() {
        if (Application.isPlaying) {

            if (m_forcedAttributes == null) {
                float transitionDuration = 0;
                if (m_usingFire && m_activeFireFogAreaList.Count > 0) {
                    FogArea selectedFogArea = m_activeFireFogAreaList.Last<FogArea>();
                    m_selectedAttributes = selectedFogArea.m_attributes;
                    if (m_wasUsingFire) {
                        transitionDuration = selectedFogArea.m_enterTransitionDuration;
                    } else {
                        transitionDuration = 0.1f;
                    }
                    m_wasUsingFire = true;
                    m_lastSelectedArea = selectedFogArea;

                } else if (m_activeFogAreaList.Count > 0) {
                    FogArea selectedFogArea = m_activeFogAreaList.Last<FogArea>();
                    if (m_wasUsingFire) {
                        transitionDuration = 0.1f;
                    } else {
                        transitionDuration = selectedFogArea.m_enterTransitionDuration;
                    }
                    m_selectedAttributes = selectedFogArea.m_attributes;
                    m_wasUsingFire = false;
                    m_lastSelectedArea = selectedFogArea;
                } else {
                    m_selectedAttributes = m_defaultAreaFog;
                    if (m_lastSelectedArea != null) {
                        transitionDuration = m_lastSelectedArea.m_exitTransitionDuration;
                    } else {
                        transitionDuration = 1.6f;
                    }
                }

                if (m_lastSelectedAttributes != m_selectedAttributes) {
                    m_tmpStart = m_start;
                    m_tmpEnd = m_end;

                    m_lastSelectedAttributes = m_selectedAttributes;
                    m_transitionTimer = m_transitionDuration = transitionDuration;
                    // Copy destination render texture to original texture
                    m_updateTmpTexture = true;

                    // Update selected colors
                    for (int i = 0; i < FogAttributes.TEXTURE_SIZE; i++)
                        m_selectedTextureColors[i] = m_selectedAttributes.texture.GetPixel(i, 0);
                }
            }
            UpdateApplyMode();
        }
    }

    void UpdateApplyMode() {
        m_updateValues = false;
        if (m_firstTime) {
            m_firstTime = false;
            m_updateValues = true;
            SetAsSelectedAttributes();
        } else {
            if (m_updateTmpTexture) {
                for (int i = 0; i < FogAttributes.TEXTURE_SIZE; i++)
                    m_tmpTexture[i] = m_texture.GetPixel(i, 0);
            }
            if (m_transitionTimer > 0) {
                m_updateValues = true;
                m_transitionTimer -= Time.deltaTime;
                if (m_transitionTimer <= 0) {
                    SetAsSelectedAttributes();
                } else {
                    float delta = 1.0f - (m_transitionTimer / m_transitionDuration);
                    m_start = Mathf.Lerp(m_tmpStart, m_selectedAttributes.m_fogStart, delta);
                    m_end = Mathf.Lerp(m_tmpEnd, m_selectedAttributes.m_fogEnd, delta);
                    for (int i = 0; i < FogAttributes.TEXTURE_SIZE; i++) {
                        m_textureColors[i] = Color.Lerp(m_tmpTexture[i], m_selectedTextureColors[i], delta);

                    }
                    m_texture.SetPixels(m_textureColors);
                }
            }
        }

        m_updateTmpTexture = false;

        if (m_updateValues || m_forceUpdate) {
            m_forceUpdate = false;
            m_texture.Apply(false);
            Shader.SetGlobalFloat(GameConstants.Materials.Property.FOG_START, m_start);
            Shader.SetGlobalFloat(GameConstants.Materials.Property.FOG_END, m_end);
            Shader.SetGlobalTexture(GameConstants.Materials.Property.FOG_TEXTURE, m_texture);
        }
    }

    private void SetAsSelectedAttributes() {
        m_start = m_tmpStart = m_selectedAttributes.m_fogStart;
        m_end = m_tmpEnd = m_selectedAttributes.m_fogEnd;
        for (int i = 0; i < FogAttributes.TEXTURE_SIZE; i++) {
            m_tmpTexture[i] = m_selectedAttributes.texture.GetPixel(i, 0);
            m_textureColors[i] = m_tmpTexture[i];
        }
        m_texture.SetPixels(m_textureColors);
    }

    public void ActivateArea(FogArea _area) {
        CheckTextureAvailability(_area.m_attributes);
        if (_area.m_isFireFog) {
            m_activeFireFogAreaList.Add(_area);
        } else {
            m_activeFogAreaList.Add(_area);
        }
    }

    public void CheckTextureAvailability(FogManager.FogAttributes _attributes, bool forceNew = false) {
        if (_attributes.texture == null) {
            if (!forceNew) {
                for (int i = 0; i < m_generatedAttributes.Count; i++) {
                    if (Equals(m_generatedAttributes[i].m_fogGradient, _attributes.m_fogGradient)) {
                        _attributes.m_fogGradient = m_generatedAttributes[i].m_fogGradient;
                        _attributes.texture = m_generatedAttributes[i].texture;
                        break;
                    }
                }
            }

            if (_attributes.texture == null) {
                _attributes.CreateTexture();
                _attributes.RefreshTexture();

                FogAttributes newAttributes = new FogAttributes();
                newAttributes.m_fogGradient = _attributes.m_fogGradient;
                newAttributes.texture = _attributes.texture;
                m_generatedAttributes.Add(newAttributes);

                if (UnityEngine.Debug.isDebugBuild && m_generatedAttributes.Count >= m_maxGradientTextures) {
                    Debug.TaggedLogWarning("Fog Manager", "To many gradient textures");
                }
            }
        }
    }

    public bool Equals(Gradient a, Gradient b) {
        if (a.mode == b.mode && a.alphaKeys.Length == b.alphaKeys.Length && a.colorKeys.Length == b.colorKeys.Length) {
            for (int i = 0; i < a.alphaKeys.Length; i++) {
                if (!a.alphaKeys[i].Equals(b.alphaKeys[i])) {
                    return false;
                }
            }

            for (int i = 0; i < a.colorKeys.Length; i++) {
                if (!a.colorKeys[i].Equals(b.colorKeys[i])) {
                    return false;
                }
            }
        } else {
            return false;
        }
        return true;


    }

    public void DeactivateArea(FogArea _area) {
        if (_area.m_isFireFog) {
            m_activeFireFogAreaList.Remove(_area);
        } else {
            m_activeFogAreaList.Remove(_area);
        }
    }


    void OnDrawGizmosSelected() {
        RefreshFog();
    }

    public void RefreshFog() {
        if (m_defaultAreaFog.texture == null)
            m_defaultAreaFog.CreateTexture();
        m_defaultAreaFog.RefreshTexture();

        if (!Application.isPlaying) {
            m_defaultAreaFog.FogSetup();
        }
    }

    public void ForceAttributes(FogAttributes _attributes) {
        m_forcedAttributes = _attributes;
        m_forceUpdate = true;
        if (m_forcedAttributes != null) {
            CheckTextureAvailability(m_forcedAttributes);
            m_lastSelectedAttributes = m_selectedAttributes = m_forcedAttributes;
            m_transitionTimer = 0;
            SetAsSelectedAttributes();
        }
    }

    private void OnFury(bool _active, DragonBreathBehaviour.Type _type) {
        m_usingFire = _active;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonCorpse : MonoBehaviour {
    //------------------------------------
    private struct SimpleTransform {
        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion localRotation;
    }



    //------------------------------------
    [SerializeField] private float m_fadeDelay = 0.5f;
    [SerializeField] private float m_fadeTime = 1f;
    [SerializeField] private float m_forceExplosion = 175f;
    [SerializeField] private Transform m_view;
    [SerializeField] private ParticleData m_blood;
    [SerializeField] private Transform[] m_bloodPoints;

    private Rigidbody[] m_gibs;
    private List<SimpleTransform> m_originalTransforms;
    private List<Vector3> m_forceDirection;

    private bool m_spawned;
    private float m_time;
    private float m_delay;

    private AttachPoint[] m_attachPoints = new AttachPoint[(int)Equipable.AttachPoint.Count];
    string m_lastDisguiseSku = "";
    private Renderer[] m_renderers = null;
    private List<Material> m_originalMaterials = new List<Material>();
    private List<Material> m_fadeMaterials = new List<Material>();
    //	private Shader m_deathShader;

    // Use this for initialization
    void Awake() {
        m_spawned = false;

        m_originalTransforms = new List<SimpleTransform>();
        m_forceDirection = new List<Vector3>();

        m_gibs = m_view.GetComponentsInChildren<Rigidbody>();

        for (int i = 0; i < m_gibs.Length; i++) {
            SimpleTransform t = new SimpleTransform();
            t.localPosition = m_gibs[i].transform.localPosition;
            t.localScale = m_gibs[i].transform.localScale;
            t.localRotation = m_gibs[i].transform.localRotation;
            m_originalTransforms.Add(t);

            Vector3 dir = m_gibs[i].transform.position - transform.position;
            dir.Normalize();
            m_forceDirection.Add(dir);
        }
        m_blood.CreatePool();
        GetReferences();
    }

    public void GetReferences() {
        // Store attach points sorted to match AttachPoint enum
        AttachPoint[] points = GetComponentsInChildren<AttachPoint>();
        for (int i = 0; i < points.Length; i++) {
            m_attachPoints[(int)points[i].point] = points[i];
        }

        Transform view = transform.Find("view");
        m_originalMaterials.Clear();
        if (view != null) {
            m_renderers = view.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < m_renderers.Length; ++i) {
                if (Application.isPlaying) {
                    m_originalMaterials.Add(m_renderers[i].material);
                } else {
                    m_originalMaterials.Add(m_renderers[i].sharedMaterial);
                }

            }
        }
        //		m_deathShader = Shader.Find("Hungry Dragon/Dragon/Death");
    }



    public static void setDeathMode(Material material) {
        material.SetFloat("_BlendMode", 1);
        material.SetOverrideTag("RenderType", "TransparentCutout");
        material.SetOverrideTag("Queue", "AlphaTest");
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //                material.renderQueue = 3000;
        material.SetFloat("_ZWrite", 1.0f);
        material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        material.EnableKeyword("CUTOFF");
        // SetKeyword(material, kw_doubleSided, true);
        material.DisableKeyword("OPAQUEALPHA");
        material.SetFloat("_EnableCutoff", 1.0f);
        material.SetFloat("_EnableDoublesided", 1.0f);
    }


    void OnDisable() {
        m_spawned = false;
    }

    public void Spawn(bool _hasBoost) {
        if (m_gibs != null) {

            float forceFactor = _hasBoost ? 1.25f : 1f;

            for (int i = 0; i < m_gibs.Length; i++) {
                m_gibs[i].transform.position = Vector3.zero;

                m_gibs[i].transform.localPosition = m_originalTransforms[i].localPosition;
                m_gibs[i].transform.localRotation = m_originalTransforms[i].localRotation;
                m_gibs[i].transform.localScale = m_originalTransforms[i].localScale;

                m_gibs[i].position = m_gibs[i].transform.position;
                m_gibs[i].velocity = Vector3.zero;

                m_gibs[i].AddForce(m_forceDirection[i] * m_forceExplosion * forceFactor, ForceMode.Impulse);
            }

            if (!string.IsNullOrEmpty(m_blood.name) && m_bloodPoints != null) {
                for (int i = 0; i < m_bloodPoints.Length; i++) {
                    GameObject ps = m_blood.Spawn(m_bloodPoints[i].transform.position + m_blood.offset);

                    if (ps != null) {
                        FollowTransform ft = ps.GetComponent<FollowTransform>();
                        if (ft != null) {
                            ft.m_follow = m_bloodPoints[i].transform;
                        }
                    }
                }
            }
        }

        m_time = m_fadeTime;
        m_delay = m_fadeDelay;
        m_spawned = true;

        SetAlpha(1);
    }

    /// <summary>
    /// Equip the disguise with the given sku.
    /// </summary>
    /// <param name="_disguiseSku">The disguise to be equipped.</param>
    public void EquipDisguise(string dragonSku, string disguiseSku) {
        if (!m_lastDisguiseSku.Equals(disguiseSku)) {
            m_lastDisguiseSku = disguiseSku;
            string dragonDisguiseSku = disguiseSku;
            DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, disguiseSku);
            if (def == null) {
                dragonDisguiseSku = dragonSku + "_0";
                def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, dragonDisguiseSku);
                if (def == null) {
                    dragonDisguiseSku = "";
                    return;
                }
            }
            m_fadeMaterials.Clear();
            string skin = def.Get("skin") + "_ingame";
            SetSkin(dragonSku, skin);
            RemoveAccessories();

            // Now body parts!
            List<string> bodyParts = def.GetAsList<string>("body_parts");
            for (int i = 0; i < bodyParts.Count; i++) {
                if (!string.IsNullOrEmpty(bodyParts[i])) {
                    GameObject prefabObj = HDAddressablesManager.Instance.LoadAsset<GameObject>(bodyParts[i]);
                    if (prefabObj != null) {
                        GameObject objInstance = Instantiate<GameObject>(prefabObj);
                        Equipable equipable = objInstance.GetComponent<Equipable>();
                        int attackPointIdx = (int)equipable.attachPoint;
                        if (equipable != null && attackPointIdx < m_attachPoints.Length && m_attachPoints[attackPointIdx] != null) {
                            m_attachPoints[attackPointIdx].EquipAccessory(equipable);
                            if (Application.isPlaying)  // if playing switch to death shaders, if not, in editor, we dont need it
                            {
                                Transform tr = equipable.transform.Find("view");
                                if (tr != null) {
                                    Renderer[] renderers = tr.GetComponentsInChildren<Renderer>();
                                    if (renderers != null && renderers.Length > 0) {
                                        // Get Materials and change shader to wing if necesarry
                                        // Then add those materials to the fade material list
                                        for (int j = 0; j < renderers.Length; ++j) {
                                            Renderer r = renderers[j];
                                            for (int k = 0; k < r.materials.Length; ++k) {
                                                string shaderName = r.materials[k].shader.name;
                                                if (shaderName.Contains("Dragon standard")) {
                                                    //													r.materials[k].shader = m_deathShader;
                                                    setDeathMode(r.materials[k]);

                                                    m_fadeMaterials.Add(r.materials[k]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        } else {
                            if (Application.isPlaying) {
                                Destroy(objInstance);
                            } else {
                                DestroyImmediate(objInstance);
                            }
                        }
                    }
                }
            }

        }
    }


    /// <summary>
    /// Sets the skin of the dragon. Performs the actual texture swap.
    /// </summary>
    /// <param name="_name">Name of the skin to be applied.</param>
    private void SetSkin(string dragonSku, string _name) {
        // Texture change
        if (_name == null || _name.Equals("default") || _name.Equals("")) {
            _name = dragonSku + "_0";       // Default skin, all dragons should have it
        }

        // Not all dragons have wings
        Material wingsMaterial = null;
        Material originalWing = HDAddressablesManager.Instance.LoadAsset<Material>(_name + "_wings");
        if (originalWing)
            wingsMaterial = new Material(originalWing);
        Material bodyMaterial = null;
        Material bodyResource = HDAddressablesManager.Instance.LoadAsset<Material>(_name + "_body");
        if (bodyResource)
            bodyMaterial = new Material(bodyResource);

        if (bodyMaterial == null) {
            bodyMaterial = m_renderers[0].material;
            int max = m_renderers.Length;
            for (int i = 0; i < max; i++) {
                m_renderers[0].material = bodyMaterial;
            }
        }
        if (Application.isPlaying) {
            if (wingsMaterial)
                setDeathMode(wingsMaterial);
            if (bodyMaterial)
                setDeathMode(bodyMaterial);

        }
        if (wingsMaterial)
            m_fadeMaterials.Add(wingsMaterial);
        m_fadeMaterials.Add(bodyMaterial);
        for (int i = 0; i < m_renderers.Length; i++) {
            bool isWings = m_renderers[i].tag == "DragonWings";
            if (isWings) {
                m_renderers[i].material = wingsMaterial;
            } else {
                m_renderers[i].material = bodyMaterial;
            }
        }
    }


    void Update() {
        if (m_spawned) {
            m_delay -= Time.deltaTime;
            if (m_delay <= 0) {
                float a = m_time / m_fadeTime;
                SetAlpha(a);
                m_time -= Time.deltaTime;
                if (m_time <= 0) m_time = 0f;
            }

            for (int i = 0; i < m_gibs.Length; i++) {
                m_gibs[i].AddForce(Vector3.down * 25f);
            }
        }
    }

    void SetAlpha(float alpha) {
        int count = m_fadeMaterials.Count;
        for (int i = 0; i < count; ++i) {
            Color tint = m_fadeMaterials[i].GetColor(GameConstants.Materials.Property.TINT);
            tint.a = alpha;
            m_fadeMaterials[i].SetColor(GameConstants.Materials.Property.TINT, tint);
        }
    }

    public void RemoveAccessories() {
        // Remove old body parts
        for (int i = 0; i < m_attachPoints.Length; i++) {
            if (i > (int)Equipable.AttachPoint.Pet_5 && m_attachPoints[i] != null) {
                m_attachPoints[i].Unequip(true);
            }
        }
    }

    public void CleanSkin() {
        string _name = "dragon_empty";
        Material wingsMaterial = HDAddressablesManager.Instance.LoadAsset<Material>(_name + "_wings");
        Material bodyMaterial = HDAddressablesManager.Instance.LoadAsset<Material>(_name + "_body");

        for (int i = 0; i < m_renderers.Length; i++) {
            bool isWings = m_renderers[i].tag == "DragonWings";
            if (isWings) {
                m_renderers[i].material = wingsMaterial;
            } else {
                m_renderers[i].material = bodyMaterial;
            }
        }
    }
}

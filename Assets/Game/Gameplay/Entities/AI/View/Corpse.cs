using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour {
    //------------------------------------
    private struct SimpleTransform {
        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion localRotation;
    }


    //------------------------------------
    private static Material sm_goldenMaterial = null;


    //------------------------------------
    [SerializeField] private bool m_isDecoration = false;
    [SeparatorAttribute]
    [SerializeField] private float m_fadeDelay = 0.5f;
    [SerializeField] private float m_fadeTime = 1f;
    [SerializeField] private float m_forceExplosion = 175f;
    [SerializeField] private Transform m_view;
    [SerializeField] private ParticleData m_blood;
    [SerializeField] private Transform[] m_bloodPoints;

    private Rigidbody[] m_gibs;
    private Renderer[] m_renderers;
    private List<Material[]> m_rendererMaterials;
    private Dictionary<int, List<Material>> m_materials;
    private List<SimpleTransform> m_originalTransforms;
    private List<Vector3> m_forceDirection;

    private bool m_spawned;
    private float m_time;
    private float m_delay;


    // Use this for initialization
    void Awake() {
        //----------------------------
        if (sm_goldenMaterial == null) sm_goldenMaterial = new Material(Resources.Load("Game/Materials/NPC_GoldenTransparent") as Material);
        //---------------------------- 

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

        m_renderers = m_view.GetComponentsInChildren<Renderer>();
        m_rendererMaterials = new List<Material[]>();
        m_materials = new Dictionary<int, List<Material>>();

        for (int i = 0; i < m_renderers.Length; ++i) {
            Renderer renderer = m_renderers[i];
            Material[] materials = renderer.sharedMaterials;

            m_materials[renderer.GetInstanceID()] = new List<Material>();
            //m_isDecoration
            for (int m = 0; m < materials.Length; ++m) {
                Material mCopy = new Material(materials[m]);
                if (m_isDecoration) {
                    mCopy.EnableKeyword(GameConstants.Materials.Keyword.TINT);
                    mCopy.renderQueue = 3000;
                }
                m_materials[renderer.GetInstanceID()].Add(mCopy);
                materials[m] = null;
            }
            renderer.sharedMaterials = materials;
            m_rendererMaterials.Add(materials);
        }

        m_blood.CreatePool();
    }

    void OnDisable() {
        m_spawned = false;
    }

    public void Spawn(bool _isGold, bool _hasBoost) {
        if (m_gibs != null) {

            float forceFactor = _hasBoost ? 1.25f : 1f;

            for (int i = 0; i < m_gibs.Length; i++) {
                m_gibs[i].transform.position = Vector3.zero;

                m_gibs[i].transform.localPosition = m_originalTransforms[i].localPosition;
                m_gibs[i].transform.localRotation = m_originalTransforms[i].localRotation;
                m_gibs[i].transform.localScale = m_originalTransforms[i].localScale;

                m_gibs[i].position = m_gibs[i].transform.position;
                m_gibs[i].velocity = Vector3.zero;

                //m_gibs[i].AddForce(m_forceDirection[i] * m_forceExplosion * forceFactor, ForceMode.Impulse);
                m_gibs[i].AddForceAtPosition(m_forceDirection[i] * m_forceExplosion * forceFactor, transform.position, ForceMode.Impulse);
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
        for (int i = 0; i < m_renderers.Length; ++i) {
            int id = m_renderers[i].GetInstanceID();
            Material[] materials = m_rendererMaterials[i];
            for (int m = 0; m < materials.Length; m++) {
                if (_isGold) materials[m] = sm_goldenMaterial;
                else materials[m] = m_materials[id][m];
                Color tint = materials[m].GetColor(GameConstants.Materials.Property.TINT);
                tint.a = 1f;
                materials[m].SetColor(GameConstants.Materials.Property.TINT, tint);
            }
            m_renderers[i].materials = materials;
        }

        m_delay = m_fadeDelay;
        m_spawned = true;
    }

    void Update() {
        if (m_spawned) {
            m_delay -= Time.deltaTime;
            if (m_delay <= 0) {
                float a = m_time / m_fadeTime;
                for (int i = 0; i < m_renderers.Length; ++i) {
                    Material[] materials = m_rendererMaterials[i];
                    for (int m = 0; m < materials.Length; m++) {
                        Color tint = materials[m].GetColor(GameConstants.Materials.Property.TINT);
                        tint.a = a;
                        materials[m].SetColor(GameConstants.Materials.Property.TINT, tint);
                    }
                    m_renderers[i].materials = materials;
                }

                m_time -= Time.deltaTime;
                if (m_time <= 0) {
                    m_time = 0f;
                    gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < m_gibs.Length; i++) {
                m_gibs[i].AddForce(Vector3.down * 250f);
            }
        }
    }
}

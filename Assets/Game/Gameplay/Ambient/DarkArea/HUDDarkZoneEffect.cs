using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDDarkZoneEffect : MonoBehaviour {

//    public Shader m_candleEffect;
    public Material m_candleMaterial;

    private MeshRenderer m_blackImage;
//    private Material m_candleMaterial;
    private Material m_oldMaterial;
    private bool m_enableState = false;
    private GameCamera m_gameCamera;
    private DragonPlayer m_dragonPlayer;
    private bool m_goOut;

    private Color m_currentColor;
    private Color m_currentColor2;
    private float m_currentRadius;
    private float m_currentFallOff;

    private static HUDDarkZoneEffect m_instance = null;

    public static void hotInitialize(CandleData candleData)
    {
        if (m_instance != null)
        {

            if (!m_instance.m_enableState)
            {
                m_instance.m_blackImage.material = m_instance.m_candleMaterial;
                m_instance.m_blackImage.enabled = true;
                m_instance.m_enableState = true;
                m_instance.m_currentTrigger = null;
                m_instance.setFireRushMaterials(true);
                m_instance.m_currentColor = candleData.m_Color;
                m_instance.m_currentColor2 = candleData.m_Color2;
                m_instance.m_currentRadius = candleData.m_radius;
                m_instance.m_currentFallOff = candleData.m_fallOff;
//                m_instance.setMaterialParameters(candleData.m_Color, candleData.m_Color2, candleData.m_radius, candleData.m_fallOff);
            }
        }
    }

    [Serializable]
    public class CandleData
    {
        public CandleData(float rad, float fo, Color color, Color color2)
        {
            m_radius = rad;
            m_fallOff = fo;
            m_Color = color;
            m_Color2 = color2;
            IsInside = false;

        }
        public float m_radius;
        public float m_fallOff;
        public Color m_Color;
        public Color m_Color2;
        public bool m_noEffect = false;

        public bool IsInside
        {
            get; set;
        }
    };

    private CandleData m_defaultCandleData = new CandleData(0.5f, 0.5f, Color.black, Color.black);

    private struct matInstanceBackup
    {
        public Material mat;
        public int renderQueue;
    };
    public float m_fireRushMultiplier = 1.0f;


    private List<matInstanceBackup> fireRushMats = null;// new List<Material>();

    // Use this for initialization
    void Start () {
        m_instance = this;
        m_blackImage = GetComponent<MeshRenderer>();
//        m_candleMaterial = new Material(m_candleEffect);
        m_oldMaterial = m_blackImage.material;
        m_blackImage.enabled = false;
        m_enableState = false;

        m_gameCamera = InstanceManager.gameCamera;
        m_dragonPlayer = InstanceManager.player;

//        m_dragonPlayer.IsFuryOn;
//        m_defaultCandleData.IsInside = true;
        m_goOut = false;

    }

    void OnEnable()
    {
        Messenger.AddListener<bool, CandleEffectTrigger>(MessengerEvents.DARK_ZONE_TOGGLE, SetEnable);
    }

    void OnDisable()
    {
        Messenger.RemoveListener<bool, CandleEffectTrigger>(MessengerEvents.DARK_ZONE_TOGGLE, SetEnable);
    }

    private CandleEffectTrigger m_currentTrigger = null;

    void SetEnable(bool enter, CandleEffectTrigger trigger)
    {
//        Debug.Log("Dark zone SetEnable: " + enter + " from: " + m_currentCandle.m_id + " to: " + candleData.m_id + " tick: " + Time.frameCount);
        if (enter)
        {
            m_currentTrigger = trigger;

            if (!m_enableState)
            {
                m_blackImage.material = m_candleMaterial;
                m_blackImage.enabled = true;
                m_enableState = true;
                setFireRushMaterials(true);
            }
        }
        else
        {

            if (m_currentTrigger == trigger)
            {
                CandleData cd = m_goOut ? m_currentTrigger.m_outData : m_currentTrigger.m_inData;
                if (cd.m_noEffect)  // disable dark screen and effect
                {
                    m_blackImage.material = m_oldMaterial;
                    setFireRushMaterials(false);
                    m_blackImage.enabled = false;
                    m_enableState = false;
//                    m_currentTrigger = null;
                }
                m_currentTrigger = null;
            }
        }
    }

    private float m_currentFireRushMultiplier = 1.0f;
    private void setMaterialParameters(Color col1, Color col2, float radius, float falloff)
    {
        float frm = m_dragonPlayer.IsFuryOn() ? m_fireRushMultiplier : 1.0f;
        m_currentFireRushMultiplier = Mathf.Lerp(m_currentFireRushMultiplier, frm, 0.1f);
        m_candleMaterial.SetColor("_Tint", col1);
        m_candleMaterial.SetColor("_Tint2", col2);
        m_candleMaterial.SetFloat("_Radius", Mathf.Clamp(radius * m_currentFireRushMultiplier, -0.1f, 2.0f));
        m_candleMaterial.SetFloat("_FallOff", falloff * m_currentFireRushMultiplier);
    }

    private void setFireRushMaterials(bool enable)
    {
        foreach(matInstanceBackup mb in fireRushMats)
        {
            if (enable)
            {
                mb.mat.renderQueue = mb.renderQueue + 1000;
            }
            else
            {
                mb.mat.renderQueue = mb.renderQueue;
            }
        }
    }

    void Update()
    {
        if (fireRushMats == null)
        {
            FireBreathDynamic[] fireRush = m_dragonPlayer.GetComponentsInChildren<FireBreathDynamic>(true);

            if (fireRush.Length != 0)
            {
                fireRushMats = new List<matInstanceBackup>();
                foreach (FireBreathDynamic fr in fireRush)
                {
                    ParticleSystemRenderer[] particleRenderers = fr.GetComponentsInChildren<ParticleSystemRenderer>(true);
                    foreach (ParticleSystemRenderer psr in particleRenderers)
                    {
                        matInstanceBackup mb = new matInstanceBackup();
                        mb.mat = psr.material;
                        mb.renderQueue = mb.mat.renderQueue;
                        fireRushMats.Add(mb);
                    }
                }
            }
        }

        if (m_enableState)
        {
            if (m_currentTrigger != null)
            {
                CandleData inData = m_currentTrigger.m_inData.m_noEffect ? m_defaultCandleData : m_currentTrigger.m_inData;
                CandleData outData = m_currentTrigger.m_outData.m_noEffect ? m_defaultCandleData : m_currentTrigger.m_outData;

//                Vector3 vd = m_gameCamera.transform.position - m_currentTrigger.transform.position;
                Vector3 vd = m_dragonPlayer.transform.position - m_currentTrigger.transform.position;
                vd.z = 0.0f;
                float sd = Vector3.Dot(m_currentTrigger.Direction, vd) / m_currentTrigger.Length;

                float delta = Mathf.Clamp01(sd);

                m_goOut = delta > 0.5f;

                m_currentColor = Color.Lerp(inData.m_Color, outData.m_Color, delta);
                m_currentColor2 = Color.Lerp(inData.m_Color2, outData.m_Color2, delta);
                m_currentRadius = Mathf.Lerp(inData.m_radius, outData.m_radius, delta);
                m_currentFallOff = Mathf.Lerp(inData.m_fallOff, outData.m_fallOff, delta);

/*
                if (sd > 1.0f)
                {
                    m_currentTrigger = null;
                }
*/
            }
            setMaterialParameters(m_currentColor, m_currentColor2, m_currentRadius, m_currentFallOff);
        }
    }
}

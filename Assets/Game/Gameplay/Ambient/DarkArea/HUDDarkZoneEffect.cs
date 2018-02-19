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
    private Camera m_camera;

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

    // Use this for initialization
    void Start () {
        m_blackImage = GetComponent<MeshRenderer>();
//        m_candleMaterial = new Material(m_candleEffect);
        m_oldMaterial = m_blackImage.material;
        m_blackImage.enabled = false;
        m_enableState = false;

        m_gameCamera = InstanceManager.gameCamera;
        m_camera = m_gameCamera.gameObject.GetComponent<Camera>();
//        m_defaultCandleData.IsInside = true;
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
            }
        }
    }


    void Update()
    {
/*
        if (m_activate != m_oldActivate)
        {
            SetEnable(m_activate);
            m_oldActivate = m_activate;
        }
*/
        if (m_enableState)
        {
            if (m_currentTrigger != null)
            {
                CandleData inData = m_currentTrigger.m_inData.m_noEffect ? m_defaultCandleData : m_currentTrigger.m_inData;
                CandleData outData = m_currentTrigger.m_outData.m_noEffect ? m_defaultCandleData : m_currentTrigger.m_outData;

                Vector3 vd = m_camera.transform.position - m_currentTrigger.transform.position;
                vd.z = 0.0f;
                float sd = Vector3.Dot(m_currentTrigger.Direction, vd) / m_currentTrigger.SqrLength;

                float delta = Mathf.Clamp01(sd);

                Color color = Color.Lerp(inData.m_Color, outData.m_Color, delta);
                Color color2 = Color.Lerp(inData.m_Color2, outData.m_Color2, delta);
                float radius = Mathf.Lerp(inData.m_radius, outData.m_radius, delta);
                float falloff = Mathf.Lerp(inData.m_fallOff, outData.m_fallOff, delta);
                m_candleMaterial.SetColor("_Tint", color);
                m_candleMaterial.SetColor("_Tint2", color2);
                m_candleMaterial.SetFloat("_Radius", radius);
                m_candleMaterial.SetFloat("_FallOff", falloff);
/*
                if (sd > 1.0f)
                {
                    m_currentTrigger = null;
                }
*/
            }
        }
    }
}

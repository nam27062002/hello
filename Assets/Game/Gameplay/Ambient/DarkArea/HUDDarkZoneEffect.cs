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
                Vector3 vd = m_camera.transform.position - m_currentTrigger.transform.position;
                vd.z = 0.0f;
                float sd = Vector3.Dot(m_currentTrigger.Direction, vd) / m_currentTrigger.SqrLength;

                float delta = Mathf.Clamp01(sd);

                Color color = Color.Lerp(m_currentTrigger.m_inData.m_Color, m_currentTrigger.m_outData.m_Color, delta);
                Color color2 = Color.Lerp(m_currentTrigger.m_inData.m_Color2, m_currentTrigger.m_outData.m_Color2, delta);
                float radius = Mathf.Lerp(m_currentTrigger.m_inData.m_radius, m_currentTrigger.m_outData.m_radius, delta);
                float falloff = Mathf.Lerp(m_currentTrigger.m_inData.m_fallOff, m_currentTrigger.m_outData.m_fallOff, delta);
                m_candleMaterial.SetColor("_Tint", color);
                m_candleMaterial.SetColor("_Tint2", color2);
                m_candleMaterial.SetFloat("_Radius", radius);
                m_candleMaterial.SetFloat("_FallOff", falloff);


            }
            /*
                        if (Time.time < m_transitionTime)
                        {
                            float delta = (m_transitionTime - Time.time) / m_nextCandle.m_time;
                            Color color = Color.Lerp(m_nextCandle.m_Color, m_currentCandle.m_Color, delta);
                            Color color2 = Color.Lerp(m_nextCandle.m_Color2, m_currentCandle.m_Color2, delta);
                            float radius = Mathf.Lerp(m_nextCandle.m_radius, m_currentCandle.m_radius, delta);
                            float falloff = Mathf.Lerp(m_nextCandle.m_fallOff, m_currentCandle.m_fallOff, delta);
                            m_candleMaterial.SetColor("_Tint", color);
                            m_candleMaterial.SetColor("_Tint2", color2);
                            m_candleMaterial.SetFloat("_Radius", radius);
                            m_candleMaterial.SetFloat("_FallOff", falloff);
            //                Debug.Log("Transition from " + m_currentCandle.m_id + " to " + m_nextCandle.m_id + " time: " + delta + " tick: " + Time.frameCount);
                        }
                        else
                        {
                            if (m_nextCandle.m_id == 0)
                            {
                                m_blackImage.material = m_oldMaterial;
                                m_blackImage.enabled = false;
                                m_enableState = false;
            //                    Debug.Log("Disabling candle: " + m_currentCandle.m_id + " to " + m_nextCandle.m_id + " tick: " + Time.frameCount);
                            }
                            else
                            {
            //                    Debug.Log("Current candle: " + m_currentCandle.m_id + " to " + m_nextCandle.m_id + " tick: " + Time.frameCount);
                            }
                            m_currentCandle = m_nextCandle;
                        }
            */
        }
    }
}

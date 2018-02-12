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
        public CandleData(int id, float rad, float fo, Color color, Color color2, float time)
        {
            m_id = id;
            m_radius = rad;
            m_fallOff = fo;
            m_Color = color;
            m_Color2 = color2;
            m_time = time;
            IsInside = false;

        }
        public int m_id;
        public float m_radius;
        public float m_fallOff;
        public Color m_Color;
        public Color m_Color2;
        public float m_time;

        public bool IsInside
        {
            get; set;
        }
    };

    private List<CandleData> m_candleList = new List<CandleData>();

    private CandleData m_defaultCandleData = new CandleData(0, 0.5f, 0.5f, Color.black, Color.black, 2.0f);

    // Use this for initialization
    void Start () {
        m_blackImage = GetComponent<MeshRenderer>();
//        m_candleMaterial = new Material(m_candleEffect);
        m_oldMaterial = m_blackImage.material;
        m_blackImage.enabled = false;
        m_enableState = false;

        m_gameCamera = InstanceManager.gameCamera;
        m_camera = m_gameCamera.gameObject.GetComponent<Camera>();
        m_candleList.Clear();
        register(m_defaultCandleData);
        m_currentCandle = m_defaultCandleData;
        m_defaultCandleData.IsInside = true;
    }

    void OnEnable()
    {
        Messenger.AddListener<bool, CandleData>(MessengerEvents.DARK_ZONE_TOGGLE, SetEnable);
    }

    void OnDisable()
    {
        Messenger.RemoveListener<bool, CandleData>(MessengerEvents.DARK_ZONE_TOGGLE, SetEnable);
    }

    private CandleData m_currentCandle = null;
    private CandleData m_nextCandle = null;
    private float m_transitionTime = 0.0f;

    private int find(int id)
    {
        if (m_candleList.Count > 0)
        {
            for (int c = 0; c < m_candleList.Count; c++)
            {
                if (m_candleList[c].m_id == id) return c;
            }
        }
        return -1;
    }

    private int register(CandleData candleData)
    {
        while (candleData.m_id >= m_candleList.Count)
        {
            m_candleList.Add(default(CandleData));
        }
        if (m_candleList[candleData.m_id] == null)
        {
            m_candleList[candleData.m_id] = candleData;
        }
        return candleData.m_id;
    }

    void SetEnable(bool enter, CandleData candleData)
    {

        if (enter)
        {
            candleData.IsInside = true;
            register(candleData);

            if (m_currentCandle.m_id > candleData.m_id)
            {
                return;
            }

            if (m_nextCandle == null || candleData.m_id != m_nextCandle.m_id)
            {
                m_nextCandle = candleData;
                m_transitionTime = Time.time + candleData.m_time;
            }

            if (!m_enableState)
            {
                m_blackImage.material = m_candleMaterial;
                m_blackImage.enabled = true;
                m_enableState = true;
            }
        }
        else
        {
            candleData.IsInside = false;
            for(int c = candleData.m_id - 1; c >= 0; c--)
            {
                if (m_candleList[c] != null && m_candleList[c].IsInside)
                {
                    if (m_candleList[c].m_id != m_nextCandle.m_id)
                    {
                        m_nextCandle = m_candleList[c];
                        m_transitionTime = Time.time + m_nextCandle.m_time;
                    }
                    break;
                }
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
            //        Debug.Log("track ahead vector: " + m_gameCamera.m_trackAheadVector);
            Vector3 offset = m_camera.WorldToViewportPoint(InstanceManager.player.transform.position);
            m_candleMaterial.SetVector("_Offset", offset);

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
            }
            else
            {
                if (m_nextCandle.m_id == 0)
                {
                    m_blackImage.material = m_oldMaterial;
                    m_blackImage.enabled = false;
                    m_enableState = false;
                }
                m_currentCandle = m_nextCandle;
            }
        }
    }
}

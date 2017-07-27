using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDDarkZoneEffect : MonoBehaviour {

//    public Shader m_candleEffect;
    public Material m_candleMaterial;

    private Image m_blackImage;
//    private Material m_candleMaterial;
    private Material m_oldMaterial;
    private bool m_enableState = false;
    private GameCamera m_gameCamera;
    private Camera m_camera;

    // Use this for initialization
    void Start () {
        m_blackImage = GetComponent<Image>();
//        m_candleMaterial = new Material(m_candleEffect);
        m_oldMaterial = m_blackImage.material;
        SetEnable(true);

        m_gameCamera = InstanceManager.gameCamera;
        m_camera = m_gameCamera.gameObject.GetComponent<Camera>();
    }

    void SetEnable(bool value)
    {
        if (value != m_enableState)
        {
            m_blackImage.enabled = value;

            if (value)
            {
                m_blackImage.material = m_candleMaterial;
            }
            else
            {
                m_blackImage.material = m_oldMaterial;
            }
            m_enableState = value;
        }
    }


    void Update()
    {
        if (m_enableState)
        {
            //        Debug.Log("track ahead vector: " + m_gameCamera.m_trackAheadVector);
            Vector3 offset = m_camera.WorldToViewportPoint(InstanceManager.player.transform.position);
            m_candleMaterial.SetVector("_Offset", offset);
        }
    }
}

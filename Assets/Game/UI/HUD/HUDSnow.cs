using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HUDSnow : MonoBehaviour {

    private int m_wposId;
    private int m_aspectId;

	private Material m_snowMaterial = null;
	private Image m_image = null;

    private static HUDSnow m_instance = null;
    public bool m_enable = true;

    public float m_fadeTime = 2.0f;
    private bool m_enable_bk = true;

    // Use this for initialization
    void Awake () 
	{
        if (FeatureSettingsManager.instance.Device_CurrentProfile == "very_low")
        {
            Destroy(gameObject);
            return;
        }
        m_instance = this;

		m_image = gameObject.GetComponent<Image>();
			
        m_wposId = Shader.PropertyToID("_WorldPosition");
        m_aspectId = Shader.PropertyToID("_Aspect");
		m_snowMaterial = m_image.material;
	}
	
	// Update is called once per frame

    public static void enableSnow(bool enable)
    {
        m_instance.m_enable = enable;
        m_instance.m_image.enabled = true;
    }


	void Update()
	{
		if (InstanceManager.gameCamera != null) {
			Vector3 pos = InstanceManager.gameCamera.transform.position;
			m_snowMaterial.SetVector(m_wposId, pos);
		}
        float aspect = (float)Screen.height / (float)Screen.width;
        m_snowMaterial.SetFloat(m_aspectId, aspect);

        if (m_enable != m_enable_bk)
        {
            Color col = m_image.color;

            if (m_enable)
            {
                col.a += Time.deltaTime / m_fadeTime;
                if (col.a > 1.0f)
                {
                    col.a = 1.0f;
                    m_enable_bk = m_enable;
                }
            }
            else
            {
                col.a -= Time.deltaTime / m_fadeTime;
                if (col.a < 0.0f)
                {
                    col.a = 0.0f;
                    m_image.enabled = false;
                    m_enable_bk = m_enable;
                }
            }

            m_image.color = col;
        }

        //Debug.Log("Device_CurrentProfile: " + FeatureSettingsManager.instance.Device_CurrentProfile);

    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HUDSnow : MonoBehaviour {

	public bool m_snowActivated = true;

    private int m_wposId;
    private int m_aspectId;

	private Material m_snowMaterial = null;
	private Image m_image = null;


    // Use this for initialization
    void Awake () 
	{
        if (FeatureSettingsManager.instance.Device_CurrentProfile == "very_low")
        {
            Destroy(gameObject);
            return;
        }

            // Remove if we shouw fireworks
        bool fireworks = SeasonManager.IsFireworksDay();
        if ( fireworks )
        {
            Destroy( gameObject );
            return;
        }

		m_image = gameObject.GetComponent<Image>();
			
        m_wposId = Shader.PropertyToID("_WorldPosition");
        m_aspectId = Shader.PropertyToID("_Aspect");
		m_snowMaterial = m_image.material;
	}
	
	// Update is called once per frame




	void Update()
	{
        if (m_snowActivated)
        {
			if (InstanceManager.gameCamera != null) {
				Vector3 pos = InstanceManager.gameCamera.transform.position;
				m_snowMaterial.SetVector(m_wposId, pos);
			}
            float aspect = (float)Screen.height / (float)Screen.width;
            m_snowMaterial.SetFloat(m_aspectId, aspect);

        }

        //Debug.Log("Device_CurrentProfile: " + FeatureSettingsManager.instance.Device_CurrentProfile);
			
	}


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InstantiateByDateTime : MonoBehaviour {

    public string m_resourcePath = "";
    public bool m_useLeveledParticle = false;
    public bool m_isNewYear = false;
    public bool m_isChineseNewYear = false;

	// Use this for initialization
	void Awake () 
    {
        bool instantiate = false;
        if ( (m_isNewYear && SeasonManager.IsNewYear()) 
            || (m_isChineseNewYear && SeasonManager.IsChineseNewYear())
            ||  DebugSettings.specialDates
        )
        {
            instantiate = true;
        }
        
        if ( instantiate )
        {
            if ( m_useLeveledParticle )
            {
                ParticleSystem ps = ParticleManager.InitLeveledParticle(m_resourcePath, transform);
                if( ps != null )
                {
                    ps.gameObject.SetActive(true);
                    ps.Play();
                }
            }
            else
            {
                GameObject go = Resources.Load<GameObject>(m_resourcePath);
                if (go != null)
                {
                    Instantiate<GameObject>(go, transform);
                }
            }
        }
    }
}

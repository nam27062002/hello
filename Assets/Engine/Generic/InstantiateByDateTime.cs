using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InstantiateByDateTime : MonoBehaviour {

    public string m_resourcePath = "";
    public bool m_useLeveledParticle = false;
    
    [Serializable]
    public struct DayOfTheYear
    {
        [Range (1,31)]
        public int day;
        
        [Range (1,12)]
        public int month;
    };
    
    public List<DayOfTheYear> m_validDays = new List<DayOfTheYear>();

	// Use this for initialization
	void Awake () 
    {
        System.DateTime dateTime = System.DateTime.Now;
        
        bool instantiate = false;
        
        instantiate = Prefs.GetBoolPlayer(DebugSettings.SPECIAL_DATES, false);
        
        int max = m_validDays.Count;
        for (int i = 0; i < max && !instantiate; i++)
        {
            instantiate = m_validDays[i].day == dateTime.Day && m_validDays[i].month == dateTime.Month;
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

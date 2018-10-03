using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (Localizer))]
public class RandomTid : MonoBehaviour {

	public List<string> m_tids;

    private string[] m_socialTids = new string[]
    {
        "TID_TIP_LOADING_FACEBOOK",
        "TID_TIP_LOADING_CLOUDSAVING_02"
    };

    private List<string> m_tidsEnabled;

	public void OnEnable()
	{
		if ( m_tids.Count > 0 )
		{
            List<string> eligibleTids = new List<string>();

            // Tids are filtered out since some values in m_tids might not be eligible
            bool socialIsEnabled = SocialPlatformManager.SharedInstance.GetIsEnabled();
            int count = m_tids.Count;
            for (int i = 0; i < count; i++)
            {
                // If social platform is disabled then no social tips should be given
                if (socialIsEnabled || m_socialTids.IndexOf(m_tids[i]) == -1)                
                {
                    eligibleTids.Add(m_tids[i]);
                }                
            }            

            string newTid = eligibleTids[ Random.Range( 0, eligibleTids.Count)];
            string[] parameters = null;
            // Social tids need the social platform to be set as a parameter
            if (m_socialTids.IndexOf(newTid) > -1)
            {
                parameters = new string[] { SocialPlatformManager.SharedInstance.GetPlatformName() };
            }

            GetComponent<Localizer>().Localize( newTid, parameters );
		}
	}
}

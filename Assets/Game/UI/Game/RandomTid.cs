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

    private string m_PlatformName = null;

    /// <summary>
    /// Returns the name of the social platform. If several platforms are supported then only the name of the first supported platform is returned.
    /// TODO: We should create a specific TID containing all platforms supported
    /// </summary>
    private string GetPlatformName()
    {
        if (string.IsNullOrEmpty(m_PlatformName))
        {
            SocialPlatformManager manager = SocialPlatformManager.SharedInstance;
            if (manager.GetIsEnabled())
            {
                List<SocialUtils.EPlatform> platformIds = manager.GetSupportedPlatformIds();
                if (platformIds.Count > 0)
                {
                    m_PlatformName = manager.GetPlatformName(platformIds[0]);
                }
            }
        }

        return m_PlatformName;
    }

    public void OnEnable()
	{
		if ( m_tids.Count > 0 )
		{
            List<string> eligibleTids = new List<string>();

            // Tids are filtered out since some values in m_tids might not be eligible
            string platformName = GetPlatformName();
            bool socialIsEnabled = SocialPlatformManager.SharedInstance.GetIsEnabled() && !string.IsNullOrEmpty(platformName);
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
                parameters = new string[] { platformName };
            }

            GetComponent<Localizer>().Localize( newTid, parameters );
		}
	}
}

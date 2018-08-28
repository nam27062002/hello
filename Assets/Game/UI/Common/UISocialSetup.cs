using UnityEngine;

/// <summary>
/// Used to setup UI stuff depending on the social platform in use
/// </summary>
public class UISocialSetup : MonoBehaviour
{
    public GameObject m_fbItem;
    public GameObject m_weiboItem;
	
	void Awake ()
    {
        // Enables only what needs to be enabled
        SocialUtils.EPlatform platform = SocialPlatformManager.SharedInstance.GetPlatform();

        if (m_fbItem != null)
        {
            m_fbItem.SetActive(platform == SocialUtils.EPlatform.Facebook);
        }

        if (m_weiboItem != null)
        {
            m_weiboItem.SetActive(platform == SocialUtils.EPlatform.Weibo);
        }        
	}		
}

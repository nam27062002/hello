using UnityEngine;

public class GlobalEventsNoActiveEventScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject m_socialRoot = null;
    
    [SerializeField]
    private GameObject m_socialFbButton = null;

    [SerializeField]
    private GameObject m_socialWeiboButton = null;

    [SerializeField]
    private GameObject m_socialTwitterButton = null;

    [SerializeField]
    private GameObject m_socialInstagramButton = null;

    void Start ()
    {
        // Enable the social buttons according to the current flavour configuration
        Flavour flavour = FlavourManager.Instance.GetCurrentFlavour();
        
        SetActive(m_socialWeiboButton, flavour.SocialPlatformASSocialUtilsEPlatform == SocialUtils.EPlatform.Weibo);
        SetActive(m_socialFbButton, flavour.SocialPlatformASSocialUtilsEPlatform == SocialUtils.EPlatform.Facebook);
        SetActive(m_socialTwitterButton, flavour.GetSetting<bool>(Flavour.SettingKey.TWITTER_ALLOWED));
        SetActive(m_socialInstagramButton, flavour.GetSetting<bool>(Flavour.SettingKey.INSTAGRAM_ALLOWED));

        // All the social stuff is hidden for kids
        SetActive(m_socialRoot, SocialPlatformManager.SharedInstance.GetIsEnabled());
    }

    private void SetActive(GameObject go, bool value)
    {
        if (go != null)
        {
            go.SetActive(value);
        }
    }
}

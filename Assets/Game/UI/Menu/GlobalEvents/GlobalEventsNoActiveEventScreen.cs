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
        bool isChina = PlatformUtils.Instance.IsChina();
        
        SetActive(m_socialWeiboButton, isChina);
        SetActive(m_socialFbButton, !isChina);
        SetActive(m_socialTwitterButton, !isChina);
        SetActive(m_socialInstagramButton, !isChina);

        // All the social stuff is hidden for kids
        SocialUtils.EPlatform platform = SocialPlatformManager.SharedInstance.GetPlatform();
        SetActive(m_socialRoot, platform != SocialUtils.EPlatform.None);
    }

    private void SetActive(GameObject go, bool value)
    {
        if (go != null)
        {
            go.SetActive(value);
        }
    }
}

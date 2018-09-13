using UnityEngine;

/// <summary>
/// Used to setup UI stuff depending on the social platform in use
/// </summary>
public class UISocialSetup : MonoBehaviour
{
    public GameObject m_fbItem = null;
	public GameObject m_twitterItem = null;
	public GameObject m_instagramItem = null;
	public GameObject m_webItem = null;

    public GameObject m_weiboItem = null;
	public GameObject m_weChatItem = null;

	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
        // Enables only what needs to be enabled

		// Aux vars
		SocialUtils.EPlatform socialPlatform = SocialPlatformManager.SharedInstance.GetPlatform();
		bool isChina = PlatformUtils.Instance.IsChina();

		Toggle(m_fbItem, socialPlatform == SocialUtils.EPlatform.Facebook);
		Toggle(m_twitterItem, !isChina);
		Toggle(m_instagramItem, !isChina);

		Toggle(m_webItem, true);    // So far website is enabled everywhere :P

		Toggle(m_weiboItem, socialPlatform == SocialUtils.EPlatform.Weibo);
		Toggle(m_weChatItem, isChina && !string.IsNullOrEmpty(GameSettings.WE_CHAT_URL));	// Hide it while the URL is not defined
	}		

	/// <summary>
	/// Toggle target object on/off if valid.
	/// </summary>
	/// <param name="_obj">Object. Can be null.</param>
	/// <param name="_active">Toggle target object on or off?</param>
	private void Toggle(GameObject _obj, bool _active) {
		if(_obj != null) _obj.SetActive(_active);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The Facebook button has been pressed.
	/// </summary>
	public void OnFacebookButton() {
		GameSettings.OpenUrl(GameSettings.FACEBOOK_URL);
	}

	/// <summary>
	/// The Twitter button has been pressed.
	/// </summary>
	public void OnTwitterButton() {
		GameSettings.OpenUrl(GameSettings.TWITTER_URL);
	}

	/// <summary>
	/// The Instagram button has been pressed.
	/// </summary>
	public void OnInstagramButton() {
		GameSettings.OpenUrl(GameSettings.INSTAGRAM_URL);
	}

	/// <summary>
	/// The Web button has been pressed.
	/// </summary>
	public void OnWebButton() {
		GameSettings.OpenUrl(GameSettings.WEB_URL);
	}

	/// <summary>
	/// The Weibo button has been pressed.
	/// </summary>
	public void OnWeiboButton() {
		GameSettings.OpenUrl(GameSettings.WEIBO_URL);
	}

	/// <summary>
	/// The WeChat button has been pressed.
	/// </summary>
	public void OnWeChatButton() {
		GameSettings.OpenUrl(GameSettings.WE_CHAT_URL);
	}
}

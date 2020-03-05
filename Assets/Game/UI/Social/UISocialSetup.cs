using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Used to setup UI stuff depending on the social platform in use
/// </summary>
public class UISocialSetup : MonoBehaviour
{
	public enum SocialPlatformMode {
		ALL,
		ALL_SUPPORTED,
		LOGGED_IN
	}

	public SocialPlatformMode m_socialPlatformMode = SocialPlatformMode.ALL_SUPPORTED;
	[Space]
    public GameObject m_fbItem = null;
	public GameObject m_twitterItem = null;
	public GameObject m_instagramItem = null;
	public GameObject m_webItem = null;
	[Space]
    public GameObject m_weiboItem = null;
	public GameObject m_weChatItem = null;
	[Space]
	public GameObject m_appleItem = null;

	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
		Refresh();
	}

	/// <summary>
	/// Toggle what needed given current environment: social platform, country and age.
	/// </summary>
	public void Refresh() {
		// Enables only what needs to be enabled
		// Aux vars
		bool isChina = PlatformUtils.Instance.IsChina();
		bool isUnderage = GDPRManager.SharedInstance.IsAgeRestrictionEnabled(); // Don't show social platforms or external links to underage players!
		
		// Social platforms
		switch(m_socialPlatformMode) {
			case SocialPlatformMode.ALL: {
				Toggle(m_fbItem, !isUnderage);
				Toggle(m_weiboItem, !isUnderage);
				Toggle(m_appleItem, true);  // Apple is allowed for underaged players
			} break;

			case SocialPlatformMode.ALL_SUPPORTED: {				
				List<SocialUtils.EPlatform> supportedSocialPlatforms = SocialPlatformManager.SharedInstance.GetSupportedPlatformIds();				
				Toggle(m_fbItem, supportedSocialPlatforms.Contains(SocialUtils.EPlatform.Facebook) && !isUnderage);
				Toggle(m_weiboItem, supportedSocialPlatforms.Contains(SocialUtils.EPlatform.Weibo) && !isUnderage);
				Toggle(m_appleItem, supportedSocialPlatforms.Contains(SocialUtils.EPlatform.SIWA) && !isUnderage);	
			} break;

			case SocialPlatformMode.LOGGED_IN: {
				SocialUtils.EPlatform loggedInSocialPlatform = SocialPlatformManager.SharedInstance.CurrentPlatform_GetId();

				Toggle(m_fbItem, loggedInSocialPlatform == SocialUtils.EPlatform.Facebook && !isUnderage);
				Toggle(m_weiboItem, loggedInSocialPlatform == SocialUtils.EPlatform.Weibo && !isUnderage);
				Toggle(m_appleItem, loggedInSocialPlatform == SocialUtils.EPlatform.SIWA && !isUnderage);	
			} break;
		}
		

		// Global items
		Toggle(m_webItem, !isUnderage);

		// Western items
		Toggle(m_twitterItem, !isChina && !isUnderage);
		Toggle(m_instagramItem, !isChina && !isUnderage);

		// China items
		Toggle(m_weChatItem, isChina && !string.IsNullOrEmpty(GameSettings.WE_CHAT_URL) && !isUnderage);    // Hide it while the URL is not defined
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
        HDTrackingManager.Instance.Notify_SocialClick("Facebook", InstanceManager.menuSceneController.currentScreen.ToString());
		GameSettings.OpenUrl(GameSettings.FACEBOOK_URL);
	}

	/// <summary>
	/// The Twitter button has been pressed.
	/// </summary>
	public void OnTwitterButton() {
        HDTrackingManager.Instance.Notify_SocialClick("Twitter", InstanceManager.menuSceneController.currentScreen.ToString());
		GameSettings.OpenUrl(GameSettings.TWITTER_URL);
	}

	/// <summary>
	/// The Instagram button has been pressed.
	/// </summary>
	public void OnInstagramButton() {
        HDTrackingManager.Instance.Notify_SocialClick("Instagram", InstanceManager.menuSceneController.currentScreen.ToString());
		GameSettings.OpenUrl(GameSettings.INSTAGRAM_URL);
	}

	/// <summary>
	/// The Web button has been pressed.
	/// </summary>
	public void OnWebButton() {
        HDTrackingManager.Instance.Notify_SocialClick("Webpage", InstanceManager.menuSceneController.currentScreen.ToString());
		GameSettings.OpenUrl(GameSettings.WEB_URL);
	}

	/// <summary>
	/// The Weibo button has been pressed.
	/// </summary>
	public void OnWeiboButton() {
        HDTrackingManager.Instance.Notify_SocialClick("Weibo", InstanceManager.menuSceneController.currentScreen.ToString());
		GameSettings.OpenUrl(GameSettings.WEIBO_URL);
	}

	/// <summary>
	/// The WeChat button has been pressed.
	/// </summary>
	public void OnWeChatButton() {
        HDTrackingManager.Instance.Notify_SocialClick("Wechat", InstanceManager.menuSceneController.currentScreen.ToString());
		GameSettings.OpenUrl(GameSettings.WE_CHAT_URL);
	}
}

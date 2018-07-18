// PopupSettings.cs
//
// Created by Alger Ortín Castellví on 11/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Pause popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupSettings : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupSettings";

    public const string KEY_SETTINGS_LANGUAGE = "SETTINGS_LANGUAGE";

    [SerializeField]
    private GameObject m_saveTab;

	[SerializeField]
    private GameObject m_3dTouch;

	[SerializeField] private Localizer m_versionText = null;
	[SerializeField] private Localizer m_userIdText = null;

    void Awake()
    {
        if (m_saveTab != null)
        {
            m_saveTab.SetActive(SocialPlatformManager.SharedInstance.GetIsEnabled());            
        }
		if (m_3dTouch != null)
		{
			// m_3dTouch.SetActive( Input.touchPressureSupported );
			m_3dTouch.SetActive( PlatformUtils.Instance.InputPressureSupprted());
		}
        CS_Init();

		// Set version number
		m_versionText.Localize(m_versionText.tid, GameSettings.internalVersion.ToString() + " ("+ ServerManager.SharedInstance.GetRevisionVersion() +")");

		// Subscribe to popup events
		PopupController controller = GetComponent<PopupController>();
		controller.OnOpenPreAnimation.AddListener(OnOpenPreAnimation);
		controller.OnOpenPostAnimation.AddListener( OnOpenAnimation );
		controller.OnClosePreAnimation.AddListener( OnCloseAnimation );
    }

	private void OnOpenPreAnimation() {
		// Refresh user ID (might have changed from the last time we opened the popup, so refresh it every time
		string uid = GameSessionManager.SharedInstance.GetUID();
		m_userIdText.gameObject.SetActive(!string.IsNullOrEmpty(uid));	// Dont show if id not initialized (we never ever did a successful auth)
		m_userIdText.Localize(m_userIdText.tid, uid);
	}

    private void OnOpenAnimation()
    {
    	HDTrackingManager.Instance.Notify_SettingsOpen();
    }

    private void OnCloseAnimation()
    {
		HDTrackingManager.Instance.Notify_SettingsClose();
    }

	public void CS_Init()
	{

		string country = "es";
		if (
			ServerManager.SharedInstance.GetServerAuthBConfig() != null &&
			ServerManager.SharedInstance.GetServerAuthBConfig()["country"] != null)
		{
			country = ServerManager.SharedInstance.GetServerAuthBConfig()["country"].ToString().Replace("\"", "");
		}

		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");

		if (settingsInstance != null)
		{

			CSTSManager.ECSTSEnvironment kEnv = CSTSManager.ECSTSEnvironment.E_CSTS_DEV;
			if (settingsInstance.m_iBuildEnvironmentSelected == (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION)
			{
				kEnv = CSTSManager.ECSTSEnvironment.E_CSTS_PROD;
				Debug.LogError("init CALETY");
			}

			CSTSManager.CSTSConfig kCSTSConfig = new CSTSManager.CSTSConfig();
			kCSTSConfig.m_eEnvironment = kEnv;
			kCSTSConfig.m_strCSTSId = "92192eadf22f6aafe6fadd926945ae60";// "cd6a617edf97d768067ac38e295f651c";
			kCSTSConfig.m_strInGamePlayerID = GameSessionManager.SharedInstance.GetUID();
			kCSTSConfig.m_strCountry = country;
			kCSTSConfig.m_bIsAutoDestroyable = true;
			kCSTSConfig.m_bUseNavigationBar = true;
			kCSTSConfig.m_kViewRect = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

			CSTSManager.SharedInstance.Initialise(kCSTSConfig);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Dragon info button has been pressed.
	/// </summary>
	public void OnDragonInfoButton() {
		// Tracking
		string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupDragonInfo.PATH);
		HDTrackingManager.Instance.Notify_InfoPopup(popupName, "settings");

		// Open the dragon info popup and initialize it with the current dragon's data
		PopupDragonInfo popup = PopupManager.OpenPopupInstant(PopupDragonInfo.PATH).GetComponent<PopupDragonInfo>();
		popup.Init(DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon));
	}

	/// <summary>
	/// The credits button has been pressed.
	/// </summary>
	public void OnCreditsButton() {
		// Open the credits popup
		PopupManager.OpenPopupInstant(PopupCredits.PATH);
	}

	/// <summary>
	/// The privacy settings button has been pressed.
	/// </summary>
	public void OnPrivacySettingsButton() {
		PopupController popupController = PopupManager.LoadPopup(PopupTermsAndConditions.PATH);
		popupController.GetComponent<PopupTermsAndConditions>().Init(PopupTermsAndConditions.Mode.MANUAL);
		popupController.Open();
	}

	public void OnCommentsButton(){
		MiscUtils.SendFeedbackEmail();
	}

	public void OpenCustomerSupport()
    {
        //CSTSManager.SharedInstance.OpenView(TranslationsManager.Instance.ISO.ToString(), PersistenceManager.Instance.IsPayer);
		if (Application.internetReachability != NetworkReachability.NotReachable)
		{
			CSTSManager.SharedInstance.OpenView(LocalizationManager.SharedInstance.Culture.Name, false);	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.;
	        HDTrackingManager.Instance.Notify_CustomerSupportRequested();
        }
        else
        {
			string str = LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION");
        	UIFeedbackText.CreateAndLaunch(str, new Vector2(0.5f, 0.5f), GetComponentInParent<Canvas>().transform as RectTransform);
        }
    }

	public void OnBackButton()
	{
		if ( !CSTSManager.SharedInstance.IsViewOpened() )
		{
			GetComponent<PopupController>().Close(false);
		}
	}
}

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

    void Awake()
    {
        if (m_saveTab != null)
        {
#if CLOUD_SAVE && (WEIBO || FACEBOOK)
            m_saveTab.SetActive(true);
#else
            m_saveTab.SetActive(false);
#endif
        }
		if (m_3dTouch != null)
		{
			m_3dTouch.SetActive( Input.touchPressureSupported );
		}
        CS_Init();
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
}

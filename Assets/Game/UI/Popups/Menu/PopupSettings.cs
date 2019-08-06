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
            m_saveTab.SetActive(SocialPlatformManager.SharedInstance.GetIsEnabled());            
        }
		if (m_3dTouch != null)
		{
			// m_3dTouch.SetActive( Input.touchPressureSupported );
			m_3dTouch.SetActive( PlatformUtils.Instance.InputPressureSupprted());
		}
        
		// Init Customer Support stuff
		CS_Init();

		// Subscribe to popup events
		PopupController controller = GetComponent<PopupController>();
		controller.OnOpenPreAnimation.AddListener(OnOpenPreAnimation);
		controller.OnOpenPostAnimation.AddListener( OnOpenAnimation );
		controller.OnClosePreAnimation.AddListener( OnCloseAnimation );
    }

	private void OnOpenPreAnimation() {
		
	}

    private void OnOpenAnimation()
    {
        string zone = InstanceManager.menuSceneController.currentScreen.ToString();
    	HDTrackingManager.Instance.Notify_SettingsOpen(zone);
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

                if (FeatureSettingsManager.IsDebugEnabled)
                    Debug.LogError("init CALETY");
            }
            
            CSTSManager.SharedInstance.Initialise(kEnv, country);
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

	public void OnBackButton()
	{
		if ( !CSTSManager.SharedInstance.IsViewOpened() )
		{
			GetComponent<PopupController>().Close(false);
		}
	}
}

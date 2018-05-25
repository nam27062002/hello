// MenuPlayScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on //2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuPlayScreen : MonoBehaviour {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//
	 //------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES											//
    //------------------------------------------------------------------//
    public GameObject m_badge;
	public Button m_connectButton;

    [SerializeField]
    private GameObject m_incentivizeRoot = null;

    [SerializeField]
    private Localizer m_incentivizeLabelLocalizer = null; 

    private static bool m_firstTimeMenu = true;

    private Transform m_tournamentBtn;



	private static bool create_mods = true;



    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() 
	{    
        PersistenceFacade.Texts_LocalizeIncentivizedSocial(m_incentivizeLabelLocalizer);
        m_tournamentBtn = transform.FindTransformRecursive("BtnTournament");
        Refresh();

		if (create_mods) {
			InstanceManager.CREATE_MODIFIERS();
			InstanceManager.APPLY_MODIFIERS();
			create_mods=false;
		}

		//create modifiers HERE
    }

    /// <summary>
    /// Component has been enabled.
    /// </summary>
    private void OnEnable() 
	{
        HDTrackingManager.Instance.Notify_MenuLoaded();        

		// Check Facebook/Weibo Connect visibility        
        Refresh();
    }

	private void Update() {
		if (NeedsToRefresh()) {
            Refresh();
        }

        if (m_firstTimeMenu) {
            FeatureSettingsManager.instance.AdjustScreenResolution(FeatureSettingsManager.instance.Device_CurrentFeatureSettings);
            m_firstTimeMenu = false;
        }
    }

    /// <summary>
    /// Component has been disabled.
    /// </summary>
    private void OnDisable() {
       
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//

	public void OnConnectBtn() {        
        PersistenceFacade.Popups_OpenLoadingPopup();

        PersistenceFacade.instance.Sync_FromSettings(delegate()
        {
            PersistenceFacade.Popups_CloseLoadingPopup();
            Refresh();
        });
    }

    private bool SocialIsLoggedIn { get; set; }

    private bool NeedsToRefresh()
    {
        return SocialIsLoggedIn != PersistenceFacade.instance.CloudDriver.IsLoggedIn;
    }
    
    private void Refresh()
    {
        m_connectButton.interactable = true;

        UserProfile.ESocialState socialState = UsersManager.currentUser.SocialState;
        SocialIsLoggedIn = PersistenceFacade.instance.CloudDriver.IsLoggedIn;

        m_incentivizeRoot.SetActive(FeatureSettingsManager.instance.IsIncentivisedLoginEnabled() && socialState != UserProfile.ESocialState.LoggedInAndInventivised);
        m_badge.SetActive(!SocialIsLoggedIn);

        if (m_tournamentBtn)
        {
            m_tournamentBtn.gameObject.SetActive( HDLiveEventsManager.instance.m_tournament.IsActive() );
        }

    }    
    
   	
    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
	/// <summary>
	/// The Privacy Policy button has been pressed.
	/// </summary>
	public void OnPrivacyPolicyButton() {
		// Add some delay to give enough time for SFX to be played and popup to be closed before losing focus
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				string privacyPolicyUrl = "https://legal.ubi.com/privacypolicy/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
				Application.OpenURL(privacyPolicyUrl);
			}, 0.25f
		);
	}
}
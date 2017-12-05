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

	private bool m_showLegalPopup;
        
    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() 
	{    
        PersistenceFacade.Texts_LocalizeIncentivizedSocial(m_incentivizeLabelLocalizer);        
        Refresh();
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
		if (m_showLegalPopup) {
			Debug.LogError("LEGAL");
			// Open terms and conditions popup
			PopupManager.OpenPopupInstant(PopupTermsAndConditions.PATH);
			HDTrackingManager.Instance.Notify_Funnel_Load(FunnelData_Load.Steps._03_terms_and_conditions);
			m_showLegalPopup = false;
		}

        if (NeedsToRefresh()) {
            Refresh();
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

	public void OnConnectBtn()
	{        
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

        m_incentivizeRoot.SetActive(socialState != UserProfile.ESocialState.LoggedInAndInventivised);
        m_badge.SetActive(!SocialIsLoggedIn);        

		m_showLegalPopup = PlayerPrefs.GetInt(PopupTermsAndConditions.KEY) != PopupTermsAndConditions.LEGAL_VERSION;
    }    
    
   	
    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
	/// <summary>
	/// The Privacy Policy button has been pressed.
	/// </summary>
	public void OnPrivacyPolicyButton() {
		string privacyPolicyUrl = "https://legal.ubi.com/privacypolicy/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		Application.OpenURL(privacyPolicyUrl);
	}
}
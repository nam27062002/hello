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
    private GameObject m_incentivizeLabel = null;
    private Localizer m_incentivizeLabelLocalizer;

    private SocialFacade.Network m_socialNetwork = SocialFacade.Network.Default;
    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() 
	{		        
        m_incentivizeLabelLocalizer = m_incentivizeLabel.GetComponent<Localizer>();
        PersistenceManager.Texts_LocalizeIncentivizedSocial(m_incentivizeLabelLocalizer);

        if (m_socialNetwork == SocialFacade.Network.Default)
        {
            m_socialNetwork = SocialManager.GetSelectedSocialNetwork();
        }               

        Refresh();
    }
	
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() 
	{
		// Check Facebook/Weibo Connect visibility        
        Refresh();
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
        PersistenceManager.Popups_OpenLoadingPopup();
        if (SocialManager.GetSelectedSocialNetwork() != m_socialNetwork)
        {
            Debug.LogError("You are trying to switch networks. There should be a proper flow in place for this.");
        }

        //[DGR] No support added yet
        //HSXAnalyticsManager.Instance.loginContext = "MainMenu";

        SocialManager.Instance.Login(m_socialNetwork, delegate (bool success)
        {
            PersistenceManager.Popups_CloseLoadingPopup();

            Refresh();
        });
    }
   
    private void Refresh()
    {
#if CLOUD_SAVE && (WEIBO || FACEBOOK)
        // By default we consider that the button has to be enabled. Next We check the login state, so it will be disabled if the user's logged in
        m_badge.SetActive(true);
        m_connectButton.interactable = true;
        
        m_incentivizeLabel.SetActive(!SocialManager.Instance.WasLoginIncentivised(SocialManager.GetSelectedSocialNetwork()));
        
        AuthManager.LoginState loginState = AuthManager.LoginState.NeverLoggedIn;
        loginState = AuthManager.Instance.GetNetworkLoginState(SocialManagerUtilities.GetLoginTypeFromSocialNetwork(m_socialNetwork));

        switch (loginState)
        {
            case AuthManager.LoginState.LoggedIn:
                m_badge.SetActive(false);
                break;
            default:
                break;
        }
#else
        m_badge.SetActive(false);
#endif
    }
   
    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
}
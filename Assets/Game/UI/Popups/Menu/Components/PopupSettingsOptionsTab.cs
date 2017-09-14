// PopupSettingsOptionsTab.cs
// Hungry Dragon
// 
// Created by David Germade on 30th August 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for handling the options tab in the settings popup.
/// </summary>
public class PopupSettingsOptionsTab : MonoBehaviour
{    
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed
    [SerializeField] private GameObject m_languagePillPrefab = null;
	[SerializeField] private SnappingScrollRect m_languageScrollList = null;
	[Space]
	[SerializeField] private GameObject m_googlePlayGroup = null;
	[SerializeField] private GameObject m_googlePlayLoginButton = null;
	[SerializeField] private GameObject m_googlePlayLogoutButton = null;
	[SerializeField] private Button m_googlePlayAchievementsButton = null;

#if UNITY_ANDROID
	const string TID_LOGIN_ERROR = "TID_GOOGLE_PLAY_AUTH_ERROR";
#elif UNITY_IPHONE
	const string TID_LOGIN_ERROR = "TID_GAME_CENTER_AUTH_ERROR";
#endif
    // Internal
	private List<PopupSettingsLanguagePill> m_pills = new List<PopupSettingsLanguagePill>();

	private PopupController m_loadingPopupController = null;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    public void Awake() {
		// Clear all content of the scroll list (used to do the layout)
		m_languageScrollList.content.DestroyAllChildren(false);

		// Cache language definitions, exluding those not supported by current platform
		List<DefinitionNode> languageDefs = null;
		if(Application.platform == RuntimePlatform.Android) {
			languageDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.LOCALIZATION, "android", "true");
		} else if(Application.platform == RuntimePlatform.IPhonePlayer) {
			languageDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.LOCALIZATION, "iOS", "true");
		} else {
			languageDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LOCALIZATION);
		}

		// Sort definitions by "order" field, create a pill for each language and init with selected language
		DefinitionsManager.SharedInstance.SortByProperty(ref languageDefs, "order", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < languageDefs.Count; i++) {
			// Create and initialize pill
			GameObject pillObj = GameObject.Instantiate<GameObject>(m_languagePillPrefab, m_languageScrollList.content.transform, false);
			PopupSettingsLanguagePill pill = pillObj.GetComponent<PopupSettingsLanguagePill>();
			pill.InitFromDef(languageDefs[i]);
			m_pills.Add(pill);
		}

		// Focus curent language
		//SelectCurrentLanguage(false);

		// Disable google play group if not available
#if UNITY_ANDROID
		m_googlePlayGroup.SetActive(true);
		Messenger.AddListener(EngineEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
#else
		m_googlePlayGroup.SetActive(false);	// [AOC] TODO!!
#endif
    }

    void OnDestroy(){
		Messenger.RemoveListener(EngineEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
    }

	/// <summary>
	/// Focus the currently selected language.
	/// </summary>
	private void SelectCurrentLanguage(bool _animate) {
		// Scroll to initial language pill
		string currentLangSku = LocalizationManager.SharedInstance.GetCurrentLanguageSKU();
		for(int i = 0; i < m_pills.Count; i++) {
			// Is it the selected one?
			if(m_pills[i].def.sku == currentLangSku) {
				// Yes! Snap to it and break the loop
				m_languageScrollList.SelectPoint(m_pills[i].GetComponent<ScrollRectSnapPoint>(), _animate);
				break;
			}
		}
	}

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
	/// <summary>
	/// A new pill has been selected on the snapping scroll list.
	/// </summary>
	/// <param name="_selectedPoint">Selected point.</param>
	public void OnSelectionChanged(ScrollRectSnapPoint _selectedPoint) {
		if(_selectedPoint == null) return;

		// Find selected language
		DefinitionNode newLangDef = _selectedPoint.GetComponent<PopupSettingsLanguagePill>().def;

		// Change localization!
		if(LocalizationManager.SharedInstance.SetLanguage(newLangDef.sku)) {
			// Store new language
			PlayerPrefs.SetString(PopupSettings.KEY_SETTINGS_LANGUAGE, newLangDef.sku);

			// [AOC] If the setting is enabled, replace missing TIDs for english ones
			if(!Prefs.GetBoolPlayer(DebugSettings.SHOW_MISSING_TIDS, false)) {
				LocalizationManager.SharedInstance.FillEmptyTids("lang_english");
			}
		}

		Messenger.Broadcast(EngineEvents.LANGUAGE_CHANGED);
	}

	/// <summary>
	/// The popup has just finished open.
	/// </summary>
	public void OnOpenPostAnimation() {
		SelectCurrentLanguage(true);
		//UbiBCN.CoroutineManager.DelayedCallByFrames(() => { SelectCurrentLanguage(true); }, 5);
	}

	public void RefreshGooglePlayView(){
		if ( m_loadingPopupController != null ){
			m_loadingPopupController.Close(true);
			m_loadingPopupController = null;
		}

		if ( ApplicationManager.instance.GameCenter_IsAuthenticated() ){
			m_googlePlayLoginButton.SetActive(false);
			m_googlePlayLogoutButton.SetActive(true);
			m_googlePlayAchievementsButton.interactable = true;
		}else{
			m_googlePlayLoginButton.SetActive(true);
			m_googlePlayLogoutButton.SetActive(false);
			m_googlePlayAchievementsButton.interactable = false;
		}
	}

	public void GooglePlayAuthCancelled(){
		if ( m_loadingPopupController != null ){
			m_loadingPopupController.Close(true);
			m_loadingPopupController = null;
		}
	}

	public void GooglePlayAuthFailed(){
		if ( m_loadingPopupController != null ){
			m_loadingPopupController.Close(true);
			m_loadingPopupController = null;
		}

		// Show generic message there was an error!
		UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize(TID_LOGIN_ERROR), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
	}

	public void OnShow(){
		#if UNITY_ANDROID
			RefreshGooglePlayView();
			Messenger.AddListener(EngineEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
			Messenger.AddListener(EngineEvents.GOOGLE_PLAY_AUTH_CANCELLED, GooglePlayAuthCancelled);
			Messenger.AddListener(EngineEvents.GOOGLE_PLAY_AUTH_FAILED, GooglePlayAuthFailed);
		#endif
	}

	public void OnHide(){
		#if UNITY_ANDROID
			Messenger.RemoveListener(EngineEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
			Messenger.RemoveListener(EngineEvents.GOOGLE_PLAY_AUTH_CANCELLED, GooglePlayAuthCancelled);
			Messenger.RemoveListener(EngineEvents.GOOGLE_PLAY_AUTH_FAILED, GooglePlayAuthFailed);
		#endif
	}

	public void OnGooglePlayLogIn(){
		if (!ApplicationManager.instance.GameCenter_IsAuthenticated()){
			// Show curtain and wait for game center response
			bool createLoading = true;

			if ( !GameCenterManager.SharedInstance.GetAuthenticatingState() )	// if not authenticating
			{
				ApplicationManager.instance.GameCenter_Login();
			}

			if (GameCenterManager.SharedInstance.GetAuthenticatingState())
			{
				m_loadingPopupController = PopupManager.PopupLoading_Open();
			}
			else
			{
				// No curatin -> something failed, we are not authenticating -> tell the player there was an error	
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize(TID_LOGIN_ERROR), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			}


		}
	}

	public void OnGooglePlayLogOut(){
		if (ApplicationManager.instance.GameCenter_IsAuthenticated()){
			ApplicationManager.instance.GameCenter_LogOut();
		}
	}

	public void OnGooglePlayAchievements(){
		if (ApplicationManager.instance.GameCenter_IsAuthenticated()){
			ApplicationManager.instance.GameCenter_ShowAchievements();
		}
	}

}
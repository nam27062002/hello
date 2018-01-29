// PopupSettingsOptionsTab.cs
// Hungry Dragon
// 
// Created by David Germade on 30th August 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
	[SerializeField] private Slider m_graphicsQualitySlider = null;
	[SerializeField] private CanvasGroup m_graphicsQualityCanvasGroup = null;
	[SerializeField] private TextMeshProUGUI m_graphicsQualityCurrentValueText = null;
	[SerializeField] private GameObject[] m_graphicsQualitySeparators = new GameObject[4];
	[Space]
	[SerializeField] private GameObject m_googlePlayGroup = null;
	[SerializeField] private GameObject m_googlePlayLoginButton = null;
	[SerializeField] private GameObject m_googlePlayLogoutButton = null;
	[SerializeField] private Button m_googlePlayAchievementsButton = null;
	[Space]
	[SerializeField] private GameObject m_gameCenterGroup = null;

#if UNITY_ANDROID
	const string TID_LOGIN_ERROR = "TID_GOOGLE_PLAY_AUTH_ERROR";
#elif UNITY_IPHONE
	const string TID_LOGIN_ERROR = "TID_GAME_CENTER_AUTH_ERROR";
#endif
    // Internal
	private List<PopupSettingsLanguagePill> m_pills = new List<PopupSettingsLanguagePill>();

	private PopupController m_loadingPopupController = null;
	private bool m_dirty = false;

	private int m_graphicsMaxLevel = 4;
	private int m_initialGraphicsQualityLevel = -1;

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

		if (m_pills.Count == 1) {
			m_languageScrollList.enabled = false;
		}

		m_dirty = true;

		// Disable google play group if not available
#if UNITY_ANDROID
		m_googlePlayGroup.SetActive(true);
		Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
#else
		m_googlePlayGroup.SetActive(false);
#endif

		// Do the same for the GameCenter group!
		m_gameCenterGroup.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
    }

    void OnDestroy(){
		Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
    }

	void Update() {
		if (m_dirty) {
			// Focus curent language
			SelectCurrentLanguage(false);
			m_dirty = false;
		}
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

	/// <summary>
	/// Update the graphics quality textfield with the current selected value.
	/// </summary>
	private void RefreshGraphicsQualityText() {
		// Just display the number
		int value = Mathf.Min(m_graphicsMaxLevel, (int)m_graphicsQualitySlider.value);	// Special case for when max level is 0
		m_graphicsQualityCurrentValueText.text = StringUtils.FormatNumber(value + 1);
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

		Messenger.Broadcast(MessengerEvents.LANGUAGE_CHANGED);
	}

	/// <summary>
	/// The graphics quality slider has changed its value.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnGraphicsQualityChanged(float _newValue) {
		// Ignore if max quality level is 0 (shouldn't even get here)
		if(m_graphicsMaxLevel == 0) return;

		// Store new value
		FeatureSettingsManager.instance.SetUserProfileLevel((int)_newValue);

		// Update text
		RefreshGraphicsQualityText();
	}

	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Initialize Graphics Quality Slider
		m_initialGraphicsQualityLevel = FeatureSettingsManager.instance.GetUserProfileLevel();
		m_graphicsMaxLevel = FeatureSettingsManager.instance.GetMaxProfileLevelSupported();
		int userLevel = m_initialGraphicsQualityLevel;
		if(userLevel < 0) {
			userLevel = FeatureSettingsManager.instance.GetCurrentProfileLevel();
		}

		// Special case when only 1 level is available
		m_graphicsQualitySlider.minValue = 0;
		if(m_graphicsMaxLevel == 0) {
			m_graphicsQualitySlider.maxValue = 1;
			m_graphicsQualitySlider.value = 1;
			m_graphicsQualityCanvasGroup.interactable = false;
		} else {
			m_graphicsQualitySlider.maxValue = m_graphicsMaxLevel;
			m_graphicsQualitySlider.value = userLevel;
			m_graphicsQualityCanvasGroup.interactable = true;
		}
		m_graphicsQualitySlider.onValueChanged.AddListener(OnGraphicsQualityChanged);

		// Adjust number of separators according to slider's max value
		for(int i = 0; i < m_graphicsQualitySeparators.Length; ++i) {
			m_graphicsQualitySeparators[i].SetActive(i < (int)m_graphicsQualitySlider.maxValue);	// Last level doesn't have a separator (it's the end of the slider)
		}

		// Initialize text
		RefreshGraphicsQualityText();
	}

	/// <summary>
	/// The popup is about to be closed.
	/// </summary>
	public void OnClosePreAnimation() {
		// If the graphics quality setting has changed, apply new value
		// Ignore if max quality level is 0 (shouldn't even get here)
		if(m_graphicsMaxLevel > 0 && FeatureSettingsManager.instance.GetUserProfileLevel() != m_initialGraphicsQualityLevel) {
			// Program a sequence of events so every step has enough time to be applied properly
			DOTween.Sequence()
				.SetAutoKill(true)
				.AppendCallback(() => {
					// Show busy screen
					BusyScreen.Setup(true, LocalizationManager.SharedInstance.Localize("TID_QUALITY_SLIDER_APPLYING"));
					BusyScreen.Show(this, false);
				})
				.AppendInterval(0.5f)
				.AppendCallback(() => {
					// Apply new quality setting
					FeatureSettingsManager.instance.RecalculateAndApplyProfile();
				})
				.AppendInterval(0.5f)
				.AppendCallback(() => {
					// Hide busy screen
					BusyScreen.Hide(this, true);
				})
				.SetUpdate(true)
				.Play();
		}

		// Remove listener
		m_graphicsQualitySlider.onValueChanged.RemoveListener(OnGraphicsQualityChanged);
	}

	/// <summary>
	/// The popup has been closed.
	/// </summary>
	public void OnClosePostAnimation() {
		
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
			Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
			Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_AUTH_CANCELLED, GooglePlayAuthCancelled);
			Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_AUTH_FAILED, GooglePlayAuthFailed);
		#endif

		m_dirty = true;
	}

	public void OnHide(){
		#if UNITY_ANDROID
			Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, RefreshGooglePlayView);
			Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_AUTH_CANCELLED, GooglePlayAuthCancelled);
			Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_AUTH_FAILED, GooglePlayAuthFailed);
		#endif
	}

	public void OnGooglePlayLogIn(){
		if (!ApplicationManager.instance.GameCenter_IsAuthenticated()){
			// Show curtain and wait for game center response
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
			// Add some delay to give enough time for SFX to be played before losing focus
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					ApplicationManager.instance.GameCenter_ShowAchievements();
				}, 0.15f
			);
		}
	}

	public void OnGameCenterButton() {
		// Black magic from Calety xD
		GameCenterManager.SharedInstance.LaunchGameCenterApp();
	}
}
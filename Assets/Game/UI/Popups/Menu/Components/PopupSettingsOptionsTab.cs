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
public class PopupSettingsOptionsTab : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string LANGUAGE_PILL_PATH = "UI/Metagame/Settings/PF_LanguagesFlagPill";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private SnappingScrollRect m_languageScrollList = null;
	[Space]
	[SerializeField] private Slider m_graphicsQualitySlider = null;
	[SerializeField] private CanvasGroup m_graphicsQualityCanvasGroup = null;
	[SerializeField] private TextMeshProUGUI m_graphicsQualityCurrentValueText = null;
	[SerializeField] private GameObject[] m_graphicsQualitySeparators = new GameObject[4];
	[Space]
	[SerializeField] private GameObject m_bloodToggle;
	[SerializeField] private GameObject m_gameCenterForChildrenGroup = null;

	// Internal
	private List<PopupSettingsLanguagePill> m_pills = new List<PopupSettingsLanguagePill>();

	private bool m_languageInitialized = false;

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
		GameObject prefab = Resources.Load<GameObject>(LANGUAGE_PILL_PATH);
		for(int i = 0; i < languageDefs.Count; i++) {
			// Create and initialize pill
			GameObject pillObj = GameObject.Instantiate<GameObject>(prefab, m_languageScrollList.content.transform, false);
			PopupSettingsLanguagePill pill = pillObj.GetComponent<PopupSettingsLanguagePill>();
			pill.InitFromDef(languageDefs[i]);
			m_pills.Add(pill);
		}
		prefab = null;

		if(m_pills.Count == 1) {
			m_languageScrollList.enabled = false;
		}

		m_languageInitialized = false;

		// Toggle some components on/off if Age Restriction is enabled
		bool ageRestriction = GDPRManager.SharedInstance.IsAgeRestrictionEnabled();
		if(m_bloodToggle != null) {
			m_bloodToggle.SetActive(!ageRestriction);
		}

		if(m_gameCenterForChildrenGroup != null) {
#if UNITY_IOS
			m_gameCenterForChildrenGroup.SetActive(ageRestriction);
#else
			m_gameCenterForChildrenGroup.SetActive(false);
#endif
		}
    }

	void Update() {
		if (!m_languageInitialized) {
			// Focus curent language
			m_languageInitialized = true;

			// Delay a frame to give time for everything to get initialized
			UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
				SelectCurrentLanguage();
				m_languageScrollList.onSelectionChanged.AddListener(OnLanguageSelectionChanged);
			}, 1);
		}
	}

	/// <summary>
	/// Focus the currently selected language.
	/// </summary>
	private void SelectCurrentLanguage() {
		// Scroll to initial language pill
		string currentLangSku = LocalizationManager.SharedInstance.GetCurrentLanguageSKU();
		for(int i = 0; i < m_pills.Count; i++) {
			// Is it the selected one?
			if(m_pills[i].def.sku == currentLangSku) {
				// Yes! Snap to it and break the loop
				m_languageScrollList.SelectPoint(m_pills[i].GetComponent<ScrollRectSnapPoint>(), false);
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
	public void OnLanguageSelectionChanged(ScrollRectSnapPoint _selectedPoint) {
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

	public void OnShow(){
		m_languageInitialized = false;
	}

	public void OnHide() {
		m_languageScrollList.onSelectionChanged.RemoveListener(OnLanguageSelectionChanged);
	}
}

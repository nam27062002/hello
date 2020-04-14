// PopupSettingsOptionsTab.cs
// Hungry Dragon
//
// Created by David Germade on 30th August 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
	[SerializeField] private Slider m_graphicsQualitySlider = null;
	[SerializeField] private CanvasGroup m_graphicsQualityCanvasGroup = null;
	[SerializeField] private TextMeshProUGUI m_graphicsQualityCurrentValueText = null;
	[SerializeField] private GameObject[] m_graphicsQualitySeparators = new GameObject[4];
	[Space]
	[SerializeField] private GameObject m_adultGroupRoot = null;
	[SerializeField] private GameObject m_childrenGroupRoot_iOS = null;
	[SerializeField] private GameObject m_childrenGroupRoot_Android = null;
	[SerializeField] private GameObject m_bloodToggleRoot = null;


	private int m_graphicsMaxLevel = 4;
	private int m_initialGraphicsQualityLevel = -1;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Toggle some components on/off if Age Restriction is enabled
		bool ageRestriction = GDPRManager.SharedInstance.IsAgeRestrictionEnabled();

		if(m_adultGroupRoot != null) {
			m_adultGroupRoot.SetActive(!ageRestriction);
		}

		if(m_childrenGroupRoot_iOS != null) {
#if UNITY_IOS
			m_childrenGroupRoot_iOS.SetActive(ageRestriction);
#else
			m_childrenGroupRoot_iOS.SetActive(false);
#endif
		}

		if(m_childrenGroupRoot_Android != null) {
#if UNITY_ANDROID
			m_childrenGroupRoot_Android.SetActive(ageRestriction);
#else
			m_childrenGroupRoot_Android.SetActive(false);
#endif
		}

		// Don't show blood toggle in China or Korea
		// Don't show for underage either
		// [AOC] Temp solution for 2.8 while waiting for the Flavours feature implementation (2.10)
		if(m_bloodToggleRoot != null) {
			m_bloodToggleRoot.SetActive(
				!ageRestriction &&
				!PlatformUtils.Instance.IsChina() && 
				!PlatformUtils.Instance.IsKorea()
			);
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

    
    public void OnLanguageBtn(){
        PopupManager.OpenPopupInstant(PopupLanguageSelector.PATH);
    }
}

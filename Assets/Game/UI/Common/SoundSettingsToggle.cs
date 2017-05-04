// SoundSettingsToggle.cs
// Hungry Dragon
// 
// Created by David Germade on 15th September 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class can be used to use a slider as a toggle for enabling/disabling the sound on settings popup.
/// </summary>
public class SoundSettingsToggle : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Slider m_soundSlider;
	[SerializeField] private Slider m_musicSlider;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initial state
		Refresh();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update slider's values from settings.
	/// </summary>
	private void Refresh() {
		if(ApplicationManager.instance.Settings_GetSound()) {
			m_soundSlider.value = m_soundSlider.maxValue;
		} else {
			m_soundSlider.value = m_soundSlider.minValue;
		}

		if(ApplicationManager.instance.Settings_GetMusic()) {
			m_musicSlider.value = m_musicSlider.maxValue;
		} else {
			m_musicSlider.value = m_musicSlider.minValue;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	///The sound slider has been toggled.
	/// </summary>
	public void OnSoundToggleChanged() {
		bool viewIsEnabled = m_soundSlider.value == m_soundSlider.maxValue;
		bool isEnabled = ApplicationManager.instance.Settings_GetSound();
		if(isEnabled != viewIsEnabled) {
			ApplicationManager.instance.Settings_ToggleSoundIsEnabled();
			if(isEnabled) {
				// AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");
			}

			Refresh();
		}
	}

	/// <summary>
	/// The music slider has been toggled.
	/// </summary>
	public void OnMusicToggleChanged() {
		bool viewIsEnabled = m_musicSlider.value == m_musicSlider.maxValue;
		bool isEnabled = ApplicationManager.instance.Settings_GetMusic();
		if(isEnabled != viewIsEnabled) {
			ApplicationManager.instance.Settings_ToggleMusicIsEnabled();
			if(isEnabled) {
				// AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");
			}

			Refresh();
		}
	}
}

// GameSettingsToggle.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class GameSettingsToggle : ToggleSlider {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] protected string m_settingId = "";
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Init toggle with current value
		this.toggled = GameSettings.Get(m_settingId);

		// Be aware for toggle changes
		this.slider.onValueChanged.AddListener(OnToggleChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
		// Stop listening for toggle changes
		this.slider.onValueChanged.RemoveListener(OnToggleChanged);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Slider value changed.
	/// </summary>
	protected virtual void OnToggleChanged(float _newValue) {
		GameSettings.Set(m_settingId, this.toggled);
	}
}
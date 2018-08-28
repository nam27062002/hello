// NotificationsToggle.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom toggle to enable/disable notifications.
/// </summary>
public class NotificationsToggle : ToggleSlider {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Init toggle with current value
		this.toggled = HDNotificationsManager.instance.GetNotificationsEnabled();

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
		// Store new value
		HDNotificationsManager.instance.SetNotificationsEnabled(this.toggled);
	}
}
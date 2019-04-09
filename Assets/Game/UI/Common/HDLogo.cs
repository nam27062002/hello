// HDLogo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/07/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to add logic to the Hungry Dragon logo.
/// </summary>
public class HDLogo : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Image m_image = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);

		// Make sure we have the right logo loaded
		Refresh();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}
    
    /// <summary>
    /// Ons the broadcast signal.
    /// </summary>
    /// <param name="_eventType">Event type.</param>
    /// <param name="_broadcastEventInfo">Broadcast event info.</param>
    public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
        switch(_eventType) {
            case BroadcastEventType.LANGUAGE_CHANGED: {
                OnLanguageChanged();
            } break;
        }
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh logo.
	/// </summary>
	private void Refresh() {
		// Get new language definition
		string langSku = LocalizationManager.SharedInstance.GetCurrentLanguageSKU();
		DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, langSku);
		if(langDef == null) return;

		// Find logo path for the current language
		string logoName = langDef.GetAsString("logo");

		// Check that the logo is no the same as already loaded
		if(m_image.sprite.name == logoName) return;

		// Load new image
		m_image.sprite = Resources.Load<Sprite>(UIConstants.HD_LOGO_PATH + logoName);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new language has been selected.
	/// </summary>
	private void OnLanguageChanged() {
		Refresh();
	}
}
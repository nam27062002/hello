// PlayTestActivator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to enable/disable game objects during play test sessions.
/// </summary>
public class PlayTestActivator : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private bool m_activeDuringPlaytest = false;

	// Internal
	private bool m_wasActive = true;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, OnPrefChanged);

		// Init internal vars
		m_wasActive = this.gameObject.activeSelf;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// If play test is active, apply now
		if(DebugSettings.isPlayTest) {
			this.gameObject.SetActive(m_activeDuringPlaytest);
		}
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, OnPrefChanged);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A debug pref has changed.
	/// </summary>
	/// <param name="_prefId">Pref identifier.</param>
	/// <param name="_newValue">The new value for that setting.</param>
	public void OnPrefChanged(string _prefId, bool _newValue) {
		// We only care about the play test setting
		if(_prefId != DebugSettings.PLAY_TEST) return;

		// Going to play test mode?
		if(_newValue) {
			// Store original activation status
			m_wasActive = this.gameObject.activeSelf;

			// Apply new status
			this.gameObject.SetActive(m_activeDuringPlaytest);
		} else {
			// Restore original activation status
			this.gameObject.SetActive(m_wasActive);
		}
	}
}
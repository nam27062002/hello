// DisableOnPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to enable/disable a game object based on the amount of popups opened.
/// Useful to hide things that are rendered on top of the popups canvas such as 
/// particles, custom cameras, other canvases...
/// </summary>
public class DisableOnPopup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Additional actions to be executed upon popup opening/closing
	[SerializeField] private UnityEvent m_onPopupOpened = new UnityEvent();
	[SerializeField] private UnityEvent m_onAllPopupsClosed = new UnityEvent();

	// Internal logic
	bool m_pendingActivation = false;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A popup has been opened.
	/// </summary>
	/// <param name="_popup">The popup that has been opened.</param>
	private void OnPopupOpened(PopupController _popup) {
		// Skip if object already disabled
		if(!gameObject.activeSelf) return;

		// Disable this object
		gameObject.SetActive(false);
		m_pendingActivation = true;

		// Execute all aditional actions
		m_onPopupOpened.Invoke();
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup that has been closed.</param>
	private void OnPopupClosed(PopupController _popup) {
		// If there are no more popups opened and activation was pending, re-enable the game object
		if(m_pendingActivation && PopupManager.openPopupsCount <= 0) {
			// Reset flag
			gameObject.SetActive(true);
			m_pendingActivation = false;

			// Execute all aditional actions
			m_onAllPopupsClosed.Invoke();
		}
	}
}
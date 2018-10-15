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
/// If a show/hide animator is defined, it will be used instead of directly 
/// activating/deactivating the object.
/// </summary>
public class DisableOnPopup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[Comment("Leave negative to use the amount of popups open at the moment this component is enabled.")]
	[SerializeField] private int m_refPopupCount = -1;

	// Optional animator to be used
	[Comment("Optional animator to be used instead of directly activating/deactivating the GameObject.")]
	[SerializeField] private ShowHideAnimator m_animator = null;

	// Additional actions to be executed upon popup opening/closing
	[Space]
	[Comment("Additional actions to be executed upon popup opening/closing")]
	[SerializeField] private UnityEvent m_onPopupOpened = new UnityEvent();
	[SerializeField] private UnityEvent m_onAllPopupsClosed = new UnityEvent();

	// Internal
	private bool m_pendingActivation = false;
	public int refPopupCount {
		get { return m_refPopupCount; }
		set { m_refPopupCount = value; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<PopupController>(MessengerEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.AddListener<PopupController>(MessengerEvents.POPUP_CLOSED, OnPopupClosed);
		Messenger.AddListener<PopupController>(MessengerEvents.POPUP_DESTROYED, OnPopupClosed);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// If target popups was not manually defined, store current popup count as reference
		if(m_refPopupCount < 0) {
			m_refPopupCount = PopupManager.openPopupsCount;

			// If this component belongs to a popup, increase ref amount to include parent popup (who is not still opened)
			PopupController parentPopup = GetComponentInParent<PopupController>();
			if(parentPopup != null) {
				// Don't if already counted
				if(!PopupManager.openedPopups.Contains(parentPopup)) {
					m_refPopupCount++;
				}
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(MessengerEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.RemoveListener<PopupController>(MessengerEvents.POPUP_CLOSED, OnPopupClosed);
		Messenger.RemoveListener<PopupController>(MessengerEvents.POPUP_DESTROYED, OnPopupClosed);
	}

	/// <summary>
	/// Check opened popups count and check whether this object should be displayed or not.
	/// </summary>
	/// <returns>Was it active?</returns>
	private bool CheckVisibility() {
		// In order for the object to be visible, there can't be more popups opened than our target ref
		bool show = PopupManager.openPopupsCount <= m_refPopupCount;
		if(m_animator != null) {
			m_animator.ForceSet(show);
		} else {
			gameObject.SetActive(show);
		}
		return show;
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

		// Hide object?
		bool show = CheckVisibility();

		// Set flag
		m_pendingActivation = !show;

		// Execute all aditional actions
		m_onPopupOpened.Invoke();
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup that has been closed.</param>
	private void OnPopupClosed(PopupController _popup) {
		// If there are no more popups opened and activation was pending, re-enable the game object
		if(m_pendingActivation) {
			// Must the object be restored?
			bool show = CheckVisibility();
			if(show) {
				// Reset flag
				m_pendingActivation = false;

				// Execute all aditional actions
				m_onAllPopupsClosed.Invoke();
			}
		}
	}
}
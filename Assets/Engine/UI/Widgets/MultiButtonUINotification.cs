// MultiButtonUINotification.cs
// 
// Created by Alger Ortín Castellví on 10/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control a UI notification for a multibutton.
/// </summary>
[RequireComponent(typeof(MultiButton))]
public class MultiButtonUINotification : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[SerializeField] private UINotification m_notification = null;
	private MultiButton m_multiButton = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get references
		m_multiButton = GetComponent<MultiButton>();

		// Subscribe to events
		m_multiButton.OnFold.AddListener(Refresh);
		m_multiButton.OnUnfold.AddListener(Refresh);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Start with hidden notification
		if(m_notification != null) {
			m_notification.Hide(false);
		}

		// Refresh
		Refresh();
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Refresh each time the component is enabled
		Refresh();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Subscribe to events
		m_multiButton.OnFold.RemoveListener(Refresh);
		m_multiButton.OnUnfold.RemoveListener(Refresh);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether to show the notification or not.
	/// </summary>
	public void Refresh() {
		// Notification visible if we have a notification in either of the multibutton's options and multibutton is folded
		if(m_notification != null) {
			// If the button is unfolded, just hide the notification
			if(!m_multiButton.folded) {
				m_notification.Hide();
			} else {
				// Look for notification in the children
				bool childNotifications = false;
				UINotification notif = null;
				for(int i = 0; i < m_multiButton.subButtons.Length; i++) {
					// Does this button have an active notification?
					notif = m_multiButton.subButtons[i].GetComponentInChildren<UINotification>(true);
					if(notif != null && notif.visible) {
						// Yes! Break loop
						childNotifications = true;
						break;
					}
				}

				// Set notification visibility
				m_notification.Set(childNotifications);
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
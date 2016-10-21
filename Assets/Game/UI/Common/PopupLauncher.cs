// PopupLauncher.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
public class PopupLauncher : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[FileList("Resources/UI/Popups", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_popupPath = "";

	// Internal
	private PopupController m_popup = null;
	public PopupController popup {
		get { return m_popup; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_DESTROYED, OnPopupDestroyed);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_DESTROYED, OnPopupDestroyed);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The linked button has been clicked.
	/// </summary>
	/// <param name="_reload">Destroy and load again if the popup is already opened (or it was closed without destroying)?</param">
	public void OpenPopup(bool _reload = false) {
		// If the popup is already open, check what to do
		if(m_popup != null) {
			// Force reloading?
			if(_reload) {
				// Close and destroy previous popup
				m_popup.Close(true);
				m_popup = null;
			} else if(m_popup.isOpen) {
				// Do nothing if already opened
				return;
			} else {
				// Re-open it
				m_popup.Open();
			}
		}

		// Popup not opened, load and open it
		else {
			m_popup = PopupManager.OpenPopupInstant(m_popupPath);
		}
	}

	/// <summary>
	/// Closes the popup.
	/// </summary>
	/// <param name="_destroy">If set to <c>true</c> destroy.</param>
	public void ClosePopup(bool _destroy = true) {
		if(m_popup != null) {
			m_popup.Close(_destroy);
			if(_destroy) m_popup = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A popup has been destroyed.
	/// </summary>
	/// <param name="_popup">The popup that has just been destroyed.</param>
	private void OnPopupDestroyed(PopupController _popup) {
		if(_popup == m_popup) {
			m_popup = null;
		}
	}
}
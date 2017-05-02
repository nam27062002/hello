// PopupLauncher.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

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
	[System.Serializable] public class PopupEvent : UnityEvent<PopupController> { };
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[FileList("Resources/UI/Popups", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] protected string m_popupPath = "";
	[SerializeField] protected float m_delay = 0f;

	// Internal
	protected PopupController m_popup = null;
	public PopupController popup {
		get { return m_popup; }
	}

	private bool m_pendingOpening = false;

	// Events
	public PopupEvent OnPopupInit = new PopupEvent();	// Invoked every time right before the popup is opened (and after it's loaded)
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_DESTROYED, OnPopupDestroyed);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_DESTROYED, OnPopupDestroyed);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The linked button has been clicked.
	/// </summary>
	/// <param name="_reload">Destroy and load again if the popup is already opened (or it was closed without destroying)?</param">
	public virtual void OpenPopup(bool _reload = false) {
		// If the popup is already open, check what to do
		if(m_popup != null) {
			// Force reloading?
			if(_reload) {
				// Close and destroy previous popup
				m_popup.Close(true);
				m_pendingOpening = true;	// Wait until the close animation has finished to re-open the popup
			} else if(m_popup.isOpen) {
				// Do nothing if already opened
				return;
			} else {
				// Re-open it
				OpenPopupInternal();
			}
		}

		// Popup not opened, load and open it
		else {
			OpenPopupInternal();
		}
	}

	/// <summary>
	/// Closes the popup.
	/// </summary>
	/// <param name="_destroy">If set to <c>true</c> destroy.</param>
	public virtual void ClosePopup(bool _destroy = true) {
		if(m_popup != null) {
			m_popup.Close(_destroy);
			if(_destroy) m_popup = null;
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gather the popup from the manager and open it.
	/// Invokes the OnPopupInit event.
	/// </summary>
	private void OpenPopupInternal() {
		// Load the popup
		m_popup = PopupManager.LoadPopup(m_popupPath);
		OnPopupInit.Invoke(m_popup);

		// Open (apply delay)
		DOVirtual.DelayedCall(m_delay, () => m_popup.Open());
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A popup has been destroyed.
	/// </summary>
	/// <param name="_popup">The popup that has just been destroyed.</param>
	protected virtual void OnPopupDestroyed(PopupController _popup) {
		if(_popup == m_popup) {
			m_popup = null;

			// Was a re-opening pending?
			if(m_pendingOpening) {
				m_pendingOpening = false;
				OpenPopupInternal();
			}
		}
	}
}
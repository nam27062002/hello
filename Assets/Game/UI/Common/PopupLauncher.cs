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
using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class PopupLauncher : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable] public class PopupEvent : UnityEvent<PopupController> { };

	public enum TrackingAction {
		NONE,
		INFO_POPUP_AUTO,
		INFO_POPUP_IBUTTON,
		INFO_POPUP_SETTINGS
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[FileList("Resources/UI/Popups", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] protected string m_popupPath = "";
	[SerializeField] protected float m_delay = 0f;
	[Space]
	[SerializeField] protected TrackingAction m_trackingAction = TrackingAction.NONE;

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
		Broadcaster.AddListener(BroadcastEventType.POPUP_DESTROYED, this);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_DESTROYED, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.POPUP_DESTROYED:
            {
                PopupManagementInfo info = (PopupManagementInfo)broadcastEventInfo;
                OnPopupDestroyed(info.popupController);
            }break;
        }
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
	/// Open the popup via PopupManager
	/// Invokes the OnPopupInit event.
	/// </summary>
	private void OpenPopupInternal() {
		// Load the popup
		m_popup = PopupManager.LoadPopup(m_popupPath);
		OnPopupInit.Invoke(m_popup);

        m_popup.OnOpen.AddListener(DoTracking);

		// Open it!
		m_popup.Open();
    }


	/// <summary>
	/// Sends the defined tracking action.
	/// </summary>
	private void DoTracking(PopupController _controller) {
		// Ignore if none
		if(m_trackingAction == TrackingAction.NONE) return;

		// Aux vars
		string popupName = Path.GetFileNameWithoutExtension(m_popupPath);

		// Do it!
		HDTrackingManager.Instance.Notify_InfoPopup(popupName, TrackingActionToString(m_trackingAction));
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Convert tracking action enum to string to be sent.
	/// </summary>
	/// <returns>String parameter value.</returns>
	/// <param name="_action">Action to be converted.</param>
	public static string TrackingActionToString(TrackingAction _action) {
		switch(_action) {
			case TrackingAction.INFO_POPUP_AUTO: return "automatic";
			case TrackingAction.INFO_POPUP_IBUTTON: return "info_button";
			case TrackingAction.INFO_POPUP_SETTINGS: return "settings";
		}
		return string.Empty;
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
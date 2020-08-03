// GameCenterButtonHandler.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple logic to manage a Game Center button.
/// </summary>
public class GameCenterButtonHandler : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private PopupController m_confirmPopup = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, OnStateUpdate);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, OnStateUpdate);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The Game Center button was pressed.
	/// </summary>
	public void OnGameCenterButton() {
		// Apple does NOT login the user, we need to check it.
		if(!GameCenterManager.SharedInstance.CheckIfAuthenticated()) {
			IPopupMessage.Config config = IPopupMessage.GetConfig();
			config.TitleTid = "TID_GAMECENTER_CONNECTION_TITLE";
			config.ShowTitle = true;
			config.MessageTid = "TID_GAMECENTER_CONNECTION_BODY";
			// This popup ignores back button and stays open so the user makes a decision
			config.BackButtonStrategy = IPopupMessage.Config.EBackButtonStratety.PerformConfirm;
			config.ConfirmButtonTid = "TID_GEN_OK";
			config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
			config.IsButtonCloseVisible = false;
			m_confirmPopup = PopupManager.PopupMessage_Open(config);
			m_confirmPopup.OnClosePreAnimation.AddListener(OnPopupDismissed);
		} else {
			GameCenterManager.SharedInstance.LaunchGameCenterApp();
		}
	}

	/// <summary>
	/// The confirmation popup has been closed.
	/// </summary>
	void OnPopupDismissed() {
		m_confirmPopup = null;
	}

	/// <summary>
	/// Game center state has changed.
	/// </summary>
	public void OnStateUpdate() {
#if UNITY_IOS
		if(m_confirmPopup != null) {
			m_confirmPopup.Close(true);
			OnGameCenterButton();
		}
#endif
	}
}
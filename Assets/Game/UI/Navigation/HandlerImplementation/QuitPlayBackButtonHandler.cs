using UnityEngine;
using UnityEngine.Events;

public class QuitPlayBackButtonHandler : BackButtonHandler {

	bool m_changingArea = false;
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	private void OnEnable() {
		Messenger.AddListener(MessengerEvents.GAME_LEVEL_LOADED, Register);
		Messenger.AddListener(MessengerEvents.PLAYER_ENTERING_AREA, OnAreaEnter);
		Messenger.AddListener(MessengerEvents.PLAYER_LEAVING_AREA, OnAreaLeave);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, Unregister);
	}

	private void OnDisable() {
		Messenger.RemoveListener(MessengerEvents.GAME_LEVEL_LOADED, Register);
		Messenger.RemoveListener(MessengerEvents.PLAYER_ENTERING_AREA, OnAreaEnter);
		Messenger.RemoveListener(MessengerEvents.PLAYER_LEAVING_AREA, OnAreaLeave);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, Unregister);
	}

	public void OnAreaEnter()
	{
		m_changingArea = false;
	}
	public void OnAreaLeave()
	{
		m_changingArea = true;
	}

	public override void Trigger() {
		if (!m_changingArea)
		{
			PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
		}
		/*
		if (GameSettings.Get(GameSettings.SHOW_EXIT_RUN_CONFIRMATION_POPUP)) {
			PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
		} else {
			if (InstanceManager.gameSceneController != null) {
				InstanceManager.gameSceneController.EndGame(true);
			}
		}*/
	}
}

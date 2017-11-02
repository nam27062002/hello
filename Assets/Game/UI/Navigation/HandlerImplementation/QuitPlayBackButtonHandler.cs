using UnityEngine;
using UnityEngine.Events;

public class QuitPlayBackButtonHandler : BackButtonHandler {

	bool m_changingArea = false;
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	private void OnEnable() {
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, Register);
		Messenger.AddListener(GameEvents.PLAYER_ENTERING_AREA, OnAreaEnter);
		Messenger.AddListener(GameEvents.PLAYER_LEAVING_AREA, OnAreaLeave);
		Messenger.AddListener(GameEvents.GAME_ENDED, Unregister);
	}

	private void OnDisable() {
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, Register);
		Messenger.RemoveListener(GameEvents.PLAYER_ENTERING_AREA, OnAreaEnter);
		Messenger.RemoveListener(GameEvents.PLAYER_LEAVING_AREA, OnAreaLeave);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, Unregister);
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

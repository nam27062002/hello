using UnityEngine;
using UnityEngine.Events;

public class QuitPlayBackButtonHandler : BackButtonHandler {
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	private void OnEnable() {
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, Register);
		Messenger.AddListener(GameEvents.GAME_ENDED, Unregister);
	}

	private void OnDisable() {
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, Register);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, Unregister);
	}

	public override void Trigger() {
		PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
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

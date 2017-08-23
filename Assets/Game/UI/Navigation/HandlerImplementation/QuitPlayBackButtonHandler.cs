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
		//TODO: add a confirmation popup
		if (InstanceManager.gameSceneController != null) {
			InstanceManager.gameSceneController.EndGame(true);
		}
	}
}

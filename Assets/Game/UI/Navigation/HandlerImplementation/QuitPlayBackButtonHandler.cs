using UnityEngine;
using UnityEngine.Events;

public class QuitPlayBackButtonHandler : BackButtonHandler {
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	private void OnEnable() {
		Register();
	}

	private void OnDisable() {
		Unregister();
	}

	public override void Trigger() {
		//TODO: add a confirmation popup
		if (InstanceManager.gameSceneController != null) {
			InstanceManager.gameSceneController.EndGame(true);
		}
	}
}

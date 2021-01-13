using UnityEngine;
using UnityEngine.Events;


public class ExitGameBackButtonHandler : BackButtonHandler {
	
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
		PopupManager.OpenPopupInstant(PopupExitGameConfirmation.PATH);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupExitRunConfirmation : MonoBehaviour {

	public const string PATH = "UI/Popups/Message/PF_PopupConfirmationExitRun";

	public void OnExitRun() {
		if (InstanceManager.gameSceneController != null) {
			InstanceManager.gameSceneController.EndGame(true);
		}

		GetComponentInParent<PopupController>().Close(true);
	}

	/// <summary>
	/// The "Don't show again" toggle has been changed.
	/// </summary>
	/// <param name="_newValue">New value of the toggle.</param>
	public void OnDontShowAgainToggled(bool _newValue) {
		// Store settings
		GameSettings.Set(GameSettings.SHOW_EXIT_RUN_CONFIRMATION_POPUP, !_newValue);	// Inverse!
	}
}

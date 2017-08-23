﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupExitGameConfirmation : MonoBehaviour {

	public const string PATH = "UI/Popups/Message/PF_PopupConfirmationExitGame";

	public void OnExitGame() {
		Application.Quit();
		GetComponentInParent<PopupController>().Close(true);
	}
}

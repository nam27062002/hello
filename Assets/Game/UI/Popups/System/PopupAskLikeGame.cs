using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupAskLikeGame : MonoBehaviour {

	public const string PATH = "UI/Popups/Message/PF_PopupAskLikeGame";

	public void OnYes()
	{
		GetComponent<PopupController>().Close(true);
		PopupManager.OpenPopupInstant(PopupAskRateUs.PATH);
	}

	public void OnNo()
	{
		GetComponent<PopupController>().Close(true);
		PopupManager.OpenPopupInstant(PopupAskFeedback.PATH);
	}
}

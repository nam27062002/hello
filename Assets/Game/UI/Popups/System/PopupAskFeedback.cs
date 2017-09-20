using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Mail;

[RequireComponent(typeof(PopupController))]
public class PopupAskFeedback : MonoBehaviour {

	public const string PATH = "UI/Popups/Message/PF_PopupAskFeedback";

	public void OnNeverAskAgain()
	{
		// Set to never ask again
		Prefs.SetBoolPlayer( Prefs.RATE_CHECK, false );
		GetComponent<PopupController>().Close(true);
	}

	public void OnYes()
	{
		// Set to never ask again
		Prefs.SetBoolPlayer( Prefs.RATE_CHECK, false );

		// Open feedback link/email
		MiscUtils.SendFeedbackEmail();

		GetComponent<PopupController>().Close(true);
	}
}

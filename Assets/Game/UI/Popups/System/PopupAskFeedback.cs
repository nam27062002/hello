using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Mail;

[RequireComponent(typeof(PopupController))]
public class PopupAskFeedback : MonoBehaviour {

	public const string PATH = "UI/Popups/Message/PF_PopupAskFeedback";

	public void OnNeverAskAgain()
	{
        HDTrackingManager.Instance.Notify_RateThisApp(HDTrackingManager.ERateThisAppResult.No);

        // Set to never ask again
        Prefs.SetBoolPlayer( Prefs.RATE_CHECK, false );
		GetComponent<PopupController>().Close(true);
	}

	public void OnYes()
	{
        // "No" is sent as a parameter because the user is saying yes to send feedback about why she doesn't like the game
        HDTrackingManager.Instance.Notify_RateThisApp(HDTrackingManager.ERateThisAppResult.No);

        // Set to never ask again
        Prefs.SetBoolPlayer( Prefs.RATE_CHECK, false );

		// Open feedback link/email
		MiscUtils.SendFeedbackEmail();

		GetComponent<PopupController>().Close(true);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Mail;

[RequireComponent(typeof(PopupController))]
public class PopupAskFeedback : MonoBehaviour {

	public const string PATH = "UI/Popups/Message/PF_PopupAskFeedback";

	const string SUPPORT_EMAIL_ADDRESS = "hungrydragon-support@ubisoft.com";
	const string SUPPORT_EMAIL_SUBJECT = "Hungry Dragon Feedback";

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
		string subject = SUPPORT_EMAIL_SUBJECT;
        string mailSubject = WWW.EscapeURL(subject).Replace("+", "%20");
        Application.OpenURL("mailto:" + SUPPORT_EMAIL_ADDRESS + "?subject=" + mailSubject); // + "&body=" + body);

		GetComponent<PopupController>().Close(true);
	}
}

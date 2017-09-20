using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscUtils {

	const string SUPPORT_EMAIL_ADDRESS = "hungrydragon-support@ubisoft.com";
	const string SUPPORT_EMAIL_SUBJECT = "Hungry Dragon Feedback";

	public static void SendFeedbackEmail(){
		// Open feedback link/email
		string subject = SUPPORT_EMAIL_SUBJECT;
        string mailSubject = WWW.EscapeURL(subject).Replace("+", "%20");
        Application.OpenURL("mailto:" + SUPPORT_EMAIL_ADDRESS + "?subject=" + mailSubject); // + "&body=" + body);
	}
}

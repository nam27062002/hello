// CPTrackingSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tools for tracking testing in the control panel.
/// </summary>
public class CPTrackingSettings : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private TMP_InputField m_emailInput = null;
    [SerializeField] private GameObject[] m_trackingGOS = null;    

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    void Start()
    {
        if (!FeatureSettingsManager.instance.IsMiniTrackingEnabled) {
            if (m_trackingGOS != null) {
                int count = m_trackingGOS.Length;
                for (int i = 0; i < count; i++) {
                    if (m_trackingGOS[i] != null)
                        m_trackingGOS[i].SetActive(false);
                }
            }           
        }
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Simple email address validator.
    /// </summary>
    /// <returns>The corrected e-mail address extracted from the input text.</returns>
    /// <param name="_emailAddress">Attempted e-mail address.</param>
    private string ValidateEmail(string _emailAddress) {
		// System.Net.Mail provides us with some simple validation, but it could still be including wrong addresses - don't give a sh*t
		try {
			_emailAddress = new MailAddress(_emailAddress).Address;
			return _emailAddress;
		} catch(System.FormatException) {
			// Address is invalid
			return "";
		}
	}

	/// <summary>
	/// Internal method to encode texts so they are valid for URLs.
	/// </summary>
	/// <returns>The escaped text.</returns>
	/// <param name="_text">The text to be escaped.</param>
	private string EscapeText(string _text) {
		return WWW.EscapeURL(_text).Replace("+","%20");
	}

	private IEnumerator SendMailAsync() {
		// Check required references
		if(m_emailInput == null) yield break;

		// Some simple e-mail address validation
		string emailAddress = ValidateEmail(m_emailInput.text);

		// Make sure tracking file exists
		string trackingFilePath = MiniTrackingEngine.filePath;
		if(!File.Exists(trackingFilePath)) yield break;

		// Compose email
		// http://answers.unity3d.com/questions/1184875/email-attachment-on-android-ios.html
		// Header
		MailMessage mail = new MailMessage();
		mail.From = new MailAddress("alger.ortin@ubisoft.com");
		mail.To.Add("alger.ortin@ubisoft.com");
		mail.To.Add(emailAddress);
		mail.Subject = "[HD] Tracking Data from device " + SystemInfo.deviceName;

		// Body
		StringBuilder sb = new StringBuilder();
		sb.Append("Timestamp: ").AppendLine(DateTime.UtcNow.ToString());
		sb.Append("Platform: ").AppendLine(Application.platform.ToString());
		sb.Append("Device Model: ").AppendLine(SystemInfo.deviceModel);
		sb.Append("Device Name: ").AppendLine(SystemInfo.deviceName);
		sb.Append("Version: ").AppendLine(GameSettings.internalVersion.ToString());
		sb.AppendLine().AppendLine("Enjoy!").AppendLine("AOC");
		mail.Body = sb.ToString();

		// Attachments
		// Add timestamp information for the file
		Attachment atch = new Attachment(trackingFilePath, MediaTypeNames.Application.Octet);
		ContentDisposition disposition = atch.ContentDisposition;
		disposition.CreationDate = File.GetCreationTime(trackingFilePath);
		disposition.ModificationDate = File.GetLastWriteTime(trackingFilePath);
		disposition.ReadDate = File.GetLastAccessTime(trackingFilePath);
		mail.Attachments.Add(atch);

		// Configure smtp server
		SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
		smtpClient.Port = 587;	// Original example
		smtpClient.Timeout = 10000;

		// Credentials
		smtpClient.Credentials = (ICredentialsByHost)(new NetworkCredential("ubispain@gmail.com", "Ubisoft75*"));
		//smtpClient.Credentials = (ICredentialsByHost) CredentialCache.DefaultNetworkCredentials;

		// Credentials 2.0
		// http://answers.unity3d.com/questions/46752/unity-3-sending-email-with-c.html
		/*NetworkCredential myCred = new NetworkCredential("ubispain@gmail.com", "Ubisoft75*");
		CredentialCache myCache = new CredentialCache();
		myCache.Add("smtp.gmail.com", 587, "Basic", myCred);
		//myCache.Add(new Uri("smtp.gmail.com"), "Basic", myCred);
		smtpClient.Credentials = myCache;*/

		// Encription
		smtpClient.EnableSsl = true;
		ServicePointManager.ServerCertificateValidationCallback = 
			delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return true;
		};

		// Send e-mail!
		try {
			smtpClient.Send(mail);
			UIFeedbackText.CreateAndLaunch("Email successfully sent!", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
		} catch(Exception _e) {
			UIFeedbackText feedback = UIFeedbackText.CreateAndLaunch(
				"Error sending mail!\n<font=\"FNT_Default/FNT_Default\">" + _e.Message + "\n" + _e.GetBaseException() + "</font>", 
				new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText"
			);
			feedback.text.fontSize = 40f;
			feedback.text.color = Colors.red;
			feedback.sequence.timeScale = 1/10f;	// Enough time to read!
			Debug.Log(_e.Message);
			Debug.Log(_e.GetBaseException());
		}

		yield return null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Delete local mini-tracking file.
	/// </summary>
	public void OnDeleteTrackingFile() {
		MiniTrackingEngine.DeleteTrackingFile();
	}

	/// <summary>
	/// Send tracking data to the target e-mail address.
	/// </summary>
	public void OnSendTrackingData() {
		// Check required references
		if(m_emailInput == null) return;

		// Some simple e-mail address validation
		string emailAddress = ValidateEmail(m_emailInput.text);
		emailAddress = EscapeText(emailAddress);

		// Compose mail
		string subject = EscapeText("[HD] Tracking Data from device " + SystemInfo.deviceName);
		string body = EscapeText(MiniTrackingEngine.ReadTrackingFile());
		Application.OpenURL("mailto:" + emailAddress + "?subject=" + subject + "&body=" + body);
	}

	/// <summary>
	/// Send tracking data to the target e-mail address.
	/// </summary>
	public void OnSendTrackingFile() {
		StartCoroutine(SendMailAsync());
	}
}
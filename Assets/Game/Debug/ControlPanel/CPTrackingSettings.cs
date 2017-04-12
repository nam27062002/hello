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
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

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
		// Check required references
		if(m_emailInput == null) return;

		// Some simple e-mail address validation
		string emailAddress = ValidateEmail(m_emailInput.text);

		// Make sure tracking file exists
		string trackingFilePath = MiniTrackingEngine.filePath;
		if(!File.Exists(trackingFilePath)) return;

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

		// Send e-mail!
		SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
		smtpServer.Port = 587;	// Original example
		smtpServer.Credentials = (ICredentialsByHost)(new NetworkCredential("ubispain@gmail.com", "Ubisoft75*"));
		smtpServer.EnableSsl = true;
		ServicePointManager.ServerCertificateValidationCallback = 
			delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return true;
		};

		try {
			smtpServer.Send(mail);
		} catch(Exception _e) {
			Debug.Log(_e.Message);
			Debug.Log(_e.GetBaseException());
		}
	}
}
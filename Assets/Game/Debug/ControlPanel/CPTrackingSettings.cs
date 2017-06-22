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
using System.Collections.Generic;

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
		MiniTrackingEngine.SendTrackingFile(
			false,
			(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) =>
			{
				if(_error == null) {
					Debug.Log("Play test tracking sent successfully");
					UIFeedbackText.CreateAndLaunch("Tracking data sent successfully", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
				} else {
					Debug.Log("<color=red>Error when sending play test tracking</color>");
					UIFeedbackText text = UIFeedbackText.CreateAndLaunch("Error sending tracking data.\nMake sure your internet connection is active and try again.", new Vector2(0.5f, 0.5f), ControlPanel.panel.parent as RectTransform, "CPFeedbackText");
					text.text.color = Color.red;
				}
			}
		);
	}
}
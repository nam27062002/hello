// CPServer.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// All cheats/shortcuts related to server.
/// </summary>
public class CPServerTab : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private ScrollRect m_outputScroll = null;
	[SerializeField] private Text m_outputText = null;



	// Internal
	private DateTime m_startTimestamp;
	private StringBuilder m_outputSb = new StringBuilder();

    //private RequestNetwork requestNetwork;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required refs
		Debug.Assert(m_outputScroll != null, "Required field missing!");
		Debug.Assert(m_outputText != null, "Required field missing!");

		// Clear console upon awakening
		m_startTimestamp = DateTime.UtcNow;
		OnClearConsoleButton();

        //RequestNetworkOnline.CreateInstance();
        //requestNetwork = new RequestNetworkOnline();

    }

    private void Update()
    {
        //requestNetwork.Update();
    }

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Given an InputField game object, extract the text from it.
	/// </summary>
	/// <returns>The text introduced in the given InputField.</returns>
	/// <param name="_inputField">The game object containing the target InputField.</param>
	private string GetInputText(GameObject _inputField) {
		InputField inputField = _inputField.FindComponentRecursive<InputField>();
		if(inputField != null) {
			return inputField.text;
		}
		return "";
	}

	/// <summary>
	/// Add a new line into the output console.
	/// </summary>
	/// <param name="_text">The text to be output.</param>
	private void Output(string _text) {
		// Add new line and timestamp
		if(m_outputSb.Length > 0) m_outputSb.AppendLine();	// Don't add new line for the very first line
		TimeSpan t = DateTime.UtcNow.Subtract(m_startTimestamp);
		m_outputSb.AppendFormat("<color={4}>{0:D2}:{1:D2}:{2:D2}.{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds, Colors.WithAlpha(Colors.white, 0.25f).ToHexString("#"));	// [AOC] Unfortunately, this version of Mono still doesn't support TimeSpan formatting (added at .Net 4)
		m_outputSb.Append(": </color>");

		// Add text
		m_outputSb.Append(_text);

		// Set text
		m_outputText.text = m_outputSb.ToString();

		// Update scroll
		// Don't reset scroll if the scroll position was manually set (different than 0)
		Debug.Log(m_outputScroll.verticalNormalizedPosition); 
		if(m_outputScroll.verticalNormalizedPosition < 0.01f) {	// Error margin
			StartCoroutine(ResetScrollPos());
		}

	}

	/// <summary>
	/// Reset scroll position with a small delay.
	/// We need to do it delayed since the layout is not updated until the next frame.
	/// </summary>
	private IEnumerator ResetScrollPos() {
		//yield return new WaitForSeconds(0.1f);
		yield return new WaitForEndOfFrame();
		m_outputScroll.normalizedPosition = Vector2.zero;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Generic button callback.
	/// </summary>
	public void OnButton1(GameObject _input) {
		// Get optional parameters
		string paramString = GetInputText(_input);

		// Do stuff!
		// [AOC] @Nacho, ni puta idea de como funciona el sistema de networking que se trajo Miguel del SS
		//		 Por lo que he visto por encima, la cosa está entre las siguientes clases:
		// - NetworkManager
		// - Server
		// - RequestNetworkOnline - aquí hay definidas las URL de los diferentes servers (dev, stage, prod, etc.), pero ni idea de como se usa
		//
		// Good luck!
		Debug.Log("Button 1 pressed with params " + paramString + " - TODO!!");
		Output("Button 1 pressed with params " + paramString);

        //requestNetwork.Login();
    }

	/// <summary>
	/// Generic button callback.
	/// </summary>
	public void OnButton2(GameObject _input) {
		// Get optional parameters
		string paramString = GetInputText(_input);

		// Do stuff!
		Debug.Log("Button 2 pressed with params " + paramString + " - TODO!!");
		Output("Button 2 pressed with params " + paramString);
	}

	/// <summary>
	/// Generic button callback.
	/// </summary>
	public void OnButton3(GameObject _input) {
		// Get optional parameters
		string paramString = GetInputText(_input);

		// Do stuff!
		Debug.Log("Button 3 pressed with params " + paramString + " - TODO!!");
		Output("Button 3 pressed with params " + paramString);
	}

	/// <summary>
	/// Generic button callback.
	/// </summary>
	public void OnButton4(GameObject _input) {
		// Get optional parameters
		string paramString = GetInputText(_input);

		// Do stuff!
		Debug.Log("Button 4 pressed with params " + paramString + " - TODO!!");
		Output("Button 4 pressed with params " + paramString);
	}

	/// <summary>
	/// Clear console button has been pressed.
	/// </summary>
	public void OnClearConsoleButton() {
		m_outputSb.Length = 0;
		Output("Hungry Dragon v" + GameSettings.internalVersion + " console output");
	}
}
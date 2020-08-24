// PopupLoading.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/08/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple popup displaying a loading widget and optionally a message.
/// </summary>
public class PopupLoading : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Message/PF_PopupLoading";
	public const string PATH_LITE = "UI/Popups/Message/PF_PopupLoadingLite";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Localizer m_messageText = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Hide message by default
		if(m_messageText != null) m_messageText.gameObject.SetActive(false);
	}

	/// <summary>
	/// Initialize the popup with a specific message.
	/// </summary>
	/// <param name="_messageTid">The TID to display. If null or empty, the message textfield won't be displayed</param>
	/// <param name="_replacements">Text replacements.</param>
	public void InitWithText(string _messageTid, params string[] _replacements) {
		// Set text
		if(m_messageText != null) {
			bool validText = !string.IsNullOrEmpty(_messageTid);
			
			// Just disable if provided text is null
			m_messageText.gameObject.SetActive(validText);

			// Set text
			if(validText) m_messageText.Localize(_messageTid, _replacements);
		}
	}
}
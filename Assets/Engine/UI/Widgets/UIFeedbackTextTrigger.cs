// UIFeedbackTextTrigger.cs
// 
// Created by Alger Ortín Castellví on 23/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Component to easily spawn UIFeedbackTexts.
/// </summary>
public class UIFeedbackTextTrigger : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed Setup
	[Header("Required")]
	[SerializeField] private string m_text = "";
	[SerializeField] private Vector2 m_position = new Vector2(0.5f, 0.5f);
	[SerializeField] private RectTransform m_parent = null;
	[Space]
	[Header("Optional")]
	[SerializeField] private string m_name = "UIFeedbackText";
	[SerializeField] private string m_prefabPath = "";
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show the Feedback Text!
	/// </summary>
	public void Show() {
		// Just do it!
		UIFeedbackText.CreateAndLaunch(
			m_text, 
			m_position, 
			m_parent, 
			m_name,
			m_prefabPath
		);
	}
}
// MenuWipButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Text;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for the big play button in the menu.
/// </summary>
public class MenuWipButton : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private string m_tid = "TID_GEN_COMING_SOON";
	[SerializeField] private Vector2 m_pos = new Vector2(0.5f, 0.5f);

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnWipButton() {
		// Show feedback
        UIFeedbackText target = UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize(m_tid), m_pos, transform.parent as RectTransform);
	}
}

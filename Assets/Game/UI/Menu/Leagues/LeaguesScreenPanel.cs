// LeaguesScreenPanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Panel corresponding to a specific event state.
/// </summary>
public class LeaguesScreenPanel : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Public Properties
	public LeaguesScreenController.Panel panelId {
		get; set;
	}

	public ShowHideAnimator anim {
		get; set;
	}

	// Exposed setup
	[SerializeField] private bool m_darkBackground = false;
	public bool darkBackground {
		get { return m_darkBackground; }
	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATES													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh displayed data.
	/// </summary>
	public virtual void Refresh() {
		// To be implemented by heirs, if needed
	}
}
// MenuPlayScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on //2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuPlayScreen : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public CanvasGroup m_hud = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Find and store reference to the HUD object
		m_hud = GameObject.Find("PF_MenuHUD").GetComponent<CanvasGroup>();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Hide menu HUD
		// [AOC] For some fucking unknown reason, enabling/disabling the hud object at this point crashes Unity -______-
		//		 Looks related to this being called from an animation event or something like that
		//		 That's the reason we're using alpha instead
		//		 http://answers.unity3d.com/questions/631084/setactivefalse-does-not-fire-ondisable-1.html
		if(m_hud != null) m_hud.alpha = 0f;
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Show menu HUD
		// [AOC] For some fucking unknown reason, enabling/disabling the hud object at this point crashes Unity -______-
		//		 Looks related to this being called from an animation event or something like that
		//		 That's the reason we're using alpha instead
		//		 http://answers.unity3d.com/questions/631084/setactivefalse-does-not-fire-ondisable-1.html
		if(m_hud != null) {
			m_hud.DOFade(1f, 0.15f);
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}
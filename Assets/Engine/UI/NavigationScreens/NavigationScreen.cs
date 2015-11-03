// NavigationScreen.cs
// 
// Created by Alger Ortín Castellví on 03/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Shared class between all screens in a navigation screen system.
/// </summary>
public class NavigationScreen : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum AnimDir {
		BACK = -1,
		NEUTRAL = 0,
		FORWARD = 1
	}
	
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private Animator m_anim = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Get animator reference
		m_anim = GetComponent<Animator>();
	}
	
	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Show this screen, using animation if any.
	/// </summary>
	/// <param name="_dir">Direction of the animation.</param>
	public void Show(AnimDir _dir) {
		// Make sure screen is active
		gameObject.SetActive(true);
		
		// If we have an animator, launch "in" animation
		if(m_anim != null) {
			m_anim.SetInteger("direction", (int)_dir);
			m_anim.SetTrigger("in");
		}
	}
	
	/// <summary>
	/// Hide this screen, using animation if any.
	/// </summary>
	/// <param name="_dir">Direction of the animation.</param>
	public void Hide(AnimDir _dir) {
		// Do we have an animator?
		if(m_anim != null) {
			// Yes! Launch "out" animation - it should disable the screen when finishing
			m_anim.SetInteger("direction", (int)_dir);
			m_anim.SetTrigger("out");
		} else {
			// No! Automatically disable screen
			gameObject.SetActive(false);
		}
	}
}
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
	public enum AnimType {
		BACK = -1,
		NEUTRAL = 0,
		FORWARD = 1,
		NONE,
		AUTO
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[Comment("Optional name to navigate through screens using identifiers")]
	[SerializeField] private string m_name = "";
	public string name {
		get { return m_name; }
		set { m_name = value; }
	}

	// References
	private Animator m_anim = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	virtual protected void Awake() {
		// Get animator reference
		m_anim = GetComponent<Animator>();
	}
	
	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Show this screen, using animation if any.
	/// </summary>
	/// <param name="_animType">Direction of the animation.</param>
	public void Show(AnimType _animType) {
		// Make sure screen is active
		gameObject.SetActive(true);

		// If we have an animator, launch "in" animation
		if(m_anim != null && _animType != AnimType.NONE) {
			if(_animType == AnimType.AUTO) _animType = AnimType.NEUTRAL;	// [AOC] "AUTO" can only be used in a navigation screen system
			m_anim.SetInteger("direction", (int)_animType);
			m_anim.SetTrigger("in");
		}
	}
	
	/// <summary>
	/// Hide this screen, using animation if any.
	/// </summary>
	/// <param name="_animType">Direction of the animation.</param>
	public void Hide(AnimType _animType) {
		// If we have an animator, launch "out" animation - it should disable the screen when finishing
		// Otherwise, disable the screen instantly
		if(m_anim != null && _animType != AnimType.NONE) {
			if(_animType == AnimType.AUTO) _animType = AnimType.NEUTRAL;	// [AOC] "AUTO" can only be used in a navigation screen system
			m_anim.SetInteger("direction", (int)_animType);
			m_anim.SetTrigger("out");
		} else {
			gameObject.SetActive(false);
		}
	}
}
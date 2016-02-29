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
	private Animator m_unityAnimator = null;
	private ShowHideAnimator m_showHideAnimator = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	virtual protected void Awake() {
		// Get animator reference
		m_unityAnimator = GetComponent<Animator>();
		m_showHideAnimator = GetComponent<ShowHideAnimator>();
	}
	
	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Show this screen, using animation if any.
	/// </summary>
	/// <param name="_animType">Direction of the animation.</param>
	public void Show(AnimType _animType) {
		// Aux vars
		bool useAnim = (_animType != AnimType.NONE);

		// Make sure screen is active
		gameObject.SetActive(true);

		// If we have an animator, launch "in" animation
		if(m_unityAnimator != null && useAnim) {
			if(_animType == AnimType.AUTO) _animType = AnimType.NEUTRAL;	// [AOC] "AUTO" can only be used in a navigation screen system
			m_unityAnimator.SetInteger("direction", (int)_animType);
			m_unityAnimator.SetTrigger("in");
		}

		// If the screen has a ShowHideAnimator, use it
		if(m_showHideAnimator != null) {
			m_showHideAnimator.Show(useAnim);
		}

		// Additionally look for all children containing a NavigationShowHideAnimator component and trigger it!
		NavigationShowHideAnimator[] animators = GetComponentsInChildren<NavigationShowHideAnimator>(false);	// Exclude inactive ones - if they're inactive we probably don't want to show them!
		for(int i = 0; i < animators.Length; i++) {
			animators[i].Show(useAnim);
		}
	}
	
	/// <summary>
	/// Hide this screen, using animation if any.
	/// </summary>
	/// <param name="_animType">Direction of the animation.</param>
	public void Hide(AnimType _animType) {
		// Aux vars
		bool useAnim = (_animType != AnimType.NONE);

		// If animation is not required, or both animators are null, disable the screen instantly
		if(!useAnim || (m_unityAnimator == null && m_showHideAnimator == null)) {
			gameObject.SetActive(false);
		}

		// If we have an animator, launch "out" animation - it should disable the screen when finishing
		if(m_unityAnimator != null && useAnim) {
			if(_animType == AnimType.AUTO) _animType = AnimType.NEUTRAL;	// [AOC] "AUTO" can only be used in a navigation screen system
			m_unityAnimator.SetInteger("direction", (int)_animType);
			m_unityAnimator.SetTrigger("out");
		} 

		// If the screen has a ShowHideAnimator, use it
		if(m_showHideAnimator != null) {
			m_showHideAnimator.Hide(useAnim);
		}

		// Additionally look for all children containing a NavigationShowHideAnimator component and trigger it!
		NavigationShowHideAnimator[] animators = GetComponentsInChildren<NavigationShowHideAnimator>(false);	// Exclude inactive ones - they are already hidden! ^_^
		for(int i = 0; i < animators.Length; i++) {
			// IMPORTANT!! Hide but don't disable them, otherwise they won't be shown again when navigating back to this screen
			animators[i].Hide(useAnim, false);
		}
	}
}
// NavigationScreen.cs
// 
// Created by Alger Ortín Castellví on 03/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

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
	public string screenName {
		get { return m_name; }
		set { m_name = value; }
	}

	[SerializeField] private bool m_allowBackToThisScreen = true;
	public bool allowBackToThisScreen {
		get { return m_allowBackToThisScreen; }
		set { m_allowBackToThisScreen = value; }
	}

    [Space]
    [SerializeField] private bool m_allowMultiTouch = false;

	// Events
	public UnityEvent OnShow = new UnityEvent();
	public UnityEvent OnHide = new UnityEvent();

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
        // we are disabling multi touch to aviod players messing around with the menus.
        Input.multiTouchEnabled = m_allowMultiTouch;

		// Aux vars
		bool useAnim = (_animType != AnimType.NONE);

		// Make sure screen is active
		gameObject.SetActive(true);

		// If we have an animator, launch "in" animation
		if(m_unityAnimator != null && useAnim) {
			if(_animType == AnimType.AUTO) _animType = AnimType.NEUTRAL;	// [AOC] "AUTO" can only be used in a navigation screen system
			m_unityAnimator.SetInteger( GameConstants.Animator.DIR , (int)_animType);
			m_unityAnimator.SetTrigger( GameConstants.Animator.IN );
		}

		// If the screen has a ShowHideAnimator, use it
		if(m_showHideAnimator != null) {
			//m_showHideAnimator.ForceShow(useAnim);
			if(useAnim) {
				m_showHideAnimator.RestartShow();
			} else {
				m_showHideAnimator.ForceShow(false);
			}
		}

		// Additionally look for all children containing a NavigationShowHideAnimator component and trigger it!
		// [AOC] Now animators automatically trigger nested animators, so this is no longer needed. Leave it in case we have issues with the new feature.
		/*NavigationShowHideAnimator[] animators = GetComponentsInChildren<NavigationShowHideAnimator>(false);	// Exclude inactive ones - if they're inactive we probably don't want to show them!
		for(int i = 0; i < animators.Length; i++) {
			// Skip ourselves
			if(animators[i] == m_showHideAnimator) continue;
			animators[i].RestartShow();
		}*/

		// Notify listeners
		OnShow.Invoke();
	}
	
	/// <summary>
	/// Hide this screen, using animation if any.
	/// </summary>
	/// <param name="_animType">Direction of the animation.</param>
	public void Hide(AnimType _animType) {
		// Aux vars
		bool useAnim = (_animType != AnimType.NONE);
		bool applied = false;

		// If we have an animator, launch "out" animation - it should disable the screen when finishing
		if(m_unityAnimator != null && useAnim) {
			if(_animType == AnimType.AUTO) _animType = AnimType.NEUTRAL;	// [AOC] "AUTO" can only be used in a navigation screen system
			m_unityAnimator.SetInteger( GameConstants.Animator.DIR , (int)_animType);
			m_unityAnimator.SetTrigger( GameConstants.Animator.OUT );
			applied = true;
		} 

		// If the screen has a ShowHideAnimator, use it
		if(m_showHideAnimator != null) {
			m_showHideAnimator.ForceHide(useAnim);
			applied = true;
		}

		// Additionally look for all children containing a NavigationShowHideAnimator component and trigger it!
		// [AOC] Now animators automatically trigger nested animators, so this is no longer needed. Leave it in case we have issues with the new feature.
		/*NavigationShowHideAnimator[] animators = GetComponentsInChildren<NavigationShowHideAnimator>(false);	// Exclude inactive ones - they are already hidden! ^_^
		for(int i = 0; i < animators.Length; i++) {
			// Skip ourselves
			if(animators[i] == m_showHideAnimator) continue;

			// IMPORTANT!! Hide but don't disable them, otherwise they won't be shown again when navigating back to this screen
			animators[i].ForceHide(useAnim, false);
		}*/

		// If no animation was triggered, hide instantly
		if(!applied) {
			gameObject.SetActive(false);
		}

		// Notify listeners
		OnHide.Invoke();
	}
}
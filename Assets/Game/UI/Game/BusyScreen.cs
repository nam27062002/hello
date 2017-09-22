// BusyScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/07/2017.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
// #define DISABLE_BUSY_SCREEN

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Splash screen to show while the game is busy.
/// </summary>
public class BusyScreen : UbiBCN.SingletonMonoBehaviour<BusyScreen> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_animator = null;

	// Internal
	private HashSet<Object> m_owners = new HashSet<Object>();	// HashSet ~= List without duplicates

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Start hidden
		m_animator.Hide(false);
	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Toggle the loading screen on/off.
	/// </summary>
	/// <param name="_show">Whether to show or hide the screen.</param>
	/// <param name="_owner">The object performing the request.</param>
	/// <param name="_animate">Use fade animation?</param>
	public static void Toggle(bool _show, Object _owner, bool _animate = true) {
		#if !DISABLE_BUSY_SCREEN
		// Only hide when there are no owners retaining the screen
		if(_show) {
			if(_owner != null) instance.m_owners.Add(_owner);
			instance.m_animator.Show(_animate);
		} else {
			instance.m_owners.Remove(_owner);
			if(instance.m_owners.Count == 0) {
				instance.m_animator.Hide(_animate);
			}
		}
		#endif
	}

	/// <summary>
	/// Toggle the loading screen on.
	/// </summary>
	/// <param name="_owner">The object performing the request.</param>
	/// <param name="_animate">Fade animation?</param>
	public static void Show(Object _owner, bool _animate = true) {
		Toggle(true, _owner, _animate);
	}

	/// <summary>
	/// Toggle the loading screen off.
	/// </summary>
	/// <param name="_owner">The object performing the request.</param>
	/// <param name="_animate">Fade animation?</param>
	public static void Hide(Object _owner, bool _animate = true) {
		Toggle(false, _owner, _animate);
	}

	/// <summary>
	/// Toggle the loading screen off, clearing the owners stack.
	/// </summary>
	/// <param name="_animate">Fade animation?</param>
	public static void ForceHide(bool _animate = true) {
		// Clear owners stack
		instance.m_owners.Clear();

		// Hide!
		Hide(null, _animate);
	}
}
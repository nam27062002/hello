// BusyScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/07/2017.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
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

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Toggle the loading screen on/off.
	/// </summary>
	/// <param name="_show">Whether to show or hide the screen.</param>
	/// <param name="_animate">Use fade animation?</param>
	public static void Toggle(bool _show, bool _animate = true) {
		// Just let the animator do it
		instance.m_animator.Set(_show, _animate);
	}
}
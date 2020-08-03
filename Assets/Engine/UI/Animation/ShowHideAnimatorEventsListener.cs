// ShowHideAnimatorEventsListener.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/12/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to ShowHideAnimator to propagate Unity's Animator Events to
/// the ShowHideAnimator even when not in the same GameObject.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ShowHideAnimatorEventsListener : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public UnityEvent OnShowCompletedEvent = new UnityEvent();
	public UnityEvent OnHideCompletedEvent = new UnityEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Animation events, must be connected to the animations!
	/// </summary>
   	public void OnShowAnimationCompleted() {
		OnShowCompletedEvent.Invoke();
	}

	/// <summary>
	/// Animation events, must be connected to the animations!
	/// </summary>
	public void OnHideAnimationCompleted() {
		OnHideCompletedEvent.Invoke();
	}
}
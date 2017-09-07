// ResultsScreenStep.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single step on the results screen sequence.
/// Parent class that must be implemented by all steps.
/// When finished, steps must invoke the OnFinished event.
/// </summary>
public abstract class ResultsScreenStep : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private bool m_darkScreen = false;

	// Events
	[HideInInspector] [SerializeField] public UnityEvent OnFinished = new UnityEvent();

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	public void Launch() {
		// Common stuff
		// Toggle dark screen
		ResultsDarkScreen.Set(m_darkScreen);

		// Custom launch implementation
		DoLaunch();
	}

	//------------------------------------------------------------------------//
	// ABSTARCT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	public abstract bool MustBeDisplayed();

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	protected abstract void DoLaunch();
}
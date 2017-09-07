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

	// Internal
	protected ResultsScreenController_NEW m_controller = null;

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this step with the given results screen controller.
	/// </summary>
	/// <param name="_controller">The results screen controller that will be triggering this step.</param>
	public void Init(ResultsScreenController_NEW _controller) {
		// Store reference
		m_controller = _controller;

		// Custom init implementation
		DoInit();
	}

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
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	public abstract bool MustBeDisplayed();

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATES													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Custom initialization of the step.
	/// </summary>
	protected virtual void DoInit() { }

	/// <summary>
	/// Custom launch of the step.
	/// </summary>
	protected virtual void DoLaunch() { }
}
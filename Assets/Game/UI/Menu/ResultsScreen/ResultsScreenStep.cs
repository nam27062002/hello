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
	[SerializeField] private Color m_darkScreenColor = Colors.WithAlpha(Color.black, 0.5f);

	// Events
	[HideInInspector] [SerializeField] public UnityEvent OnFinished = new UnityEvent();

	// Internal
	protected ResultsScreenController m_controller = null;
	protected ResultsScreenController.Step m_stepID = ResultsScreenController.Step.INIT;

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this step with the given results screen controller.
	/// </summary>
	/// <param name="_controller">The results screen controller that will be triggering this step.</param>
	/// <param name="_stepID">Self-awareness of which step we are.</param>
	public virtual void Init(ResultsScreenController _controller, ResultsScreenController.Step _stepID) {
		// Store reference
		m_controller = _controller;

		// Self-awareness of which step we are.
		m_stepID = _stepID;

		// Debug
		ControlPanel.Log("Init Step " + m_stepID, ControlPanel.ELogChannel.ResultsScreen);

		// Custom init implementation
		DoInit();
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	public virtual void Launch() {
		// Common stuff
		// Toggle dark screen
		if(m_darkScreen) {
			// Apply custom color
			ResultsDarkScreen.instance.color = m_darkScreenColor;
			ResultsDarkScreen.Show();
		} else {
			ResultsDarkScreen.Hide();
		}

		// Debug
		ControlPanel.Log("Launching Step " + m_stepID, ControlPanel.ELogChannel.ResultsScreen);

		// Custom launch implementation
		DoLaunch();
	}

	/// <summary>
	/// Shows the summary.
	/// </summary>
	public virtual void ShowSummary() {
		m_controller.summary.GetComponent<ShowHideAnimator>().Show();
	}

	/// <summary>
	/// Hides the summary.
	/// </summary>
	public virtual void HideSummary() {
		m_controller.summary.GetComponent<ShowHideAnimator>().Hide();
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
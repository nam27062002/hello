// ShowOnTutorialStep.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to toggle an object based on current tutorial step.
/// </summary>
public class ShowOnTutorialStep : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[Tooltip("Won't be shown until all target steps are completed")]
	[SerializeField] private TutorialStep[] m_targetSteps;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<TutorialStep, bool>(GameEvents.TUTORIAL_STEP_TOGGLED, OnTutorialStepToggled);

		// Apply initial visibility
		Apply();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Subscribe to external events
		Messenger.RemoveListener<TutorialStep, bool>(GameEvents.TUTORIAL_STEP_TOGGLED, OnTutorialStepToggled);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check current tutorial state and apply visibility.
	/// </summary>
	private void Apply() {
		// Skip if current user profile is not ready
		if(UsersManager.currentUser == null) {
			return;
		}

		// Check whether all target states are completed
		for(int i = 0; i < m_targetSteps.Length; i++) {
			// If a single step is not completed, disable object and return
			if(!UsersManager.currentUser.IsTutorialStepCompleted(m_targetSteps[i])) {
				this.gameObject.SetActive(false);
				return;
			}
		}

		// All steps completed! Activate object
		this.gameObject.SetActive(true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A tutorial step has been toggled.
	/// </summary>
	/// <param name="_step">The step.</param>
	/// <param name="_completed">Whether it has been marked as completed or uncompleted.</param>
	private void OnTutorialStepToggled(TutorialStep _step, bool _completed) {
		// Just apply visibility
		Apply();
	}
}
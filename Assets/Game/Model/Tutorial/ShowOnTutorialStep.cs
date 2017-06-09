// ShowOnTutorialStep.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

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
	private enum Mode {
		GAME_OBJECT_ACTIVATION,
		SELECTABLE_INTERACTABLE,
		CANVAS_GROUP_INTERACTABLE
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Mode m_mode = Mode.GAME_OBJECT_ACTIVATION;
	[SerializeField] private bool m_reverseMode = false;

	[Comment("Won't be activated until all target steps are completed")]
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
		bool toggle = true;
		for(int i = 0; i < m_targetSteps.Length; i++) {
			// If a single step is not completed, disable object and return
			if(!UsersManager.currentUser.IsTutorialStepCompleted(m_targetSteps[i])) {
				toggle = false;
				break;	// No need to keep looping
			}
		}

		// Reverse mode?
		if(m_reverseMode) toggle = !toggle;

		// All steps completed! Apply to object based on mode
		switch(m_mode) {
			case Mode.GAME_OBJECT_ACTIVATION: {
				this.gameObject.SetActive(toggle);
			} break;

			case Mode.SELECTABLE_INTERACTABLE: {
				// Requires a Selectable component
				Selectable selectable = this.GetComponent<Selectable>();
				if(selectable != null) {
					selectable.interactable = toggle;
				} else {
					Debug.LogError("<color=red>SELECTABLE NOT FOUND</color>");
				}
			} break;

			case Mode.CANVAS_GROUP_INTERACTABLE: {
				// Requires a Selectable component
				CanvasGroup canvasGroup = this.GetComponent<CanvasGroup>();
				if(canvasGroup != null) {
					canvasGroup.interactable = toggle;
				} else {
					Debug.LogError("<color=red>CANVAS GROUP NOT FOUND</color>");
				}
			} break;
		}
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
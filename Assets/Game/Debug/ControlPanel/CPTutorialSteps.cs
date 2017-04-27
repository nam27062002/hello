// CPTutorialSteps.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tutorial steps control for the control panel.
/// </summary>
public class CPTutorialSteps : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Auxiliar class
	private class StepToggle {
		public TutorialStep step = TutorialStep.INIT;
		public Toggle toggle = null;
		public GameObject obj = null;
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Transform m_container = null;
	[SerializeField] private GameObject m_stepTogglePrefab = null;

	// Internal
	private List<StepToggle> m_toggles = new List<StepToggle>();
	private bool m_ignoreToggles = true;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Fill container, creating a step group for every tutorial step
		// Enum class makes it easy for us!
		foreach(TutorialStep step in Enum.GetValues(typeof(TutorialStep))) {
			// Skip ALL and INIT steps
			if(step == TutorialStep.INIT || step == TutorialStep.ALL) continue;

			// Create a new toggle object and store it
			StepToggle newToggle = new StepToggle();
			newToggle.step = step;
			m_toggles.Add(newToggle);

			// Instantiate a new toggle for this value
			newToggle.obj = GameObject.Instantiate<GameObject>(m_stepTogglePrefab, m_container, false);

			// Initialize toggle visuals
			// (Toggle will actually be initialized on the OnEnable() call)
			newToggle.toggle = newToggle.obj.GetComponentInChildren<Toggle>();
			newToggle.obj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = step.ToString();

			// Subscribe to toggle change triggers
			newToggle.toggle.onValueChanged.AddListener(
				(bool _toggled) => {
					// Ignore during initialization
					if(m_ignoreToggles) return;

					// Ignore if user data is not loaded
					if(UsersManager.currentUser == null) return;

					// Change target step
					UsersManager.currentUser.SetTutorialStepCompleted(newToggle.step, _toggled);
					PersistenceManager.Save();
				}
			);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Refresh toggles status
		m_ignoreToggles = true;
		for(int i = 0; i < m_toggles.Count; i++) {
			// Is user data ready?
			if(UsersManager.currentUser != null) {
				m_toggles[i].toggle.interactable = true;
				m_toggles[i].toggle.isOn = UsersManager.currentUser.IsTutorialStepCompleted(m_toggles[i].step);
			} else {
				m_toggles[i].toggle.interactable = false;
			}
		}
		m_ignoreToggles = false;

		// Subscribe to external events
		Messenger.AddListener<TutorialStep, bool>(GameEvents.TUTORIAL_STEP_TOGGLED, OnTutorialStepToggled);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		m_ignoreToggles = true;

		// Unsubscribe from external events
		Messenger.RemoveListener<TutorialStep, bool>(GameEvents.TUTORIAL_STEP_TOGGLED, OnTutorialStepToggled);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A tutorial step has been completed/toggled.
	/// </summary>
	/// <param name="_step">Target step.</param>
	/// <param name="_completed">Is the step completed?.</param>
	private void OnTutorialStepToggled(TutorialStep _step, bool _completed) {
		// Skip if ignoring toggles
		if(m_ignoreToggles) return;

		// Update matching toggle
		for(int i = 0; i < m_toggles.Count; i++) {
			if(m_toggles[i].step == _step) {
				m_ignoreToggles = true;
				m_toggles[i].toggle.isOn = _completed;
				m_ignoreToggles = false;
				break;
			}
		}
	}

	/// <summary>
	/// Toggle all steps.
	/// </summary>
	public void OnToggleAll() {
		// Figure out whether to toggle on or off
		int completedCount = 0;
		for(int i = 0; i < m_toggles.Count; i++) {
			if(UsersManager.currentUser.IsTutorialStepCompleted(m_toggles[i].step)) {
				completedCount++;
			}
		}

		// Figure out new value
		bool newValue = true;

		// 3 cases:
		// 1) All completed
		if(completedCount == m_toggles.Count) {
			// Un-complete them
			newValue = false;
		}

		// 2) None completed
		else if(completedCount == 0) {
			// Complete them
			newValue = true;
		}

		// 3) Mixed: change to the state with more steps
		else {
			if(completedCount > m_toggles.Count/2) {
				// Most of them completed: complete the rest
				newValue = true;
			} else {
				// Most of them uncompleted: un-complete the rest
				newValue = false;
			}
		}

		// Apply new value
		for(int i = 0; i < m_toggles.Count; i++) {
			UsersManager.currentUser.SetTutorialStepCompleted(m_toggles[i].step, newValue);
		}
		PersistenceManager.Save();
	}
}
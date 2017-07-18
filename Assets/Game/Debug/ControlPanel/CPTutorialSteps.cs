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
using TMPro;

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
	[Space]
	[SerializeField] private TextMeshProUGUI m_gamesPlayedText = null;

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

		// Refresh games played text
		m_gamesPlayedText.text = UsersManager.currentUser.gamesPlayed.ToString();

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
	/// <param name="_toggle">Whether to toggle them on or off</param>
	public void OnToggleAll(bool _toggle) {
		// Apply new value
		for(int i = 0; i < m_toggles.Count; i++) {
			UsersManager.currentUser.SetTutorialStepCompleted(m_toggles[i].step, _toggle);
		}
		PersistenceManager.Save();
	}

	/// <summary>
	/// Modify the amount of played games.
	/// </summary>
	/// <param name="_amount">Amount. Negative to subtract.</param>
	public void OnAddGamesPlayed(int _amount) {
		// Min 0!
		UsersManager.currentUser.gamesPlayed = Mathf.Max(0, UsersManager.currentUser.gamesPlayed + _amount);

		// Refresh text
		m_gamesPlayedText.text = UsersManager.currentUser.gamesPlayed.ToString();
	}
}
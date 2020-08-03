// TutorialPopupTrigger.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to open a popup if a tutorial step is not completed.
/// </summary>
[RequireComponent(typeof(PopupLauncher))]
public class TutorialPopupTrigger : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private TutorialStep m_targetStep = TutorialStep.INIT;
	[SerializeField] private bool m_checkOnEnable = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		if(m_checkOnEnable) Check();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the assigned step was completed.
	/// If it wasn't, the assigned popup will be opened and the tutorial step marked as completed.
	/// Typically to be assigned to a screen animator's OnShowPostAnimation() event.
	/// </summary>
	public void Check() {
		// Only if active!
		if(!isActiveAndEnabled) return;

		// Is tutorial step completed?
		if(!UsersManager.currentUser.IsTutorialStepCompleted(m_targetStep)) {
			// Open popup!
			GetComponent<PopupLauncher>().OpenPopup();

			// Mark tutorial step as completed
			UsersManager.currentUser.SetTutorialStepCompleted(m_targetStep, true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
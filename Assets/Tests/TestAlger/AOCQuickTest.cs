// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[EnumMask] public TutorialStep m_tutorialStep = TutorialStep.INIT;
	[HideInInspector] public TutorialStep m_setMask = TutorialStep.INIT;
	[HideInInspector] public bool m_setValue = true;

	public UserProfile.SaveData m_saveData = new UserProfile.SaveData();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		/*Debug.Log(m_setMask + ": " + IsTutorialStepCompleted(m_setMask));
		SetTutorialStepCompleted(m_setMask, m_setValue);
		Debug.Log(m_setMask + ": " + IsTutorialStepCompleted(m_setMask));*/

		GameObject profilePrefab = Resources.Load<GameObject>(PersistenceProfile.RESOURCES_FOLDER + "PF_Default");
		if(profilePrefab != null) {
			PersistenceProfile prof = profilePrefab.GetComponent<PersistenceProfile>();
			PersistenceManager.SaveData data = prof.data;
			Debug.Log(data.profile.tutorialStep + " (" + data.profile.tutorialStep.ToString("X") + ")" + "\n" + data.profile.tutorialStep.GetHashCode());
		}
	}

	/// <summary>
	/// Check whether a tutorial step has been completed by this user.
	/// </summary>
	/// <returns><c>true</c> if <paramref name="_step"/> is marked as completed in this profile; otherwise, <c>false</c>.</returns>
	/// <param name="_step">The tutorial step to be checked. Can also be a composition of steps (e.g. (TutorialStep.STEP_1 | TutorialStep.STEP_2), in which case all steps will be tested).</param>
	public bool IsTutorialStepCompleted(TutorialStep _step) {
		// Special case for NONE: ignore
		if(_step == TutorialStep.INIT) return false;
		return (m_tutorialStep & _step) != 0;
	}

	/// <summary>
	/// Mark/unmark a tutorial step as completed.
	/// </summary>
	/// <param name="_step">The tutorial step to be marked. Can also be a composition of steps (e.g. (TutorialStep.STEP_1 | TutorialStep.STEP_2), in which case all steps will be marked).</param>
	/// <param name="_completed">Whether to mark it as completed or uncompleted.</param>
	public void SetTutorialStepCompleted(TutorialStep _step, bool _completed = true) {
		// Special case for NONE: ignore
		if(_step == TutorialStep.INIT) return;
		if(_completed) {
			m_tutorialStep |= _step;
		} else {
			m_tutorialStep &= ~_step;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}
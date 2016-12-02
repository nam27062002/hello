﻿// ResultsSceneController.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.
//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for the results scene.
/// </summary>
public class ResultsSceneController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Camera m_mainCamera;

	[Space]
	[SerializeField] private GameObject m_gameUI;
	[SerializeField] private GameObject m_resultsUI;

	[Space]
	[Tooltip("Default scene setup prefab to be used in levels where no setup can be found.")]
	[SerializeField] private GameObject m_defaultSetupPrefab = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Disable results UI
		m_resultsUI.SetActive(false);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Pick a random results scene setup from the art scene and initializes camera,
	/// dragon and UI.
	/// </summary>
	public void Show() {
		// Turn off main camera
		m_mainCamera.gameObject.SetActive(false);

		// Select a random scene setup and instantiate it
		// Find all scene setup prefabs for the loaded level - we have a special component for that, look for it
		// If no setup is found (i.e. test levels), use the placeholder prefab
		GameObject setupPrefab = m_defaultSetupPrefab;
		ResultsSceneSetupList setupList = GameObject.FindObjectOfType<ResultsSceneSetupList>();
		if(setupList != null && setupList.setupPrefabs.Length > 0) {
			setupPrefab = setupList.setupPrefabs.GetRandomValue();
		}

		// Instantiate the prefab
		GameObject newSetupObj = GameObject.Instantiate<GameObject>(setupPrefab);
		ResultsSceneSetup targetSetup = newSetupObj.GetComponent<ResultsSceneSetup>();

		// Activate and initialize UI, turn off Game UI
		// [AOC] TODO!! Nicer transition
		m_gameUI.SetActive(false);
		m_resultsUI.SetActive(true);
		ResultsScreenController controller = m_resultsUI.GetComponentInChildren<ResultsScreenController>();
		if(controller != null) {
			controller.Init(targetSetup);
			controller.LaunchAnim();
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}

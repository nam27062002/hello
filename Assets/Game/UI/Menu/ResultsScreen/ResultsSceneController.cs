// ResultsSceneController.cs
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
	[SerializeField] private Camera m_resultsCamera;
	
	[SerializeField] private GameObject m_gameUI;
	[SerializeField] private GameObject m_resultsUI;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Disable results camera and UI
		m_resultsCamera.gameObject.SetActive(false);
		m_resultsUI.SetActive(false);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Pick a random results scene setup from the art scene and initializes camera,
	/// dragon and UI.
	/// </summary>
	public void Show() {
		// Switch cameras
		m_mainCamera.gameObject.SetActive(false);
		m_resultsCamera.gameObject.SetActive(true);

		// Activate and initialize UI
		m_gameUI.SetActive(false);
		m_resultsUI.SetActive(true);
		m_resultsUI.GetComponentInChildren<ResultsScreenController>().Initialize();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}

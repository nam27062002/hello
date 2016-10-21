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

	[Space]
	[SerializeField] private GameObject m_gameUI;
	[SerializeField] private GameObject m_resultsUI;

	[Space]
	[Tooltip("Default scene setup prefab to be used in levels where no setup can be found.")]
	[SerializeField] private GameObject m_defaultSetupPrefab = null;

	// Internal
	private ResultsSceneSetup[] m_sceneSetups = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Disable results UI
		m_resultsUI.SetActive(false);

		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe to external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Pick a random results scene setup from the art scene and initializes camera,
	/// dragon and UI.
	/// </summary>
	public void Show() {
		// Activate and initialize UI
		m_gameUI.SetActive(false);
		m_resultsUI.SetActive(true);
		m_resultsUI.GetComponentInChildren<ResultsScreenController>().Initialize();

		// Turn off main camera
		m_mainCamera.gameObject.SetActive(false);

		// Select a random scene setup and launch it!
		ResultsSceneSetup targetSetup = m_sceneSetups.GetRandomValue();
		targetSetup.gameObject.SetActive(true);
		targetSetup.LaunchAnim();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The game level has been loaded.
	/// </summary>
	private void OnLevelLoaded() {
		// Find all suitable result scene setups in the loaded level
		// We have a special component for that, look for it
		ResultsSceneSetupList setupList = GameObject.FindObjectOfType<ResultsSceneSetupList>();
		if(setupList != null && setupList.scenesList.Length > 0) {
			m_sceneSetups = setupList.scenesList;
		} else {
			// If no setup was found (i.e. test levels), use the placeholder prefab
			// Create a new instance and add it to the array
			GameObject newSetupObj = GameObject.Instantiate<GameObject>(m_defaultSetupPrefab);
			m_sceneSetups = new ResultsSceneSetup[1];
			m_sceneSetups[0] = newSetupObj.GetComponent<ResultsSceneSetup>();
		}

		// Disable all found setups
		for(int i = 0; i < m_sceneSetups.Length; i++) {
			m_sceneSetups[i].gameObject.SetActive(false);
		}
	}
}

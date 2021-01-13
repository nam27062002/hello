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
	[SerializeField] private GameObject m_resultsUI;

	[Space]
	[Tooltip("Scene setup where the user's dragon has to be set")]
	[SerializeField] private ResultsSceneSetup m_resultsSceneSetup = null;

	// Public properties
	public Camera mainCamera {
		get { return m_resultsSceneSetup != null ? m_resultsSceneSetup.camera : null; }
	}

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
		// Disable results UI
		m_resultsUI.SetActive(false);

        // Make sure it's not visible until Show() is called
        if (m_resultsSceneSetup != null) {
            m_resultsSceneSetup.gameObject.SetActive(false);
        }

		// Allow unlimited amount of particles
		ParticleManager.instance.poolLimits = ParticleManager.PoolLimits.Unlimited;
		ParticleManager.Clear();
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
        if (m_resultsSceneSetup != null) {
            m_resultsSceneSetup.gameObject.SetActive(true);
        }

        // Activate and initialize UI, turn off Game UI
        // [AOC] TODO!! Nicer transition		
        m_resultsUI.SetActive(true);

		ResultsScreenController controllerNew = m_resultsUI.GetComponentInChildren<ResultsScreenController>();
		if(controllerNew != null) {
			controllerNew.StartFlow(m_resultsSceneSetup);
		}

        // Make sure no slow motion was inherited!
        InstanceManager.timeScaleController.GoingToResults();
		// Time.timeScale = 1f;

        // Start music!
        AudioController.PlayMusic("hd_results_music");
    }

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}

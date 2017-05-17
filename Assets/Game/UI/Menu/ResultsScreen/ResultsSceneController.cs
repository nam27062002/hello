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
	[SerializeField] private GameObject m_resultsUI;

	[Space]
	[Tooltip("Scene setup where the user's dragon has to be set")]
	[SerializeField] private ResultsSceneSetup m_resultsScenesetup = null;   

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
        if (m_resultsScenesetup != null) {
            m_resultsScenesetup.gameObject.SetActive(false);
        }
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
        if (m_resultsScenesetup != null) {
            m_resultsScenesetup.gameObject.SetActive(true);
        }

        // Activate and initialize UI, turn off Game UI
        // [AOC] TODO!! Nicer transition		
        m_resultsUI.SetActive(true);
		ResultsScreenController controller = m_resultsUI.GetComponentInChildren<ResultsScreenController>();
		if(controller != null) {
			controller.Init(m_resultsScenesetup);
			controller.LaunchAnim();
		}

		// Make sure no slow motion was inherited!
		Time.timeScale = 1f;

        // Start music!
        AudioController.PlayMusic("hd_results_music");
    }

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}

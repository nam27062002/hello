// UIScene3DLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar component to automatically load a 3D UI scene into a target Raw Image in the UI.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class UIScene3DLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_scenePrefab = null;

	// Internal
	[Comment("Scene instance exposed for convenience, do not set it from the inspector", 5)]
	[SerializeField] private UIScene3D m_sceneInstance = null;
	public UIScene3D scene {
		get { 
			if(m_sceneInstance == null) LoadScene(m_scenePrefab);
			return m_sceneInstance; 
		}
	}

	private RawImage m_rawImage = null;
	public RawImage rawImage {
		get { return m_rawImage; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_scenePrefab != null, "Required field not initialized!");

		// Initialize external references
		m_rawImage = GetComponent<RawImage>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// If scene wasn't already loaded, do it now
		if(m_sceneInstance == null) {
			LoadScene(m_scenePrefab);
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		UnloadScene();	
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Loads a given scene.
	/// </summary>
	/// <param name="_scenePrefab">Scene prefab.</param>
	public void LoadScene(GameObject _scenePrefab) {
		Debug.Log("LOADING SCENE " + _scenePrefab.name);
		// The prefab must contain a UIScene3D component
		if(_scenePrefab.GetComponent<UIScene3D>() == null) {
			Debug.LogError("Trying to load a scene without the UIScene3D component - aborting");
			return;
		}

		// Unload current scene, if any
		UnloadScene();

		// Load the new scene
		// UIScene3DManager makes it easy for us! d
		m_scenePrefab = _scenePrefab;
		m_sceneInstance = UIScene3DManager.CreateFromPrefab<UIScene3D>(m_scenePrefab);

		// Initialize the raw image where the dragon will be rendered
		m_rawImage.texture = m_sceneInstance.renderTexture;
		m_rawImage.color = Colors.white;
	}

	/// <summary>
	/// Unloads the scene.
	/// </summary>
	public void UnloadScene() {
		// Destroy next dragon 3D scene
		if(m_sceneInstance != null) {
			UIScene3DManager.Remove(m_sceneInstance);
			m_sceneInstance = null;
			m_rawImage.texture = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
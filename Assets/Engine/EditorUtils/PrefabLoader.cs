// PrefabLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Helper class to easily instantiate prefabs into the scene.
/// </summary>
public class PrefabLoader : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[FileListAttribute("Resources", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_resourcePath = "";
	public string resourcePath {
		get { return m_resourcePath; }
		set {
			m_resourcePath = value;
			Reload();
		}
	}

	[SerializeField] private bool m_resetScale = true;
	public bool resetScale {
		get { return m_resetScale; }
		set { m_resetScale = value; }
	}

	[SerializeField] private bool m_keepLayers = false;
	public bool keepLayers {
		get { return m_keepLayers; }
		set { m_keepLayers = value; }
	}

	// Internal
	[HideInInspector]	// Serialize, cause we want to store instances created in edit mode, but don't expose so users don't think it should be set manually
	[SerializeField] private GameObject m_loadedInstance = null;
	public GameObject loadedInstance {
		get { return m_loadedInstance; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialiation.
	/// </summary>
	private void Awake() {
		// Try to find out already instantiated previews of the prefab
		// We'll do it roughly using the name
		foreach(Transform child in transform) {
			if(m_resourcePath.Contains(child.name)) {
				m_loadedInstance = child.gameObject;
				break;
			}
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load assigned prefab.
	/// </summary>
	public void Load() {
		Load(m_resourcePath);
	}

	/// <summary>
	/// Reload prefab based on mode.
	/// </summary>
	public void Reload() {
		Load(m_resourcePath);
	}

	/// <summary>
	/// Load the prefab at the given path
	/// </summary>
	/// <param name="_resourcePath">Full path of the prefab to be loaded, starting inside the Resources folder and without extension.</param>
	public void Load(string _resourcePath) {
		// Unload current prefab if any
		Unload();

		// Load selected prefab
		if(!string.IsNullOrEmpty(_resourcePath)) {
			// Instantiate the prefab and add it as child of this object
			GameObject prefab = Resources.Load<GameObject>(_resourcePath);
			if(prefab != null) {
				m_loadedInstance = GameObject.Instantiate<GameObject>(prefab, this.transform, false);
				m_loadedInstance.transform.localPosition = Vector3.zero;
				m_loadedInstance.transform.localRotation = Quaternion.identity;

				// Reset name (we don't want those annoying "Clone" suffixes added by Unity)
				m_loadedInstance.name = prefab.name;

				// Keep layers?
				if(!m_keepLayers) {
					m_loadedInstance.SetLayerRecursively(this.gameObject.layer);
				}

				// Reset scale if required
				if(m_resetScale) {
					m_loadedInstance.transform.localScale = Vector3.one;
				}
			}
		}

		// Update path
		m_resourcePath = _resourcePath;
	}

	/// <summary>
	/// Destroy current loaded prefab, if any.
	/// </summary>
	public void Unload() {
		if(m_loadedInstance != null) {
			GameObject.DestroyImmediate(m_loadedInstance);	// Immediate so it can be called from the editor
			m_loadedInstance = null;
		}

		/*// Destroy the instance and clear reference
		foreach(Transform child in transform) {
			GameObject.DestroyImmediate(child.gameObject);	// Immediate so it can be called from the editor
		}
		m_loadedInstance = null;*/
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}

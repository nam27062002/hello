// UI3DLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to load 3D prefabs into a UI canvas.
/// </summary>
public class UI3DLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public class UI3DLoaderEvent : UnityEvent<UI3DLoader> {}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[FileListAttribute("Resources", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_resourcePath = "";
	[SerializeField] private bool m_loadOnAwake = false;
	[Space]
	[SerializeField] private Transform m_container = null;
	[SerializeField] private UI3DScaler m_scaler = null;
	[Space]
	[SerializeField] private GameObject m_loadingPrefab = null;

	// Internal
	private ResourceRequest m_loadingRequest = null;
	public ResourceRequest loadingRequest {
		get { return m_loadingRequest; }
	}

	private GameObject m_instance = null;
	public GameObject instance {
		get { return m_instance; }
	}

	private GameObject m_loadingInstance = null;

	// Events
	public UI3DLoaderEvent OnLoadingComplete = new UI3DLoaderEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Show loading icon from start
		ShowLoading(true);

		// If defined, start loading
		if(m_loadOnAwake) {
			Load();
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// A change has been done in the inspector.
	/// </summary>
	private void OnDestroy() {
		// Delete instance and requests
		if(m_loadingRequest != null) {
			m_loadingRequest = null;
		}

		if(m_instance != null) {
			GameObject.Destroy(m_instance);
			m_instance = null;
		}

		// Destroy loading icon
		ShowLoading(false);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// If we're loading, wait for the loading process to end
		if(m_loadingRequest != null) {
			if(m_loadingRequest.isDone) {
				// Done! Instantiate loaded asset
				m_instance = GameObject.Instantiate<GameObject>((GameObject)m_loadingRequest.asset, m_container.transform, false);
				if(m_instance != null) {
					// Apply layer
					m_instance.SetLayerRecursively(m_container.gameObject.layer);

					// Refresh the scaler
					if(m_scaler != null) {
						m_scaler.Refresh(true, true);
					}
				}

				// Hide loading icon
				ShowLoading(false);

				// Notify subscribers
				OnLoadingComplete.Invoke(this);
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start loading the prefab asynchronously. The OnLoadComplete event will be
	/// triggered when the loading has finished.
	/// If an instance was already loaded, it will be replaced.
	/// </summary>
	/// <returns>The async resources request started. Same as accessing the loadingRequest property.</returns>
	public ResourceRequest Load() {
		// If we have something loaded, destroy it
		if(m_instance != null) {
			GameObject.Destroy(m_instance);
			m_instance = null;
		}

		// We don't care if we're already loading another asset, it will be ignored once done loading
		m_loadingRequest = Resources.LoadAsync<GameObject>(m_resourcePath);
		ShowLoading(true);

		return m_loadingRequest;
	}

	/// <summary>
	/// Show/hide loading icon.
	/// Since loading is usually only done once, the icon will be 
	/// instantiated/destroyed every time to free resources.
	/// </summary>
	/// <param name="_show">Whether to show or hide the icon.</param>
	private void ShowLoading(bool _show) {
		if(_show) {
			// If loading icon not instantiated, do it now
			if(m_loadingInstance != null) {
				if(m_loadingPrefab != null) {
					m_loadingInstance = GameObject.Instantiate<GameObject>(m_loadingPrefab, this.transform, false);
				}
			} else {
				m_loadingInstance.SetActive(true);
			}
		} else {
			if(m_loadingInstance != null) {
				GameObject.Destroy(m_loadingInstance);
				m_loadingInstance = null;
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
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

	private GameObject m_loadedInstance = null;
	public GameObject loadedInstance {
		get { return m_loadedInstance; }
	}

	private GameObject m_loadingSymbol = null;

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

		if(m_loadedInstance != null) {
			GameObject.Destroy(m_loadedInstance);
			m_loadedInstance = null;
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
				m_loadedInstance = GameObject.Instantiate<GameObject>((GameObject)m_loadingRequest.asset, m_container.transform, false);
				if(m_loadedInstance != null) {
					// Apply layer
					m_loadedInstance.SetLayerRecursively(m_container.gameObject.layer);

					// Refresh the scaler
					if(m_scaler != null) {
						m_scaler.Refresh(true, true);
					}
				}

				// Hide loading icon
				ShowLoading(false);

				// Notify subscribers
				OnLoadingComplete.Invoke(this);

				// Loading request done!
				m_loadingRequest = null;
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
		if(m_loadedInstance != null) {
			GameObject.Destroy(m_loadedInstance);
			m_loadedInstance = null;
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
			if(m_loadingSymbol == null) {
				if(m_loadingPrefab != null) {
					m_loadingSymbol = GameObject.Instantiate<GameObject>(m_loadingPrefab, this.transform, false);
				}
			} else {
				m_loadingSymbol.SetActive(true);
			}
		} else {
			if(m_loadingSymbol != null) {
				GameObject.Destroy(m_loadingSymbol);
				m_loadingSymbol = null;
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
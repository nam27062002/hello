﻿// UI3DLoader.cs
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
using System.Collections;

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
	public Transform container {
		get { return m_container; }
		set { m_container = value; } 
	}

	[SerializeField] private UI3DScaler m_scaler = null;
	public UI3DScaler scaler {
		get { return m_scaler; }
		set { m_scaler = value; }
	}

	[Space]
	[SerializeField] private GameObject m_loadingPrefab = null;

	// Internal
	private ResourceRequest m_loadingRequest = null;
	public ResourceRequest loadingRequest {
		get { return m_loadingRequest; }
	}

	[HideInInspector]	// Serialize, cause we want to store instances created in edit mode, but don't expose so users don't think it should be set manually
	[SerializeField] private GameObject m_loadedInstance = null;
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
			LoadAsync();
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
				InstantiatePrefab((GameObject)m_loadingRequest.asset);

				// Loading request done!
				m_loadingRequest = null;
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load the prefab synchronously. The OnLoadComplete event will be triggered
	/// when the loading has finished and the object has been instantiated.
	/// If an instance was already loaded, it will be replaced.
	/// </summary>
	/// <returns>The newly loaded instance. Same as loadedInstance.</returns>
	public GameObject Load() {
		// If we have an async request running, kill it
		if(m_loadingRequest != null) {
			m_loadingRequest = null;
		}

		// Load and instantiate the prefab
		GameObject prefabObj = Resources.Load<GameObject>(m_resourcePath);
		InstantiatePrefab(prefabObj);

		// Return newly instantiated instance
		return m_loadedInstance;
	}

	/// <summary>
	/// Start loading the prefab asynchronously. The OnLoadComplete event will be
	/// triggered when the loading has finished and the object has been instantiated.
	/// If an instance was already loaded, it will be replaced.
	/// </summary>
	/// <returns>The async resources request started. Same as accessing the loadingRequest property.</returns>
	public ResourceRequest LoadAsync() {
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
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create and initialize an instance of the target prefab.
	/// </summary>
	/// <param name="_prefabObj">Prefab object to be instantiated.</param>
	private void InstantiatePrefab(GameObject _prefabObj) {
		// If we have something loaded, destroy it
		if(m_loadedInstance != null) {
			SafeDestroy(m_loadedInstance);
			m_loadedInstance = null;
		}

		// Do it!
		m_loadedInstance = GameObject.Instantiate<GameObject>(_prefabObj, m_container.transform, false);
		if(m_loadedInstance != null) {
			// Apply layer
			Renderer[] renderers = m_loadedInstance.transform.GetComponentsInChildren<Renderer>(false);
			for (int i = 0; i < renderers.Length; ++i) {
				Renderer renderer = renderers[i];
				if (renderer.GetType() == typeof(SkinnedMeshRenderer) || renderer.GetType() == typeof(MeshRenderer)) {
					renderer.gameObject.SetLayer(m_container.gameObject.layer);
				}
			}

			// Remove dangerous scripts
			CollisionEventForwarding cef = m_loadedInstance.FindComponentRecursive<CollisionEventForwarding>();
			if(cef != null) {
				SafeDestroy(cef);
				cef = null;
			}

			// Reset position
			m_loadedInstance.transform.localPosition = Vector3.zero;

			ViewControl vc = m_loadedInstance.GetComponent<ViewControl>();
			if (vc != null) {
				vc.SetMaterialType(ViewControl.MaterialType.NORMAL);
			}
		}

		// The scaler needs refreshing, but do it delayed so the animation has time to initialize
		// Depending on whether the game is running or not, use frames or seconds
		if(Application.isPlaying) {
			StartCoroutine(RefreshScalerDelayed());
		} else {
			Invoke("RefreshScaler", 0.1f);
		}

		// Hide loading icon
		ShowLoading(false);

		// Notify subscribers
		OnLoadingComplete.Invoke(this);
	}

	/// <summary>
	/// Force a refresh on the scaler.
	/// </summary>
	private void RefreshScaler() {
		// Just do it
		if(m_scaler != null) {
			m_scaler.Refresh(true, true);
		}
	}

	/// <summary>
	/// Force a refresh on the scaler after a one frame delay.
	/// </summary>
	/// <returns>The scaler delayed.</returns>
	private IEnumerator RefreshScalerDelayed() {
		// Wait a single frame then refresh the scaler
		yield return new WaitForEndOfFrame();

		// Do it! ^^
		RefreshScaler();
	}

	/// <summary>
	/// Destroy the object using the appropriate method based on whether the application is playing or not.
	/// </summary>
	/// <param name="_obj">Object to be destroyed.</param>
	private void SafeDestroy(Object _obj) {
		if(Application.isPlaying) {
			GameObject.Destroy(_obj);
		} else {
			GameObject.DestroyImmediate(_obj);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}
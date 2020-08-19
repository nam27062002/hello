// HDAddressablesLoader.cs
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
/// Simple script to load 3D prefabs through the addressables system.
/// </summary>
public class HDAddressablesLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
    [System.Serializable]
	public class HDAddressablesLoaderEvent : UnityEvent<HDAddressablesLoader> {}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// DEPRECATED - we keep it to migrate to the new addressables system -> the asset ID will be extracted from this attribute
	[FileListAttribute("", StringUtils.PathFormat.ASSETS_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] protected string m_resourcePath = "";
	[SerializeField] protected int m_useFolderLevelInID = 0;
	[SerializeField] protected string m_assetId = "";

	[SerializeField] protected bool m_loadOnAwake = false;

	[Space]
	[SerializeField] protected Transform m_container = null;
	public Transform container {
		get { return m_container; }
		set { m_container = value; } 
	}

	// Internal
	protected AddressablesOp m_loadingRequest = null;
	public AddressablesOp loadingRequest {
		get { return m_loadingRequest; }
	}

	[HideInInspector]	// Serialize, cause we want to store instances created in edit mode, but don't expose so users don't think it should be set manually
	[SerializeField] protected GameObject m_loadedInstance = null;
	public GameObject loadedInstance {
		get { return m_loadedInstance; }
	}

	// Events
	public HDAddressablesLoaderEvent OnLoadingComplete = new HDAddressablesLoaderEvent();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// If we have something loaded, destroy it
		Unload();

        // If defined, start loading
        if (m_loadOnAwake) {
			LoadAsync();
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	protected virtual void Update() {
		// Update loading request
		if(m_loadingRequest != null) {
			if(m_loadingRequest.isDone) {
				InstantiatePrefab(m_loadingRequest.GetAsset<GameObject>());
				m_loadingRequest = null;
			}
		}
	}

	/// <summary>
	/// A change has been done in the inspector.
	/// </summary>
	protected virtual void OnDestroy() {
        // Delete pending requests
        if(m_loadingRequest != null) {
            m_loadingRequest.Cancel();
            m_loadingRequest = null;
        }

		// Delete instance
		Unload();
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
		m_loadingRequest = null;
        
        // Load and instantiate the prefab
        GameObject prefabObj = HDAddressablesManager.Instance.LoadAsset<GameObject>(m_assetId);
        InstantiatePrefab(prefabObj);

		// Return newly instantiated instance
		return m_loadedInstance;
	}

    /// <summary>
	/// Start loading a prefab asynchronously. The OnLoadComplete event will be
	/// triggered when the loading has finished and the object has been instantiated.
	/// If an instance was already loaded, it will be replaced.
    /// </summary>
    /// <param name="_assetId">The assetId of the addressable item</param>
    public AddressablesOp LoadAsync(string _assetId) {
        m_assetId = _assetId;
        return LoadAsync();
    }

	/// <summary>
	/// Start loading the prefab asynchronously. The OnLoadComplete event will be
	/// triggered when the loading has finished and the object has been instantiated.
	/// If an instance was already loaded, it will be replaced.
	/// </summary>
	/// <returns>The async resources request started. Same as accessing the loadingRequest property.</returns>
	public AddressablesOp LoadAsync() {
		// If we have something loaded, destroy it
		Unload();

        // We don't care if we're already loading another asset, it will be ignored once done loading
        m_loadingRequest = HDAddressablesManager.Instance.LoadAssetAsync(m_assetId);        
		return m_loadingRequest;
	}

    /// <summary>
    /// Unload existing instance. Nothing will happen if there is no instance.
    /// </summary>
    public void Unload() {
		// Just do it :)
		if(m_loadedInstance != null) {
			SafeDestroy(m_loadedInstance);
			m_loadedInstance = null;
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
        if (_prefabObj != null) {
            // If we have something loaded, destroy it
            Unload();

            // Do it!
            m_loadedInstance = Instantiate(_prefabObj, m_container.transform, false);
            if (m_loadedInstance != null) {
				// Make sure it's activated (the prefab might be saved disabled for faster loading)
				m_loadedInstance.SetActive(true);

                // Apply layer
                Renderer[] renderers = m_loadedInstance.transform.GetComponentsInChildren<Renderer>(false);
                for (int i = 0; i < renderers.Length; ++i) {
                    Renderer renderer = renderers[i];
                    if (renderer.GetType() == typeof(SkinnedMeshRenderer) || renderer.GetType() == typeof(MeshRenderer)) {
                        renderer.gameObject.SetLayer(m_container.gameObject.layer);
                    }
                }

                // Remove collision events
                CollisionEventForwarding cef = m_loadedInstance.FindComponentRecursive<CollisionEventForwarding>();
                if (cef != null) {
                    SafeDestroy(cef);
                    cef = null;
                }

                // Reset position
                m_loadedInstance.transform.localPosition = Vector3.zero;

				// Clear any status effects on the materials
                ViewControl vc = m_loadedInstance.GetComponent<ViewControl>();
                if (vc != null) {
                    vc.SetMaterialType(ViewControl.MaterialType.NORMAL);
                }

				// Process particle systems
				ParticleSystem[] ps = m_loadedInstance.GetComponentsInChildren<ParticleSystem>();
				for(int i = 0; i < ps.Length; ++i) {
					// Make sure they belong to the right layer	
					ps[i].gameObject.SetLayer(m_container.gameObject.layer);
				}

				// Let heirs do further processing
				ProcessNewInstance();
            }

            // Notify subscribers
            OnLoadingComplete.Invoke(this);
        }
	}

	/// <summary>
	/// Destroy the object using the appropriate method based on whether the application is playing or not.
	/// </summary>
	/// <param name="_obj">Object to be destroyed.</param>
	protected void SafeDestroy(Object _obj) {
		if(Application.isPlaying) {
            Destroy(_obj);
		} else {
            DestroyImmediate(_obj);
		}
	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATES													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// To be overwritten by heirs to do further processing on a newly loaded instance.
	/// </summary>
	protected virtual void ProcessNewInstance() {
		// To be implemented by heirs if needed
	}

}
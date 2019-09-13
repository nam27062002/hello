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
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to load 3D prefabs into a UI canvas.
/// </summary>
public class UI3DAddressablesLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
    [System.Serializable]
	public class UI3DAddressablesLoaderEvent : UnityEvent<UI3DAddressablesLoader> {}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[FileListAttribute("", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_resourcePath = "";
    public string resourcePath
    {
        get { return m_resourcePath; }
    }
    [SerializeField] private int m_useFolderLevelInID = 0;
    public int useFolderLevelInID { get { return m_useFolderLevelInID; } }
    // this will be stored in the editor
    [SerializeField] private string m_assetId = "";
    public string assetId
    {
        get { return m_assetId; }
    }

    [SerializeField] private bool m_loadOnAwake = false;

	[Space]
	[SerializeField] private Transform m_container = null;
	public Transform container {
		get { return m_container; }
		set { m_container = value; } 
	}

	[Space]
	[SerializeField] private GameObject m_loadingPrefab = null;
    public GameObject loadingPrefab {
        get { return m_loadingPrefab; }
        set { m_loadingPrefab = value; }
    }

    // Internal
    private AddressablesOp m_loadingRequest = null;
	public AddressablesOp loadingRequest {
		get { return m_loadingRequest; }
	}

	[HideInInspector]	// Serialize, cause we want to store instances created in edit mode, but don't expose so users don't think it should be set manually
	[SerializeField] private GameObject m_loadedInstance = null;
	public GameObject loadedInstance {
		get { return m_loadedInstance; }
	}



    private GameObject m_loadingSymbol = null;

	// Events
	public UI3DAddressablesLoaderEvent OnLoadingComplete = new UI3DAddressablesLoaderEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
        Unload();

        // Show loading icon from start
        ShowLoading(true);        

        // If defined, start loading
        if (m_loadOnAwake) {
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
        // Delete pending requests
        if (m_loadingRequest != null)
        {
            m_loadingRequest.Cancel();
            m_loadingRequest = null;
        }

		// Delete instance
		Unload();

		// Destroy loading icon
		ShowLoading(false);
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
    public AddressablesOp LoadAsync(string _assetId)
    {
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
        ShowLoading(true);

		return m_loadingRequest;
	}

    /// <summary>
    /// Update loop.
    /// </summary>
    private void Update()
    {
        if (m_loadingRequest != null)
        {
            if (m_loadingRequest.isDone)
            {
                InstantiatePrefab(m_loadingRequest.GetAsset<GameObject>());
                m_loadingRequest = null;
            }
        }
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
					m_loadingSymbol = Instantiate(m_loadingPrefab, this.transform, false);
				}
			} else {
				m_loadingSymbol.SetActive(true);
			}
		} else {
			if(m_loadingSymbol != null) {
                Destroy(m_loadingSymbol);
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

                // Remove dangerous scripts
                CollisionEventForwarding cef = m_loadedInstance.FindComponentRecursive<CollisionEventForwarding>();
                if (cef != null) {
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

            // Hide loading icon
            ShowLoading(false);

            // Notify subscribers
            OnLoadingComplete.Invoke(this);
        }
	}


	/// <summary>
	/// Destroy the object using the appropriate method based on whether the application is playing or not.
	/// </summary>
	/// <param name="_obj">Object to be destroyed.</param>
	private void SafeDestroy(Object _obj) {
		if(Application.isPlaying) {
            Destroy(_obj);
		} else {
            DestroyImmediate(_obj);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}
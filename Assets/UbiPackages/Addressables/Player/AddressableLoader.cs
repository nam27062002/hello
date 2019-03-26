// AddressableInstantiator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar component to automatically instantiate an addressable asset.
/// </summary>
public class AddressableLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	public class OnInstantiateEvent : UnityEvent<GameObject> { };

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] protected Transform m_parent = null;
	public Transform parent {
		get { return m_parent; }
		set { m_parent = value; }
	}

	[SerializeField] protected string m_assetId = "";	// [AOC] TODO!! Create an attribute to list all addressable IDs?
	public string assetId {
		get { return m_assetId; }
		set { m_assetId = value; }
	}

	[Space]
	[SerializeField] protected bool m_async = true;
	public bool async {
		get { return m_async; }
		set { m_async = value; }
	}

	[SerializeField] private bool m_keepLayers = false;
	public bool keepLayers {
		get { return m_keepLayers; }
		set { m_keepLayers = value; }
	}

	[Space]
	[SerializeField] protected GameObject m_loadingPlaceholderPrefab = null;

	// Events
	public OnInstantiateEvent OnInstantiate = new OnInstantiateEvent();

	// Internal
	protected GameObject m_instance = null;
	public GameObject instance {
		get { return m_instance; }
	}

	protected AddressablesOp m_asyncOperation = null;
	protected AddressableLoadingPlaceholder m_loadingPlaceholderInstance = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	protected virtual void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Initial load!
		Load();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	protected virtual void Update() {
		// Check async operation progress
		if(m_asyncOperation != null) {
			// Is it finished?
			if(m_asyncOperation.isDone) {
				// Loading done! Instantiate!
				InitInstance(m_asyncOperation.GetAsset<GameObject>());
			} else {
				// No! Update progress
				if(m_loadingPlaceholderInstance != null) {
					m_loadingPlaceholderInstance.progress = m_asyncOperation.progress;
				}
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unload instance
		Unload();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start loading.
	/// Will be ignored if an instance already exists, unless forced.
	/// </summary>
	/// <param name="_force">If set to <c>true</c>, unload existing instance (if any) and load again.</param>
	public void Load(bool _force = false) {
		// Unload previous instance?
		if(m_instance != null) {
			if(_force) {
				Unload();
			} else {
				return;	// Already loaded, skip
			}
		}

		// Do the loading. Async?
		if(m_async) {
			// Start the loading operation
			m_asyncOperation = HDAddressablesManager.Instance.LoadAssetAsync(m_assetId);

			// Instantiate placeholder
			if(m_loadingPlaceholderPrefab != null) {
				GameObject placeholderInstance = Instantiate<GameObject>(m_loadingPlaceholderPrefab, m_parent, false);
				m_loadingPlaceholderInstance = placeholderInstance.GetComponent<AddressableLoadingPlaceholder>();
			}
		} else {
			// Just load and instantiate the asset
			GameObject prefab = HDAddressablesManager.Instance.LoadAsset<GameObject>(m_assetId);
			InitInstance(prefab);
		}
	}

	/// <summary>
	/// Destroy existing instance (if any).
	/// </summary>
	public void Unload() {
		// If an async operation is running, cancel it
		// [AOC] For now doesn't seem to be a way to cancel the async operation, so just stop caring about it
		if(m_asyncOperation != null) {
			m_asyncOperation = null;
		}

		// Destroy instance
		if(m_instance != null) {
			Destroy(m_instance);
			m_instance = null;
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create the instance and initialize it.
	/// </summary>
	/// <param name="_prefab">Prefab to create the instance from.</param>
	protected virtual void InitInstance(GameObject _prefab) {
		// Clear async stuff
		ClearAsyncOperation();

		// Make sure prefab is valid
		if(_prefab == null) {
			Debug.LogError(Color.red.Tag("Attempting to instantiated an invalid addressable asset! " + m_assetId));
			return;
		}

		// Create the new instance
		m_instance = Instantiate<GameObject>(_prefab, m_parent, false);

		// Keep layers?
		if(!m_keepLayers) {
			m_instance.SetLayerRecursively(m_parent.gameObject.layer);
		}
	}

	/// <summary>
	/// Clear async stuff,
	/// </summary>
	protected void ClearAsyncOperation() {
		// Clear reference
		m_asyncOperation = null;

		// Hide loading placeholder
		if(m_loadingPlaceholderInstance != null) {
			Destroy(m_loadingPlaceholderInstance.gameObject);
			m_loadingPlaceholderInstance = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
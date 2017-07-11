// PopupManager.cs
// Monster
// 
// Created by Alger Ortín Castellví on 17/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple manager to load and open popups.
/// TODO:
/// - Keep an updated list of open popups
/// - Allow popup queues
/// - Optional delay before opening a popup
/// - Stacked popups (popup over popup)
/// </summary>
public class PopupManager : UbiBCN.SingletonMonoBehaviour<PopupManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Auxiliar class to help managing the async loading of popups
	private class PopupAsyncLoadTask {
		public ResourceRequest request = null;
		public UnityEvent OnLoaded = null;
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Use our own canvas for practicity.
	[SerializeField] private Canvas m_canvas = null;
	public static Canvas canvas {
		get { return instance.m_canvas; }
	}

	// Queues - Expose just for debug purposes
	[SerializeField] private Queue<ResourceRequest> m_loadingQueue = new Queue<ResourceRequest>();
	[SerializeField] private List<PopupController> m_openedPopups = new List<PopupController>();
	[SerializeField] private List<PopupController> m_closedPopups = new List<PopupController>();

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public static int openPopupsCount {
		get { return instance.m_openedPopups.Count; }
	}

	public static int loadingPopupsCount {
		get { return instance.m_loadingQueue.Count; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Required fields
		DebugUtils.Assert(m_canvas != null, "PopupManager requires a canvas to put the popups on!");
	}

	/// <summary>
	/// The manager has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_DESTROYED, OnPopupDestroyed);
	}

	/// <summary>
	/// The manager has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_DESTROYED, OnPopupDestroyed);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Check for loading popups waiting to be opened
		if(m_loadingQueue.Count > 0) {
			ResourceRequest task = m_loadingQueue.Peek();
			if(task.isDone && m_openedPopups.Count == 0) {
				// Instantiate and open popup
				PopupController popup = InstantiatePopup((GameObject)task.asset);
				popup.Open();

				// Dequeue loading task
				m_loadingQueue.Dequeue();
			}
		}
	}

	/// <summary>
	/// Destructor
	/// </summary>
	override protected void OnDestroy() {
		// Call parent
		base.OnDestroy();
	}

	//------------------------------------------------------------------//
	// PRIVATE METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Instantiate a popup from a given prefab.
	/// If there is already an instance of the same prefab in the closed popups list,
	/// it will be reused rather than creating a new one.
	/// </summary>
	/// <returns>The new instance of the popup's game object.</returns>
	/// <param name="_prefab">The prefab of the popup.</param>
	private PopupController InstantiatePopup(GameObject _prefab) {
		// If we already have an instance on the closed popups list, reuse it
		PopupController controller = null;
		for(int i = 0; i < m_closedPopups.Count; i++) {
			// [AOC] TODO!! Find a better way to check if it's actually the same type of popup
			if(m_closedPopups[i].name == _prefab.name) {
				controller = m_closedPopups[i];
				break;
			}
		}

		// Otherwise create a new instance into the canvas
		if(controller == null) {
			// Create a new instance!
			GameObject popupObj = Instantiate(_prefab);
			popupObj.SetActive(true); // Awake is never called if the popup is saved disabled
			popupObj.transform.SetParent(instance.m_canvas.transform, false);
			popupObj.name = _prefab.name;	// To be able to identify it later on
			
			// Open the popup - all popups managed by the manager must have a PopupController
			controller = popupObj.GetComponent<PopupController>();
			DebugUtils.Assert(controller != null, "Couldn't find the PopupController component in the popup " + popupObj.name + ".\nAll popups managed by the manager must have a PopupController.");
		}

		// Make sure the popup appears on top
		controller.transform.SetAsLastSibling();

		// Return the newly created object
		return controller;
	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start loading a popup from the resources folder and put it in the opening queue.
	/// Once loaded and all the previous popups opened, it will be instantiated and opened.
	/// </summary>
	/// <param name="_resourcesPath">The path of the popup in the resources folder.</param>
	public static void OpenPopupAsync(string _resourcesPath) {
		// Start loading it asynchronously from resources
		ResourceRequest task = Resources.LoadAsync<GameObject>(_resourcesPath);
		Debug.Assert(task != null, "The prefab defined to popup " + _resourcesPath + " couldn't be found");	// [AOC] TODO!! Check path

		// Enqueue popup
		instance.m_loadingQueue.Enqueue(task);
	}

	/// <summary>
	/// Load a popup from the resources folder and open it.
	/// </summary>
	/// <returns>The popup that has just been opened.</returns>
	/// <param name="_resourcesPath">The path of the popup in the resources folder.</param>
	public static PopupController OpenPopupInstant(string _resourcesPath) {
		// Load and instantiate the popup
		PopupController popup = LoadPopup(_resourcesPath);

		// Open it and return reference
		popup.Open();
		return popup;
	}

	/// <summary>
	/// Loads a popup from the resources folder, but leaves it closed.
	/// </summary>
	/// <returns>The popup that has been opened.</returns>
	/// <param name="_resourcesPath">The path of the popup in the resources folder.</param>
	public static PopupController LoadPopup(string _resourcesPath) {
		// Load the popup's prefab
		GameObject prefab = Resources.Load<GameObject>(_resourcesPath);
		Debug.Assert(prefab != null, "The prefab defined to popup " + _resourcesPath + " couldn't be found");	// [AOC] TODO!! Check path

		// Instantiate it and return reference
		return instance.InstantiatePopup(prefab);
	}

	/// <summary>
	/// Clear will delete all popups registered with the manager (whether they're open or not).
	/// It will also stop any active loading task.
	/// </summary>
	public static void Clear() {
		// Create delete action
		System.Action<PopupController> deleteAction = (PopupController _popup) => {
			// Destroy directly rather than using PopupController's methods
			GameObject.Destroy(_popup.gameObject);
		};

		// Iterate popups lists
		instance.m_openedPopups.ForEach(deleteAction);
		instance.m_closedPopups.ForEach(deleteAction);

		// Clear lists
		instance.m_openedPopups.Clear();
		instance.m_closedPopups.Clear();

		// Unfortunately, async operations cannot be canceled in Unity, so let's just clear the queue
		instance.m_loadingQueue.Clear();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A popup has been opened.
	/// </summary>
	/// <param name="_popup">The target popup.</param>
	private void OnPopupOpened(PopupController _popup) {
		// Add it to the opened popups list
		m_openedPopups.Add(_popup);

		// Make sure it's not on other lists
		m_closedPopups.Remove(_popup);
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The target popup.</param>
	private void OnPopupClosed(PopupController _popup) {
		// Remove it from the opened popups list
		m_openedPopups.Remove(_popup);

		// Add it to the closed popups list
		m_closedPopups.Add(_popup);
	}

	/// <summary>
	/// A popup has been destroyed.
	/// </summary>
	/// <param name="_popup">The target popup.</param>
	private void OnPopupDestroyed(PopupController _popup) {
		// Remove it from all the lists
		m_openedPopups.Remove(_popup);
		m_closedPopups.Remove(_popup);
	}
    
    public static PopupController PopupMessage_Open(PopupMessage.Config _config)
    {    
        PopupController _popup = OpenPopupInstant("UI/Popups/Message/PF_PopupMessage");
        if (_popup != null)
        {
            PopupMessage _popupMessage = _popup.GetComponent<PopupMessage>();
            _popupMessage.Configure(_config);
        }

        return _popup;       
    }

    public static PopupController PopupLoading_Open()
    {
        return OpenPopupInstant("UI/Popups/Message/PF_PopupLoading");        
    }

    public static PopupController PopupEnableCloud_Open(PopupMessage.Config _config)
    {
        PopupController _popup = OpenPopupInstant("UI/Popups/Message/PF_PopupEnableCloud");
        if (_popup != null)
        {
            PopupMessage _popupMessage = _popup.GetComponent<PopupMessage>();
            _popupMessage.Configure(_config);
        }

        return _popup;
    }
}

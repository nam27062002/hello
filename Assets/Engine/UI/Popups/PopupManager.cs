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
/// - Priority queue
/// - Optional delay before opening a popup
/// </summary>
public class PopupManager : UbiBCN.SingletonMonoBehaviour<PopupManager>, IBroadcastListener {
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
	[SerializeField] private HashSet<PopupController> m_openedPopups = new HashSet<PopupController>();
	[SerializeField] private HashSet<PopupController> m_closedPopups = new HashSet<PopupController>();
	[SerializeField] private List<PopupController> m_queuedPopups = new List<PopupController>();	// Using a list rather than a queue to be able to remove popups from it. Ideally we would use a custom priority queue.

	// Internal
	private GraphicRaycaster m_canvasRaycaster = null;
	private bool m_collectionsBlocked = false;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public static int openPopupsCount {
		get { return instance.m_openedPopups.Count; }
	}

	public static int loadingPopupsCount {
		get { return instance.m_loadingQueue.Count; }
	}

	public static HashSet<PopupController> openedPopups {
		get { return instance.m_openedPopups; }
	}

	public static List<PopupController> queuedPopups {		// First popup in the queue is open
		get { return instance.m_queuedPopups; }
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

		// Cache some references
		m_canvasRaycaster = m_canvas.GetComponent<GraphicRaycaster>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	protected void Start() {
		// Start with canvas camera disabled (for performance)
		RefreshCameraActive();
	}

	public void RefreshCameraActive()
	{
		if(m_openedPopups.Count == 0) {
			m_canvasRaycaster.enabled = false;
			m_canvas.worldCamera.gameObject.SetActive(false);
		}
		else
		{
			m_canvasRaycaster.enabled = true;
			m_canvas.worldCamera.gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// The manager has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.POPUP_DESTROYED, this);
	}

	/// <summary>
	/// The manager has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_DESTROYED, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.POPUP_DESTROYED:
            {
                PopupManagementInfo info = (PopupManagementInfo)broadcastEventInfo;
                OnPopupDestroyed(info.popupController);
            }break;
        }
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
		// Make sure canvas camera is enabled
		m_canvas.worldCamera.gameObject.SetActive(true);
		m_canvasRaycaster.enabled = true;
		//m_canvas.gameObject.SetActive(true);

		// If we already have an instance on the closed popups list, reuse it
		PopupController controller = null;
		foreach(PopupController c in m_closedPopups) {
			// [AOC] TODO!! Find a better way to check if it's actually the same type of popup
			if(c.name == _prefab.name) {
				controller = c;
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
			
			// Get its controller - all popups managed by the manager must have a PopupController
			controller = popupObj.GetComponent<PopupController>();
			DebugUtils.Assert(controller != null, "Couldn't find the PopupController component in the popup " + popupObj.name + ".\nAll popups managed by the manager must have a PopupController.");

			// Be aware when the popup opens/closes to update manager's lists
			controller.OnOpen.AddListener(OnPopupOpened);
			controller.OnClose.AddListener(OnPopupClosed);
		}

		// Make sure the popup appears on top
		controller.transform.SetAsLastSibling();

		// Return the newly created object
		return controller;
	}

	/// <summary>
	/// Update the enqueued popups and open the top one if needed.
	/// </summary>
	private void UpdateQueue() {
		// Nothing to do if queue is empty
		if(m_queuedPopups.Count == 0) return;

		// Don't open any popup from the queue if another popup has been opened manually
		if(m_openedPopups.Count > 0) return;

		// Does the first popup in the queue need to be opened?
		PopupController firstPopup = m_queuedPopups.First();
		if(!firstPopup.gameObject.activeSelf) {
			// Yes! Do it
			firstPopup.gameObject.SetActive(true);
			firstPopup.Open();
		}
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

        if (prefab == null)
        {
            throw new System.Exception("The prefab defined to popup " + _resourcesPath + " couldn't be found");
//            Debug.Assert(prefab != null, "The prefab defined to popup " + _resourcesPath + " couldn't be found");	// [AOC] TODO!! Check path
        }

        // Instantiate it and return reference
        return instance.InstantiatePopup(prefab);
	}

	/// <summary>
	/// Load a popup and put it in the queue.
	/// Will automatically be opened when on top of the queue.
	/// </summary>
	/// <returns>The loaded popup instance.</returns>
	/// <param name="_resourcesPath">The path of the popup in the resources folder.</param>
	public static PopupController EnqueuePopup(string _resourcesPath) {
		// Load the popup
		PopupController popup = LoadPopup(_resourcesPath);

		// Enqueue it
		EnqueuePopup(popup);

		// Return
		return popup;
	}

	/// <summary>
	/// Put a popup instance in the queue, provided it's not already there.
	/// Will automatically be opened when on top of the queue.
	/// If already in the queue and hasn't yet been opened, will be pushed to the end of the queue.
	/// </summary>
	/// <param name="_popup">The popup to be enqueued.</param>
	public static void EnqueuePopup(PopupController _popup) {
		// Just in case
		if(_popup == null) return;

		// Is it on the queue?
		int queueIdx = instance.m_queuedPopups.IndexOf(_popup);
		if(queueIdx == 0) {
			// Yes, but first in the queue: do nothing
			return;
		} else if(queueIdx > 0) {
			// Yes! Move it to the end
			instance.m_queuedPopups.RemoveAt(queueIdx);
			instance.m_queuedPopups.Add(_popup);
		} else {
			// Not in the queue: Add it at the end!
			// Add it to the queue 
			instance.m_queuedPopups.Add(_popup);

			// Disable it until the queue logic decides to open it
			// Unless popup is already open, in which case we will respect it
			if(!_popup.isOpen) {
				_popup.gameObject.SetActive(false);
			}

			// Update the queue logic
			instance.UpdateQueue();
		}
	}

	/// <summary>
	/// Remove a specific popup from the queue.
	/// If opened, it will be closed.
	/// </summary>
	/// <param name="_popup">Popup to be removed.</param>
	/// <param name="_destroy">Destroy the popup after removal?</param>
	public static void RemoveFromQueue(PopupController _popup, bool _destroy) {
		// Is the popup in the queue?
		int queueIdx = instance.m_queuedPopups.IndexOf(_popup);
		if(queueIdx < 0) return;

		// Instantly remove it from the queue
		instance.m_queuedPopups.RemoveAt(queueIdx);

		// Destroy it?
		if(_destroy) {
			Destroy(_popup.gameObject);
		}
	}

	/// <summary>
	/// Find an open popup instance by its path.
	/// </summary>
	/// <returns>The opened popup. <c>null</c> if no popup is opened with the given path.</returns>
	/// <param name="_resourcesPath">Resources path to be checked.</param>
	public static PopupController GetOpenPopup(string _resourcesPath) {
		// Strip prefab name from input path
		string prefabName = System.IO.Path.GetFileNameWithoutExtension(_resourcesPath);

		// Find the popup matching this prefab name among the opened popups
		PopupController popup = null;
		foreach(PopupController c in instance.m_openedPopups) {
			if(c.name == prefabName) {
				popup = c;
				break;
			}
		}
		return popup;
	}

	/// <summary>
	/// Close a target opened popup.
	/// Nothing will happen if the requested popup is not open.
	/// </summary>
	/// <returns>The popup to be closed.</returns>
	/// <param name="_resourcesPath">The path of the popup in the resources folder.</param>
	public static void ClosePopup(string _resourcesPath, bool _destroy = true) {
		// Is the popup opened?
		PopupController popup = GetOpenPopup(_resourcesPath);

		// Yes! Close it
		if(popup != null) {
			popup.Close(_destroy);
		}
	}

	/// <summary>
	/// Clear will delete all popups registered with the manager (whether they're open or not).
	/// It will also stop any active loading task.
	/// </summary>
	/// <param name="_animate">Whether to launch close animation for opened popups before actually destroying them.</param>
	public static void Clear(bool _animate) {
		// Prevent events to modify collections while iterating them
		instance.m_collectionsBlocked = true;

		// Closed popups first
		foreach(PopupController c in instance.m_closedPopups) {
			Destroy(c.gameObject);
		}
		instance.m_closedPopups.Clear();

		// Queued popups: Destroy directly (unless open, which will already be handled by the opened popups list)
		ClearQueue();

		// Opened popups: trigger animation?
		foreach(PopupController c in instance.m_openedPopups) {
			if(_animate) {
				// Trigger close animation
				c.Close(true);
			} else {
				// Destroy directly rather than using PopupController's methods
				Destroy(c.gameObject);
			}
		}
		instance.m_openedPopups.Clear();

		// Loading popups: Unfortunately, async operations cannot be canceled in Unity, so let's just clear the queue
		instance.m_loadingQueue.Clear();

		// Allow back events to modify collections
		instance.m_collectionsBlocked = false;
	}

	/// <summary>
	/// Clear the popup's queue and destroy queued popups.
	/// Currently open popups won't be destroyed.
	/// </summary>
	public static void ClearQueue() {
		// Directly destroy all popups that are not opened
		foreach(PopupController c in instance.m_queuedPopups) {
			if(!c.isOpen) {
				Destroy(c.gameObject);
			}
		}
		instance.m_queuedPopups.Clear();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A popup has been opened.
	/// </summary>
	/// <param name="_popup">The target popup.</param>
	private void OnPopupOpened(PopupController _popup) {
		// Are we allowed to modify collections?
		if(!m_collectionsBlocked) {
			// Add it to the opened popups list
			m_openedPopups.Add(_popup);

			// Make sure it's not on other lists
			m_closedPopups.Remove(_popup);
		}
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The target popup.</param>
	private void OnPopupClosed(PopupController _popup) {
		// Are we allowed to modify collections?
		if(!m_collectionsBlocked) {
			// Remove it from the opened popups list
			m_openedPopups.Remove(_popup);

			// Add it to the closed popups list
			m_closedPopups.Add(_popup);

			// If it's in the queue, remove it from there and try to open the next one in queue
			if(m_queuedPopups.Count > 0) {
				m_queuedPopups.Remove(_popup);
				UpdateQueue();
			}
		}

		// If there are no more open popups, disable canvas camera for performance
		RefreshCameraActive();
	}

	/// <summary>
	/// A popup has been destroyed.
	/// </summary>
	/// <param name="_popup">The target popup.</param>
	private void OnPopupDestroyed(PopupController _popup) {
		// Are we allowed to modify collections?
		if(!m_collectionsBlocked) {
			// Remove it from all the lists
			m_openedPopups.Remove(_popup);
			m_closedPopups.Remove(_popup);

			// If it's in the queue, remove it from there and try to open the next one in queue
			if(m_queuedPopups.Count > 0) {
				m_queuedPopups.Remove(_popup);
				UpdateQueue();
			}
		}

		// If there are no more open popups, disable canvas camera for performance
		RefreshCameraActive();
	}
    
    public static PopupController PopupMessage_Open(IPopupMessage.Config _config)
    {    
		// Load different prefabs depending on text type
		string path = PopupMessage.PATH;
		switch(_config.TextType) {
			case IPopupMessage.Config.ETextType.DEFAULT:	path = PopupMessage.PATH;		break;
			case IPopupMessage.Config.ETextType.SYSTEM:		path = PopupMessageSystem.PATH;	break;
		}

        PopupController _popup = OpenPopupInstant(path);
        if (_popup != null)
        {
            IPopupMessage _popupMessage = _popup.GetComponent<IPopupMessage>();
            _popupMessage.Configure(_config);
        }

        return _popup;       
    }

    public static PopupController PopupLoading_Open()
    {
        return OpenPopupInstant("UI/Popups/Message/PF_PopupLoading");        
    }

    public static PopupController PopupEnableCloud_Open(IPopupMessage.Config _config)
    {
        PopupController _popup = OpenPopupInstant("UI/Popups/Message/PF_PopupEnableCloud");
        if (_popup != null)
        {
            IPopupMessage _popupMessage = _popup.GetComponent<IPopupMessage>();
            _popupMessage.Configure(_config);
        }

        return _popup;
    }

    /// <summary>
	/// Check whether the given popup was the last opened popup.
	/// </summary>
    public static bool IsLastOpenPopup(PopupController _popup) {
		// Find out the last opened popup
		PopupController lastPopup = null;
		foreach(PopupController c in instance.m_openedPopups) {
			if(lastPopup == null) {
				lastPopup = c;
			} else if(lastPopup.openTimestamp < c.openTimestamp) {
				lastPopup = c;
			}
		}

		// Is it the target popup?
		return lastPopup == _popup;
    }
}

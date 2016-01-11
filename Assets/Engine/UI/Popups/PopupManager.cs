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
public class PopupManager : SingletonMonoBehaviour<PopupManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Use our own canvas for practicity.
	[SerializeField] private Canvas m_canvas = null;

	// Queues
	private Queue<ResourceRequest> m_loadingQueue = new Queue<ResourceRequest>();
	private List<PopupController> m_openedPopups = new List<PopupController>();

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
	}

	/// <summary>
	/// The manager has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
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
				InstantiateAndOpenPopup((GameObject)task.asset);

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
	/// Instantiate and open a popup from a given prefab.
	/// </summary>
	/// <returns>The new instance of the popup's game object.</returns>
	/// <param name="_prefab">The prefab of the popup.</param>
	private PopupController InstantiateAndOpenPopup(GameObject _prefab) {
		// Instantiate it to the canvas
		GameObject popupObj = Instantiate(_prefab);
		popupObj.transform.SetParent(instance.m_canvas.transform, false);
		
		// Open the popup - all popups managed by the manager must have a PopupController
		PopupController controller = popupObj.GetComponent<PopupController>();
		DebugUtils.Assert(controller != null, "Couldn't find the PopupController component in the popup " + popupObj.name + ".\nAll popups managed by the manager must have a PopupController.");
		controller.Open();
		
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
		DebugUtils.SoftAssert(task != null, "The prefab defined to popup " + _resourcesPath + " couldn't be found");	// [AOC] TODO!! Check path

		// Enqueue popup
		instance.m_loadingQueue.Enqueue(task);
	}

	/// <summary>
	/// Load a popup from the resources folder and open it.
	/// </summary>
	/// <returns>The popup that has just been opened.</returns>
	/// <param name="_resourcesPath">The path of the popup in the resources folder.</param>
	public static PopupController OpenPopupInstant(string _resourcesPath) {
		// Start loading it asynchronously from resources
		GameObject prefab = Resources.Load<GameObject>(_resourcesPath);
		DebugUtils.SoftAssert(prefab != null, "The prefab defined to popup " + _resourcesPath + " couldn't be found");	// [AOC] TODO!! Check path
		
		// Open popup
		return instance.InstantiateAndOpenPopup(prefab);
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
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The target popup.</param>
	private void OnPopupClosed(PopupController _popup) {
		// Remove it from the opened popups list
		m_openedPopups.Remove(_popup);
	}
}

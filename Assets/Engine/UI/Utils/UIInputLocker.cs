// UIInputLocker.cs
// 
// Created by Alger Ortín Castellví on 03/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to be attached to a UI Canvas used to lock/unlock the input 
/// on that canvas (and all the canvas behind it).
/// Use EngineEvents.UI_LOCK_INPUT(bool) to lock/unlock the input.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class UIInputLocker : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// External references
	private Canvas m_canvas = null;
	private EmptyGraphic m_locker = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_canvas = GetComponent<Canvas>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<bool>(EngineEvents.UI_LOCK_INPUT, OnLockInputEvent);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(EngineEvents.UI_LOCK_INPUT, OnLockInputEvent);

		// Disable locker object as well (if any)
		if(m_locker != null) {
			m_locker.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Destroy locker object as well
		if(m_locker != null) {
			GameObject.Destroy(m_locker);
			m_locker = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A request to lock/unlock the UI has been sent.
	/// </summary>
	/// <param name="_lock">Whether to lock or unlock this canvas.</param>
	private void OnLockInputEvent(bool _lock) {
		// If the locker object was not created, do it now
		if(m_locker == null) {
			// Create new game object
			GameObject lockerObj = new GameObject("UILocker");

			// Add full-canvas rect transform object
			RectTransform lockerTransform = lockerObj.AddComponent<RectTransform>();
			lockerTransform.SetParent(m_canvas.transform);
			lockerTransform.localScale = Vector3.one;
			lockerTransform.anchorMin = Vector2.zero;
			lockerTransform.anchorMax = Vector2.one;
			lockerTransform.pivot = Vector2.one * 0.5f;
			lockerTransform.anchoredPosition3D = Vector3.zero;
			lockerTransform.offsetMin = Vector2.zero;
			lockerTransform.offsetMax = Vector2.zero;

			// Add empty graphic to block raycast
			m_locker = lockerObj.AddComponent<EmptyGraphic>();
			m_locker.color = Colors.WithAlpha(Color.cyan, 0.5f);
		}

		// Make sure the locker object is on top of everything else in the canvas
		m_locker.transform.SetAsLastSibling();

		// Toggle locker object
		m_locker.gameObject.SetActive(_lock);
	}
}
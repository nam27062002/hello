// MouseEventsHandler.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Small improvement over Unity's default OnMouse events.
/// Keep in mind that these events are never captured, so they will never be blocked by the UI or else.
/// In order for an object to receive such events, a collider or graphic must be present in the hierarchy,
/// as well as an EventSystem somewhere in the scene and a PhysicsRaycast attached to a camera or
/// a GraphicsRaycast attached to a UI Canvas.
/// </summary>
public class MouseEventsHandler : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private float m_dragThreshold = 50f;
	public float dragThreshold {
		get { return m_dragThreshold; }
		set { m_dragThreshold = value; }
	}

	[SerializeField] private int m_frameCountThreshold = 20;
	public int frameCountThreshold {
		get { return m_frameCountThreshold; }
		set { m_frameCountThreshold = value; }
	}

	// Events
	public UnityEvent OnMouseClick = new UnityEvent();

	// Internal
	private bool m_clickDetection = false;
	private Vector3 m_mouseDownPos = Vector3.zero;
	private int m_downFramesCount = 0;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Mouse down.
	/// </summary>
	public void OnMouseDown() {
		// Ignore if not enabled
		if(!isActiveAndEnabled) return;

		// Start click detection
		m_clickDetection = true;
		m_mouseDownPos = Input.mousePosition;
		m_downFramesCount = 0;
	}

	/// <summary>
	/// Mouse drag.
	/// </summary>
	public void OnMouseDrag() {
		// Ignore if not enabled
		if(!isActiveAndEnabled) return;

		// Do nothing if click has already been discarded
		if(m_clickDetection) {
			// Increase frame counter
			m_downFramesCount++;

			// If frame count threshold or drag distance threshold are overflown, stop detecting click
			if((Input.mousePosition - m_mouseDownPos).sqrMagnitude > m_dragThreshold || m_downFramesCount > m_frameCountThreshold) {
				m_clickDetection = false;
			}
		}
	}

	/// <summary>
	/// Mouse up.
	/// </summary>
	public void OnMouseUp() {
		// Ignore if not enabled
		if(!isActiveAndEnabled) return;

		// Click detected?
		if(m_clickDetection) {
			// Notify listeners
			OnMouseClick.Invoke();

			// Reset flag
			m_clickDetection = false;
		}
	}
}
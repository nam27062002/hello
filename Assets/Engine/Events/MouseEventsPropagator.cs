// MouseEventsPropagator.cs
// 
// Created by Alger Ortín Castellví on 07/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Workaround class to be able to subscribe to mouse events from outside the 
/// target object.
/// Events are created on demand to avoid using unnecessary memory.
/// Events are autocleared upon object destruction.
/// Usage:
/// <c>
/// MouseEventsPropagator mouseEvents = GetComponentInChildren<MouseEventsPopagator>();
/// mouseEvents.onMouseUp.AddListener(MyMouseUpListener);
/// </c>
/// See http://docs.unity3d.com/ScriptReference/MonoBehaviour.OnMouseDown.html
/// </summary>
public class MouseEventsPropagator : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum EventType {
		MOUSE_DOWN,			// Called when the user has pressed the mouse button while over the GUIElement or Collider.
		MOUSE_DRAG,			// Called when the user has clicked on a GUIElement or Collider and is still holding down the mouse.
		MOUSE_ENTER,		// Called when the mouse enters the GUIElement or Collider.
		MOUSE_EXIT,			// Called when the mouse is not any longer over the GUIElement or Collider.
		MOUSE_OVER,			// Called every frame while the mouse is over the GUIElement or Collider.
		MOUSE_UP,			// Called when the user has released the mouse button.
		MOUSE_UP_AS_BUTTON,	// Only called when the mouse is released over the same GUIElement or Collider as it was pressed.
		COUNT
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal
	private UnityEvent[] m_events = new UnityEvent[(int)EventType.COUNT];

	// Public accessors
	/// <summary>
	/// Called when the user has pressed the mouse button while over the GUIElement 
	/// or Collider.
	/// </summary>
	public UnityEvent onMouseDown { get { return GetEvent(EventType.MOUSE_DOWN); }}

	/// <summary>
	/// Called when the user has clicked on a GUIElement or Collider and is still 
	/// holding down the mouse.
	/// </summary>
	public UnityEvent onMouseDrag { get { return GetEvent(EventType.MOUSE_DRAG); }}

	/// <summary>
	/// Called when the mouse enters the GUIElement or Collider.
	/// </summary>
	public UnityEvent onMouseEnter { get { return GetEvent(EventType.MOUSE_ENTER); }}

	/// <summary>
	/// Called when the mouse is not any longer over the GUIElement or Collider.
	/// </summary>
	public UnityEvent onMouseExit { get { return GetEvent(EventType.MOUSE_EXIT); }}

	/// <summary>
	/// Called every frame while the mouse is over the GUIElement or Collider.
	/// </summary>
	public UnityEvent onMouseOver { get { return GetEvent(EventType.MOUSE_OVER); }}

	/// <summary>
	/// Called when the user has released the mouse button.
	/// </summary>
	public UnityEvent onMouseUp { get { return GetEvent(EventType.MOUSE_UP); }}

	/// <summary>
	/// Only called when the mouse is released over the same GUIElement or Collider 
	/// as it was pressed.
	/// </summary>
	public UnityEvent onMouseUpAsButton { get { return GetEvent(EventType.MOUSE_UP_AS_BUTTON); }}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Clear all events
		for(int i = 0; i < m_events.Length; i++) {
			if(m_events[i] != null) {
				m_events[i].RemoveAllListeners();
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets an event from the array, creating it if necessary.
	/// </summary>
	/// <returns>The event of the given type.</returns>
	/// <param name="_type">The type of event to be returned.</param>
	private UnityEvent GetEvent(EventType _type) {
		int idx = (int)_type;
		if(m_events[idx] == null) {
			m_events[idx] = new UnityEvent();
		}
		return m_events[idx];
	}

	/// <summary>
	/// Invoke the event of the given type, if initialized.
	/// </summary>
	/// <param name="_type">Type of event to be invoked.</param>
	private void Invoke(EventType _type) {
		int idx = (int)_type;
		if(m_events[idx] != null) {
			m_events[idx].Invoke();
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called when the user has pressed the mouse button while over the GUIElement 
	/// or Collider.
	/// </summary>
	private void OnMouseDown() {
		Invoke(EventType.MOUSE_DOWN);
	}

	/// <summary>
	/// Called when the user has clicked on a GUIElement or Collider and is still 
	/// holding down the mouse.
	/// </summary>
	private void OnMouseDrag() {
		Invoke(EventType.MOUSE_DRAG);
	}

	/// <summary>
	/// Called when the mouse enters the GUIElement or Collider.
	/// </summary>
	private void OnMouseEnter() {
		Invoke(EventType.MOUSE_ENTER);
	}

	/// <summary>
	/// Called when the mouse is not any longer over the GUIElement or Collider.
	/// </summary>
	private void OnMouseExit() {
		Invoke(EventType.MOUSE_EXIT);
	}

	/// <summary>
	/// Called every frame while the mouse is over the GUIElement or Collider.
	/// </summary>
	private void OnMouseOver() {
		Invoke(EventType.MOUSE_OVER);
	}

	/// <summary>
	/// Called when the user has released the mouse button.
	/// </summary>
	private void OnMouseUp() {
		Invoke(EventType.MOUSE_UP);
	}

	/// <summary>
	/// Only called when the mouse is released over the same GUIElement or Collider 
	/// as it was pressed.
	/// </summary>
	private void OnMouseUpAsButton() {
		Invoke(EventType.MOUSE_UP_AS_BUTTON);
	}
}
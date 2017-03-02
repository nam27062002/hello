// DragControl.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Generic class to control an object transformation via 2D drag gestures.
/// No transformation is actually performed, subscribe to events to apply custom transformations.
/// TODO:
/// - Elastic bounce on limits
/// - Bounce on limits
/// </summary>
public class DragControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable]
	public class DragControlEvent : UnityEvent<DragControl> {}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_sensitivity = 1f;
	public float sensitivity {
		get { return m_sensitivity; }
		set { m_sensitivity = value; }
	}

	[SerializeField] private bool m_inertia = true;
	public bool inertia {
		get { return m_inertia; }
		set { m_inertia = value; }
	}

	[Tooltip("Only used if inertia is active")]
	[SerializeField] private float m_inertiaDecelerationRate = 0.135f;
	public float inertiaDecelerationRate {
		get { return m_inertiaDecelerationRate; }
		set { m_inertiaDecelerationRate = value; }
	}

	[Space]
	[SerializeField] private bool m_clampX = false;
	public bool clampX {
		get { return m_clampX; }
		set {
			m_clampX = value;
			ApplyOffset(Vector2.zero);	// Refresh value
		}
	}

	[SerializeField] private Range m_xRange = new Range(-100f, 100f);
	public Range xRange {
		get { return m_xRange; }
		set {
			m_xRange = value;
			ApplyOffset(Vector2.zero);
		}
	}

	[SerializeField] private bool m_clampY = false;
	public bool clampY {
		get { return m_clampY; }
		set {
			m_clampY = value;
			ApplyOffset(Vector2.zero);	// Refresh value
		}
	}

	[SerializeField] private Range m_yRange = new Range(-100f, 100f);
	public Range yRange {
		get { return m_yRange; }
		set {
			m_yRange = value;
			ApplyOffset(Vector2.zero);
		}
	}

	// Events
	[Space]
	public DragControlEvent OnValueChanged = new DragControlEvent();

	// Public Logic
	private Vector2 m_velocity = Vector2.zero;
	public Vector2 velocity { 
		get { return m_velocity; } 
		set { m_velocity = value; } 
	}

	private bool m_dragging = false;
	public bool dragging {
		get { return m_dragging; }
	}

	private Vector2 m_value = Vector2.zero;
	public Vector2 value {
		get { return m_value; }
		set { m_value = value; }
	}

	private Vector2 m_previousValue = Vector2.zero;
	public Vector2 previousValue {
		get { return m_previousValue; }
	}

	public Vector2 offset {
		get { return m_value - m_previousValue; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

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
		// Stop any active movement
		m_velocity = Vector2.zero;
		m_dragging = false;
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame, at the end of the frame.
	/// </summary>
	private void LateUpdate() {
		// Aux vars
		float deltaTime = Time.unscaledDeltaTime;

		// If inertia is enabled, update velocity and apply inertia
		// Except while dragging
		if(!m_dragging && m_velocity != Vector2.zero) {
			// Both axis
			Vector2 offset = Vector2.zero;
			for(int axis = 0; axis < 2; axis++) {
				// Only if inertia is enabled
				if(m_inertia) {
					// Update velocity and stop when too small
					m_velocity[axis] *= Mathf.Pow(m_inertiaDecelerationRate, deltaTime);
					if(Mathf.Abs(m_velocity[axis]) < 1f) {
						m_velocity[axis] = 0f;
					}

					// Apply velocity
					offset[axis] = m_velocity[axis] * deltaTime;
				} else {
					m_velocity[axis] = 0f;
				}
			}

			// Apply offset
			ApplyOffset(offset);
		}

		// While dragging, compute new velocity if inertia is enabled
		if(m_dragging && m_inertia) {
			// Compute based on offset applied by the dragging handler and interpolate with previous velocity for a smoother experience
			Vector2 newVelocity = (m_value - m_previousValue)/deltaTime;
			m_velocity = Vector3.Lerp(m_velocity, newVelocity, deltaTime * 10f);
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	/// <summary>
	/// A value has changed in the editor.
	/// </summary>
	private void OnValidate() {
		// Cap some values
		m_sensitivity = Mathf.Max(0f, m_sensitivity);
		m_inertiaDecelerationRate = Mathf.Max(0f, m_inertiaDecelerationRate);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update current value and previous value by applying the given offset.
	/// Sensitivity will be applied to given offset.
	/// </summary>
	/// <param name="_offset">Unscaled offset to be applied.</param>
	private void ApplyOffset(Vector2 _offset) {
		// Do it
		m_previousValue = m_value;
		m_value += _offset * m_sensitivity;

		// Apply clamping, if any
		if(m_clampX) m_value.x = m_xRange.Clamp(m_value.x);
		if(m_clampY) m_value.y = m_yRange.Clamp(m_value.y);

		// Notify listeners
		if(m_value != m_previousValue) {
			OnValueChanged.Invoke(this);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The input has started dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnBeginDrag(PointerEventData _event) {
		// Stop movement
		m_velocity = Vector2.zero;

		// Reset flag
		m_dragging = true;
	}

	/// <summary>
	/// The input is dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event.</param>
	public void OnDrag(PointerEventData _event) {
		// Directly apply offset
		ApplyOffset(_event.delta);
	}

	/// <summary>
	/// The input has stopped dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnEndDrag(PointerEventData _event) {
		// Reset flag
		m_dragging = false;
	}
}
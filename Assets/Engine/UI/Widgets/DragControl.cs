// DragControl.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
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
/// </summary>
[RequireComponent(typeof(Graphic))]
public class DragControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	protected const float IOS_SENSITIVITY_CORRECTION = 0.5f;
	protected const float ANDROID_SENSITIVITY_CORRECTION = 0.5f;

	[Serializable]
	public class DragControlEvent : UnityEvent<DragControl> {}

	public enum ClampMode {
		CLAMP,
		BOUNCE,
		LOOP
	};

	[Serializable]
	public class ClampSetup {
		public bool clamp = false;
		public Range range = new Range(-100f, 100f);
		public ClampMode mode = ClampMode.CLAMP;
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] protected bool[] m_axisEnabled = new bool[] {true, true};

	public bool horizontal {
		get { return m_axisEnabled[0]; }
		set { m_axisEnabled[0] = value; }
	}

	public bool vertical {
		get { return m_axisEnabled[1]; }
		set { m_axisEnabled[1] = value; }
	}

	[Space]
	[SerializeField] protected float m_sensitivity = 1f;
	public float sensitivity {
		get { return m_sensitivity; }
		set { m_sensitivity = value; UpdateSensitivityCorrection(); }
	}

	[SerializeField] protected Vector2 m_idleVelocity = Vector2.zero;
	public Vector2 idleVelocity {
		get { return m_idleVelocity; }
		set { m_idleVelocity = value; }
	}

	[SerializeField] protected bool m_inertia = true;
	public bool inertia {
		get { return m_inertia; }
		set { m_inertia = value; }
	}

	[Range(0f, 0.99f)] [SerializeField] protected float m_inertiaAcceleration = 0.135f;	// Values bigger than 1 would accelerate the value infinitely
	public float inertiaAcceleration {
		get { return m_inertiaAcceleration; }
		set { m_inertiaAcceleration = value; }
	}

	// Clamping
	[SerializeField]
	protected ClampSetup[] m_clampSetup = new ClampSetup[2];	// One per axis

	public ClampSetup clampSetupX {
		get { return m_clampSetup[0]; }
		set { m_clampSetup[0] = value; CheckValue(); }
	}

	public ClampSetup clampSetupY {
		get { return m_clampSetup[1]; }
		set { m_clampSetup[1] = value; CheckValue(); }
	}

	// Events
	[Space]
	public DragControlEvent OnValueChanged = new DragControlEvent();

	// Public Logic
	protected Vector2 m_velocity = Vector2.zero;
	public Vector2 velocity { 
		get { return m_velocity; } 
		set { m_velocity = value; } 
	}

	protected bool m_dragging = false;
	public bool dragging {
		get { return m_dragging; }
	}

	protected Vector2 m_value = Vector2.zero;
	public Vector2 value {
		get { return m_value; }
		set { m_value = value; CheckValue(); }
	}

	protected Vector2 m_previousValue = Vector2.zero;
	public Vector2 previousValue {
		get { return m_previousValue; }
	}

	public Vector2 offset {
		get { return m_value - m_previousValue; }
	}

	// Internal
	protected float m_correctedSensitivity = 1f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	virtual protected void Awake() {
		// Initialize corrected sensitivity
		UpdateSensitivityCorrection();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	virtual protected void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	virtual protected void OnEnable() {
		// Stop any active movement
		m_velocity = Vector2.zero;
		m_dragging = false;
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	virtual protected void OnDisable() {

	}

	/// <summary>
	/// Called every frame, at the end of the frame.
	/// </summary>
	virtual protected void LateUpdate() {
		// Aux vars
		float deltaTime = Time.unscaledDeltaTime;

		// While dragging, don't update value (it's done via the drag events)
		if(m_dragging) {
			// If inertia is enabled, update velocity for when we stop dragging
			if(m_inertia) {
				// Compute based on offset applied by the dragging handler and interpolate with previous velocity for a smoother experience
				Vector2 newVelocity = (m_value - m_previousValue)/deltaTime;
				m_velocity = Vector3.Lerp(m_velocity, newVelocity, deltaTime * 10f);
			}
		}

		// If we're not dragging, apply velocity
		else {
			// Both axis
			Vector2 offset = Vector2.zero;
			for(int axis = 0; axis < 2; axis++) {
				// Skip if axis not enabled
				if(!m_axisEnabled[axis]) continue;

				// Update velocity while moving
				if(m_velocity != Vector2.zero) {
					// If inertia is enabled, update velocity
					if(m_inertia) {
						// Stop when too small
						m_velocity[axis] *= Mathf.Pow(m_inertiaAcceleration, deltaTime);
						if(Mathf.Abs(m_velocity[axis]) < 1f) {
							m_velocity[axis] = 0f;
						}
					} else {
						// Inertia disabled, don't move
						m_velocity[axis] = 0f;
					}
				}

				// Apply velocity, including idle velocity
				offset[axis] = (m_velocity[axis] + m_idleVelocity[axis]) * deltaTime;
			}

			// Apply offset
			ApplyOffset(offset);
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	virtual protected void OnDestroy() {

	}

	/// <summary>
	/// A value has changed in the editor.
	/// </summary>
	virtual protected void OnValidate() {
		// Cap some values
		m_axisEnabled.Resize(2);
		m_sensitivity = Mathf.Max(0f, m_sensitivity);
		m_inertiaAcceleration = Mathf.Max(0f, m_inertiaAcceleration);
		m_clampSetup.Resize(2);	// Fixed size!

		// For when sensitivity is changed during play mode
		UpdateSensitivityCorrection();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update current value and previous value by applying the given offset.
	/// Sensitivity will be applied to given offset.
	/// </summary>
	/// <param name="_offset">Unscaled offset to be applied.</param>
	virtual protected void ApplyOffset(Vector2 _offset) {
		// Not for disabled axis!
		for(int i = 0; i < 2; i++) {
			if(!m_axisEnabled[i]) _offset[i] = 0f;
		}

		// Do it
		m_previousValue = m_value;
		m_value += _offset * m_sensitivity;

		// Apply clamping, if any
		for(int i = 0; i < 2; i++) {
			// Skip if axis disabled
			if(!m_axisEnabled[i]) continue;

			// Clamp mode?
			if(m_clampSetup[i].clamp) {
				switch(m_clampSetup[i].mode) {
					case ClampMode.CLAMP: {
						// Stick to edges and clear velocity
						if(!m_clampSetup[i].range.Contains(m_value[i])) {
							m_value[i] = m_clampSetup[i].range.Clamp(m_value[i]);
							m_velocity[i] = 0f;
						}
					} break;

					case ClampMode.BOUNCE: {
						// If we're outside bounds, clamp and reverse velocity
						if(!m_clampSetup[i].range.Contains(m_value[i])) {
							m_value[i] = m_clampSetup[i].range.Clamp(m_value[i]);
							m_velocity[i] *= -1f;
						}
					} break;

					case ClampMode.LOOP: {
						// If we're outside bounds, clamp to opposite limit
						if(m_value[i] >= m_clampSetup[i].range.max) {
							m_value[i] = m_clampSetup[i].range.min;
						} else if(m_value[i] <= m_clampSetup[i].range.min) {
							m_value[i] = m_clampSetup[i].range.max;
						}
					} break;
				}
			}
		}

		// Notify listeners
		if(m_value != m_previousValue) {
			ApplyValue();	// Internal notification
			OnValueChanged.Invoke(this);	// External
		}
	}

	/// <summary>
	/// Check current value to make sure it fits clamping setup.
	/// </summary>
	virtual protected void CheckValue() {
		// Actually is the same as just applying a zero offset
		ApplyOffset(Vector2.zero);
	}

	/// <summary>
	/// Updates the sensitivity correction based on platform.
	/// Should be called every time sensitivity is changed.
	/// </summary>
	private void UpdateSensitivityCorrection() {
		// Compute corrected sensitivity
		switch(Application.platform) {
			case RuntimePlatform.IPhonePlayer:	m_correctedSensitivity = m_sensitivity * IOS_SENSITIVITY_CORRECTION;		break;
			case RuntimePlatform.Android:		m_correctedSensitivity = m_sensitivity * ANDROID_SENSITIVITY_CORRECTION;	break;
			default:							m_correctedSensitivity = m_sensitivity;										break;
		}
	}

	//------------------------------------------------------------------------//
	// VIRTUAL METHODS														  //
	// To be implemented by heirs if needed.								  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do whatever needed to apply the value.
	/// </summary>
	virtual protected void ApplyValue() {
		// To be implemented by heirs.
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
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
using DG.Tweening;

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
	public const float IOS_SENSITIVITY_CORRECTION = 0.5f;
	public const float ANDROID_SENSITIVITY_CORRECTION = 0.5f;

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
	[SerializeField] protected Transform m_target = null;
	public Transform target {
		get { return m_target; }
		set { InitFromTarget(value, false); }
	}

	[Space]
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

	[Range(0f, 0.99f)] [SerializeField] protected float m_inertiaAcceleration = 0.01f;	// Values bigger than 1 would accelerate the value infinitely
	public float inertiaAcceleration {
		get { return m_inertiaAcceleration; }
		set { m_inertiaAcceleration = value; }
	}

	// Clamping
	[SerializeField]
	protected ClampSetup[] m_clampSetup = new ClampSetup[2];	// One per axis

	public ClampSetup clampSetupX {
		get { return m_clampSetup[0]; }
		set { m_clampSetup[0] = value; SetValue(m_value); }
	}

	public ClampSetup clampSetupY {
		get { return m_clampSetup[1]; }
		set { m_clampSetup[1] = value; SetValue(m_value); }
	}

	// Auto-restore value
	[Space]
	[SerializeField] protected bool m_forceInitialValue = false;
	public bool forceInitialValue {
		get { return m_forceInitialValue; }
		set { m_forceInitialValue = value; }
	}

	[SerializeField] protected Vector2 m_initialValue = Vector2.zero;
	public Vector2 initialValue {
		get { return m_initialValue; }
		set { m_initialValue = value; }
	}

	[SerializeField] protected bool m_restoreOnDisable = true;
	public bool restoreOnDisable {
		get { return m_restoreOnDisable; }
		set { m_restoreOnDisable = value; }
	}
	
	[SerializeField] protected float m_restoreDuration = 0.25f;
	public float restoreDuration {
		get { return m_restoreDuration; }
		set { m_restoreDuration = value; }
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
		set { SetValue(value); }
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
	private Vector2 m_originalValue = Vector2.zero;
	private Tweener m_tween = null;

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
		m_previousValue = Vector2.zero;
		m_value = Vector2.zero;
		m_velocity = Vector2.zero;
		m_dragging = false;

		// Initial setup
		InitFromTarget(target, true);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	virtual protected void OnDisable() {
		// Restore original value?
		if(m_restoreOnDisable) {
			RestoreOriginalValue();
		}
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
		// Except if animating
		else if(m_tween == null) {
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
		SetValue(m_value + _offset);
	}

	/// <summary>
	/// Set a new value. Value will be clamped and treated and, if different than 
	/// previous value, it will be applied too.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	/// <param name="_force">Force applying the new value regardless of whether it's different from previous value.</param>
	virtual protected void SetValue(Vector2 _newValue, bool _force = false) {
		// Save previous and new values
		m_previousValue = m_value;
		m_value = _newValue;

		// Apply clamping, if any
		ApplyClamp();

		// Notify listeners
		if(m_value != m_previousValue || _force) {
			ApplyValue();	// Internal notification
			OnValueChanged.Invoke(this);	// External
		}
	}

	/// <summary>
	/// Apply clamping to the current value.
	/// </summary>
	virtual protected void ApplyClamp() {
		// Just do it
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
	// ORIGINAL VALUE MANAGEMENT											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Intitialize the drag controller with the given target.
	/// </summary>
	/// <param name="_target">New target.</param>
	/// <param name="_force">Do all the initialization even if the target is the same as the current one.</param>
	public void InitFromTarget(Transform _target, bool _force) {
		// If we have a valid target already, reset it to its original position (if the flag says so)
		// Only if we're active!
		if(isActiveAndEnabled && m_restoreOnDisable) {
			RestoreOriginalValue();
		}

		// Store target
		Transform oldTarget = m_target;
		m_target = _target;

		// Nothing else to do if new target is not valid
		if(m_target == null) return;

		// Extra stuff only if the target is different than the current one (or forced)
		// Otherwise we would be overriding original values
		if(m_target != oldTarget || _force) {
			// Initialize drag control with initial value
			// Force initial value or use current target value?
			m_value = GetValueFromTarget();
			m_originalValue = m_value;
			if(m_forceInitialValue) {
				TweenTo(m_initialValue, 0.25f);
			} else {
				SetValue(m_value, true);	// Make sure value is within bounds
			}
		}
	}

	/// <summary>
	/// Restore original value of the target.
	/// Doesn't check the flag nor the state of the component.
	/// </summary>
	/// <param name="_animate">Whether to animate or not.</param>
	public void RestoreOriginalValue(bool _animate = true) {
		// Target must be valid
		if(m_target == null) return;

		// Animated?
		if(m_restoreDuration > 0f && _animate) {
			TweenTo(m_originalValue, m_restoreDuration);
		} else {
			SetValue(m_originalValue, true);
		}
	}

	/// <summary>
	/// Tween to a specific value.
	/// </summary>
	/// <param name="_value">Target value.</param>
	/// <param name="_duration">Duration of the tween.</param>
	public void TweenTo(Vector2 _value, float _duration) {
		// Kill any existing tween
		if(m_tween != null) {
			m_tween.Kill();
			m_tween = null;
		}

		// Launch new tween
		m_tween = DOTween.To(
			() => { return value; }, 			// Getter
			(_v) => { SetValue(_v, true); },	// Setter
			_value, 
			_duration
		)
		.SetUpdate(true)
		.SetEase(Ease.InOutQuad)
		.OnComplete(() => { m_tween = null; });
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

	/// <summary>
	/// Get the current value from target.
	/// </summary>
	/// <returns>The current value from target.</returns>
	virtual protected Vector2 GetValueFromTarget() {
		// To be implemented by heirs.
		return Vector2.zero;
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
		// Apply offset with sensitivity corrected based on platform
		ApplyOffset(_event.delta * m_correctedSensitivity);
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
// DragControlZoom.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Connector of a drag control with a zoom for a camera using the pinch gesture.
/// </summary>
public class DragControlZoom : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Mode {
		FOV,				// Only for presp cameras
		Z_MOVE,				// Only for presp cameras
		ORTOGRAPHIC_SIZE	// Only for orto cameras
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[SerializeField] private Camera m_camera = null;
	public Camera camera {
		get { return m_camera; }
		set { InitFromCamera(value, false); }
	}

	[SerializeField] private Mode m_mode = Mode.FOV;
	public Mode mode {
		get { return m_mode; }
		set { m_mode = value; SetRange(m_range); }		// Adjust range to new mode
	}

	// Zoom setup
	[Space]
	[SerializeField] private Range m_range = new Range(-10f, 10f);	// world units if Z_MOVE, angle if FOV, ortographic size if orto, all of them relative to the initial value
	public Range range {
		get { return m_range; }
		set { SetRange(value); }
	}

	[SerializeField] private float m_speed = 0.5f;	// units/touch delta
	public float speed {
		get { return m_speed; }
		set { m_speed = value; }
	}

	// Auto-restore value
	[Space]
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

	// Internal
	private float m_initialValue = -1f;
	private float m_value = -1f;
	private Range m_actualRange = new Range();

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
		InitFromCamera(m_camera, true);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Restore camera to its original value?
		if(m_restoreOnDisable) {
			RestoreOriginalValue();
		}
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Skip if disabled
		if(!isActiveAndEnabled) return;

		// Detect zoom
		float zoomSpeed = m_speed;
		bool zoomChanged = false;
		if(zoomSpeed > 0f) {
			// Aux vars
			float zoomOffset = 0f;

			// In editor, use mouse wheel
			#if UNITY_EDITOR
			// If mouse wheel has been scrolled
			if(Input.mouseScrollDelta.sqrMagnitude > Mathf.Epsilon) {
				// Change value size based on mouse wheel
				zoomOffset = Input.mouseScrollDelta.y * zoomSpeed;

				// Mark dirty
				zoomChanged = true;
			}

			// In device, use pinch gesture
			// From https://unity3d.com/learn/tutorials/topics/mobile-touch/pinch-zoom
			#else
			// If there are two touches on the device...
			if(Input.touchCount == 2) {
				// Store both touches.
				Touch touchZero = Input.GetTouch(0);
				Touch touchOne = Input.GetTouch(1);

				// Find the position in the previous frame of each touch.
				//Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				//Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
				Vector2 touchZeroPrevPos = touchZero.position - FixTouchDelta(touchZero);
				Vector2 touchOnePrevPos = touchOne.position - FixTouchDelta(touchOne);

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

				// Find the difference in the distances between each frame.
				float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

				// ... change the zoom value based on the change in distance between the touches.
				zoomOffset = deltaMagnitudeDiff * zoomSpeed;

				// Apply sensitivity correction based on platform
				// Compute corrected sensitivity
				switch(Application.platform) {
					case RuntimePlatform.IPhonePlayer:	zoomOffset *= DragControl.IOS_SENSITIVITY_CORRECTION;		break;
					case RuntimePlatform.Android:		zoomOffset *= DragControl.ANDROID_SENSITIVITY_CORRECTION;	break;
				}

				// Mark dirty
				zoomChanged = true;
			}
			#endif

			// Has zoom changed?
			if(zoomChanged) {
				// Process offset based on zoom mode
				switch(m_mode) {
					case Mode.Z_MOVE: {
						zoomOffset *= -1f;
					} break;
				}

				// Apply the zoom change!
				ApplyValue(m_value + zoomOffset);
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reset control to work with the given camera.
	/// </summary>
	/// <param name="_cam">New camera.</param>
	/// <param name="_force">Do all the initialization even if the target is the same as the current one.</param>
	private void InitFromCamera(Camera _cam, bool _force) {
		// If we have a valid camera already, reset it to its original position (if the flag says so)
		// Only if we're active!
		if(isActiveAndEnabled && m_restoreOnDisable) {
			RestoreOriginalValue();
		}

		// Store new camera
		Camera oldCamera = m_camera;
		m_camera = _cam;

		// Nothing else to do if new target camera is not valid
		if(m_camera == null) return;

		// Extra stuff only if the camera is different than the current one
		// Otherwise we would be overriding original values
		if(m_camera != oldCamera) {
			// Initialize with current target value
			m_value = GetValue();

			// Store target's original value
			m_initialValue = GetValue();

			// Properly initialize range
			SetRange(m_range);
		}
	}

	/// <summary>
	/// Restore current camera to its original value.
	/// Doesn't check the flag nor the state of the component.
	/// </summary>
	public void RestoreOriginalValue() {
		// Camera must be valid
		if(m_camera == null) return;

		// Animated?
		if(m_restoreDuration > 0f) {
			DOTween.To(
				() => { return m_value; }, 		// Getter
				(_v) => { ApplyValue(_v); },	// Setter
				m_initialValue, 
				m_restoreDuration
			)
				.SetUpdate(true)
				.SetEase(Ease.InOutQuad);
		} else {
			ApplyValue(m_initialValue);
		}
	}

	/// <summary>
	/// Depending on mode, get current value directly from the camera.
	/// </summary>
	/// <returns>The value.</returns>
	private float GetValue() {
		// Depends on mode
		switch(m_mode) {
			case Mode.FOV: {
				return m_camera.fieldOfView;
			} break;

			case Mode.Z_MOVE: {
				return m_camera.transform.localPosition.z;
			} break;

			case Mode.ORTOGRAPHIC_SIZE: {
				return m_camera.orthographicSize;
			} break;
		}
		return -1f;
	}

	/// <summary>
	/// Apply new value to the camera, depending on mode.
	/// No checks performed.
	/// </summary>
	/// <param name="_value">New value to be applied.</param>
	private void ApplyValue(float _value) {
		// Clamp new value to limits
		_value = m_actualRange.Clamp(_value);

		// Depends on mode
		switch(m_mode) {
			case Mode.FOV: {
				_value = Mathf.Clamp(_value, 1f, 179f);	// Limit FOV values!
				m_camera.fieldOfView = _value;
			} break;

			case Mode.Z_MOVE: {
				m_camera.transform.SetLocalPosZ(_value);
			} break;

			case Mode.ORTOGRAPHIC_SIZE: {
				_value = Mathf.Max(0.01f, _value);	// Avoid size <= 0!
				m_camera.orthographicSize = _value;
			} break;
		}

		// Store new value
		m_value = _value;
	}

	/// <summary>
	/// Compute the touch delta to be screen-independent.
	/// http://answers.unity3d.com/questions/209030/android-touch-variation-correction.html
	/// </summary>
	/// <returns>The corrected touch delta.</returns>
	/// <param name="_touch">Touch.</param>
	public Vector2 FixTouchDelta(Touch _touch) {
		// From Unity's doc:
		// The absolute position of the touch is recorded periodically and available 
		// in the position property. The deltaPosition value is a Vector2 that represents 
		// the difference between the touch position recorded on the most recent update and 
		// that recorded on the previous update. The deltaTime value gives the time that 
		// elapsed between the previous and current updates; you can calculate the touch's 
		// speed of motion by dividing deltaPosition.magnitude by deltaTime.
		float dt = Time.unscaledDeltaTime / _touch.deltaTime;
		if(float.IsNaN(dt) || float.IsInfinity(dt)) {
			dt = 1.0f;
		}
		return _touch.deltaPosition * dt;
	}

	/// <summary>
	/// Sets a new range.
	/// Range is always relative to camera's original position.
	/// </summary>
	/// <param name="_range">New range.</param>
	private void SetRange(Range _range) {
		// Store new range
		m_range = _range;

		// Compute actual range, considering initial value and mode's limitations
		m_actualRange.min = m_initialValue + m_range.min;
		m_actualRange.max = m_initialValue + m_range.max;
		switch(m_mode) {
			case Mode.FOV: {
				m_actualRange.min = Mathf.Max(1f, m_actualRange.min);
				m_actualRange.max = Mathf.Min(179f, m_actualRange.max);
			} break;

			case Mode.ORTOGRAPHIC_SIZE: {
				m_actualRange.min = Mathf.Max(0.01f, m_actualRange.min);	// Avoid size <= 0!
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
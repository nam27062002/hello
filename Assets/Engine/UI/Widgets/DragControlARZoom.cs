// DragControlARZoom.cs
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
public class DragControlARZoom : MonoBehaviour {	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Zoom setup
	[Space]
	[SerializeField] private Range m_range = new Range(1f, 10f);
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
        m_value = 0f;
        m_initialValue = 0f;
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
        m_initialValue = 0f;
        ApplyValue(m_initialValue);
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
	/// Restore current camera to its original value.
	/// Doesn't check the flag nor the state of the component.
	/// </summary>
	public void RestoreOriginalValue() {
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
	/// Apply new value to the AR kit, depending on mode.
	/// No checks performed.
	/// </summary>
	/// <param name="_value">New value to be applied.</param>
	private void ApplyValue(float _value) {
        // Clamp new value to limits
        _value = m_actualRange.Clamp(_value);

        ARKitManager.SharedInstance.ChangeZoom(_value);

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
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
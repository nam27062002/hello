// CameraPath.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[ExecuteInEditMode]
public class PathFollower : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum LinkMode {
		NOT_LINKED,
		DELTA,
		SNAP_POINT
	};
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// External refs
	[SerializeField] private Transform m_target = null;
	public Transform target {
		get { return m_target; }
		set { 
			m_target = value; 
			m_dirty = true;
		}
	}

	[SerializeField] private BezierCurve m_path = null;
	public BezierCurve path {
		get { return m_path; }
		set { 
			m_path = value; 
			m_dirty = true;
		}
	}

	// Properties
	[SerializeField] [Range(0f, 1f)] private float m_delta = 0f;
	public float delta {
		get { return m_delta; }
		set {
			// Apply to target object and mark delta as dirty
			m_delta = ClampDelta(value); 
			m_deltaDirty = true;
			Apply();
		}
	}

	public Vector3 position {
		get {
			// Apply offset!
			if(m_path == null) return Vector3.zero + offset;
			return m_path.GetValue(delta) + offset;
		}
	}

	[SerializeField] private int m_snapPoint = 0;
	public int snapPoint {
		get { 
			// If delta is dirty, compute again
			if(m_deltaDirty && m_path != null) {
				m_snapPoint = m_path.GetPointAt(m_delta);
				m_deltaDirty = false;
			}
			return m_snapPoint; 
		}
		set {
			// Update delta as well and apply
			if(m_path != null) {
				m_snapPoint = Mathf.Clamp(value, 0, m_path.pointCount - 1);
				m_delta = m_path.GetDelta(m_snapPoint);
				Apply();
			}
		}
	}

	public bool isTweening {
		get { return m_tween != null && m_tween.IsPlaying(); }
	}

	[SerializeField] private LinkMode m_linkMode = LinkMode.DELTA;
	public LinkMode linkMode {
		get { return m_linkMode; }
		set { 
			m_linkMode = value; 
			m_dirty = true;
		}
	}

	[Tooltip("Check for curve changes and keep position updated. Expensive, better avoid it.")]
	[SerializeField] private bool m_keepUpdated = false;
	public bool keepUpdated {
		get { return m_keepUpdated; }
		set { m_keepUpdated = value; }
	}

	// Extra
	[Space]
	[SerializeField] private Vector3 m_offset = Vector3.zero;
	public Vector3 offset {
		get { return m_offset; }
		set { 
			m_offset = value; 
			m_dirty = true;
		}
	}

	// Internal vars
	private Tweener m_tween = null;
	private bool m_dirty = true;
	private bool m_deltaDirty = true;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Make sure target's position is updated - except if tweening!
		// Skip if flag not set
		if(m_keepUpdated || !Application.isPlaying) {
			// [AOC] TODO!! This is highly inefficient, figure out a better way to do it
			if(m_dirty || (m_path != null && m_path.dirty)) {
				if(!isTweening) {
					switch(m_linkMode) {
						case LinkMode.DELTA: Apply(); break;
						case LinkMode.SNAP_POINT: SnapTo(snapPoint); break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Clear tween
		if(m_tween != null) {
			m_tween.Kill();
			m_tween = null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		if(!isActiveAndEnabled) return;

		if(m_target != null) {
			// Pos
			Gizmos.color = Colors.WithAlpha(Colors.red, 0.75f);
			Gizmos.DrawSphere(m_target.position, 1f);
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Applies current delta value to the target.
	/// Resets dirty flag.
	/// </summary>
	public void Apply() {
		// Check params
		if(m_target == null || m_path == null) return;

		// Just do it!
		m_target.position = position;

		// Not dirty anymore :)
		m_dirty = false;
	}

	/// <summary>
	/// Instanly go to a specific delta in the path.
	/// </summary>
	/// <param name="_delta">Target delta.</param>
	public void GoTo(float _delta) {
		// Check params
		if(m_target == null || m_path == null) return;

		// Set delta and apply
		delta = _delta;
		Apply();
	}

	/// <summary>
	/// Animate to a specific delta in the path.
	/// </summary>
	/// <param name="_delta">Target delta.</param>
	/// <param name="_seconds">Duration in seconds.</param>
	/// <param name="_ease">Optional ease function.</param>
	public void GoTo(float _delta, float _seconds, Ease _ease = Ease.InOutCubic) {
		// Check params
		if(m_target == null || m_path == null) return;

		// If path is not closed clamp target value
		if(!m_path.closed) {
			_delta = Mathf.Clamp01(_delta);
		}

		// If tween is not created, do it now
		if(m_tween == null) {
			m_tween = DOTween.To(
				() => { 
					return delta; 
				}, 
				_newValue => {
					m_delta = _newValue;	// Don't use property setter, which would kill the tween!
					m_delta = ClampDelta(m_delta);	// If path is closed, loop around ^_^
					m_deltaDirty = true;	// In case anyone wants to use the snapPoint property
					Apply();
				}, 
				_delta,
				_seconds)
				.SetAutoKill(false);
		} else {
			// Change tween's params and restart it
			m_tween.ChangeValues(delta, _delta, _seconds);
			m_tween.SetEase(_ease);
			m_tween.Restart();
		}
	}

	/// <summary>
	/// Instantly go to a specific point in the path.
	/// </summary>
	/// <param name="_snapPoint">Target control point.</param>
	/// <returns>The delta corresponding to the target snap point</returns>
	public float SnapTo(int _snapPoint) {
		// Check params
		if(m_target == null || m_path == null) return 0f;

		// Set the snap point and apply
		snapPoint = _snapPoint;
		Apply();
		return delta;
	}

	/// <summary>
	/// Animate to a specific point in the path.
	/// </summary>
	/// <param name="_snapPoint">Target control point.</param>
	/// <param name="_seconds">Duration in seconds.</param>
	/// <param name="_ease">Optional ease function.</param>
	/// <returns>The delta corresponding to the target snap point</returns>
	public float SnapTo(int _snapPoint, float _seconds, Ease _ease = Ease.InOutCubic) {
		// Check params
		if(m_target == null || m_path == null) return 0f;

		// Just use the delta version with the same parameters
		float targetDelta = m_path.GetDelta(_snapPoint);
		GoTo(targetDelta, _seconds, _ease);
		return targetDelta;
	}

	/// <summary>
	/// Stop if animation is running.
	/// Call it before manually setting a delta value, otherwise it will be overriden by the animation.
	/// </summary>
	public void Stop() {
		// Just do it
		if(m_tween != null && m_tween.IsPlaying()) {
			m_tween.Pause();
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Clamps the delta between 0 and 1.
	/// If the path is closed, we will loop. Otherwise delta will be just clamped.
	/// </summary>
	/// <returns>New clamped delta value.</returns>
	/// <param name="_delta">Delta value to be clamped.</param>
	private float ClampDelta(float _delta) {
		// If path is not null and it's closed, loop around
		if(m_path != null && m_path.closed) {
			while(_delta < 0f) _delta += 1f;
			while(_delta >= 1f) _delta -= 1f;	// If 1f, set 0f
		} else {
			_delta = Mathf.Clamp01(_delta);
		}
		return _delta;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}
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
public class PathFollower : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// External refs
	[SerializeField] private Transform m_target = null;
	public Transform target {
		get { return m_target; }
	}

	[SerializeField] private BezierCurve m_path = null;
	public BezierCurve path {
		get { return m_path; }
	}

	// Properties
	[SerializeField] [Range(0f, 1f)] private float m_delta = 0f;
	public float delta {
		get { return m_delta; }
		set {
			m_delta = Mathf.Clamp01(value); 
			Apply();
		}
	}

	public Vector3 position {
		get {
			if(m_path == null) return Vector3.zero;
			return m_path.GetValue(delta); 
		}
	}

	public int snapPoint {
		get { 
			if(m_path == null) return 0;
			return m_path.GetPointAt(delta);
		}
		set {
			if(m_path == null) return;
			delta = m_path.GetDelta(value);
		}
	}

	// Internal vars
	private Tweener m_tween = null;

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
		m_target.position = m_path.GetValue(m_delta);
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

		// Make sure target value is valid
		_delta = Mathf.Clamp01(_delta);

		// If tween is not created, do it now
		if(m_tween == null) {
			m_tween = DOTween.To(
				() => { 
					return delta; 
				}, 
				_newValue => { 
					m_delta = _newValue;	// Don't use property setter, which would kill the tween!
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
	// CALLBACKS														//
	//------------------------------------------------------------------//
}
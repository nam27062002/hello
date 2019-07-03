// UIRotator.cs
// 
// Created by Alger Ortín Castellví on 21/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiny component to rotate UI elements based on a percentage and an angle range.
/// </summary>
[ExecuteInEditMode]
public class UIRotator : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Origin {
		TOP,
		RIGHT,
		BOTTOM,
		LEFT
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private RectTransform[] m_targets = new RectTransform[0];

	[Space]

	[SerializeField] private Range m_angleRange = new Range(0f, 360f);
	public Range angleRange {
		get { return m_angleRange; }
		set { m_angleRange = value; Refresh(); }
	}

	[SerializeField] private bool m_rotateTargets = false;
	[SerializeField] private Origin m_origin = Origin.TOP;
	[SerializeField] private bool m_clockwise = true;

	[Space]

	[SerializeField] private float m_radius = 10f;

	[Range(0f, 1f)]
	[SerializeField] private float m_currentValue = 0f;
	public float currentValue {
		get { return m_currentValue; }
		set { m_currentValue = value; Refresh(); }
	}

	[SerializeField] private float m_angle = 0f;
	public float angle {
		get { return m_angle; }
		set { currentValue = m_angleRange.InverseLerp(value); }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Do a first refresh
		Refresh();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Refresh
		Refresh();
	}

	/// <summary>
	/// A change has occurred in the inspector.
	/// </summary>
	private void OnValidate() {
		// Refresh
		Refresh();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Check for transform changes
		if(transform.hasChanged) {
			Refresh();
			transform.hasChanged = false;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do the layout!
	/// </summary>
	private void Refresh() {
		// Update angle
		m_angle = m_angleRange.Lerp(currentValue);

		// Compute actual angle
		float targetAngle = CorrectAngle(m_angle);

		// Compute position
		Vector2 pos = new Vector2(
			m_radius * Mathf.Cos(targetAngle * Mathf.Deg2Rad),
			m_radius * Mathf.Sin(targetAngle * Mathf.Deg2Rad)
		);

		// Apply to each item!
		RectTransform rt = null;
		for(int i = 0; i < m_targets.Length; ++i) {
			// Skip if target is not active
			if(!m_targets[i].gameObject.activeSelf) continue;

			// Shorter notation
			rt = m_targets[i];

			// Apply rotation to target item?
			if(m_rotateTargets) {
				rt.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
			}

			// Apply offset
			rt.anchoredPosition = pos;
		}
	}

	/// <summary>
	/// Modify the given angle applying the different setup parameters of the rotator.
	/// </summary>
	/// <returns>The corrected angle.</returns>
	/// <param name="_angle">Angle to be corrected.</param>
	private float CorrectAngle(float _angle) {
		// Compute actual angle
		float targetAngle = _angle;

		// Reverse?
		if(m_clockwise) {
			targetAngle = 360f - targetAngle;
		}

		// Apply offset based on origin
		switch(m_origin) {
			case Origin.TOP: targetAngle += 90f; break;
			case Origin.RIGHT: targetAngle += 0f; break;
			case Origin.BOTTOM: targetAngle -= 90f; break;
			case Origin.LEFT: targetAngle += 180f; break;
		}

		return targetAngle;
	}

	//------------------------------------------------------------------------//
	// EDITOR																  //
	//------------------------------------------------------------------------//
#if UNITY_EDITOR
	/// <summary>
	/// Editor helper.
	/// </summary>
	private void OnDrawGizmosSelected() {
		// Nothing if component is not enabled
		if(!this.isActiveAndEnabled) return;

		// Set Handles matrix to this object's transform
		RectTransform rt = this.transform as RectTransform;
		Handles.matrix = rt.localToWorldMatrix;

		// Draw angle range
		Handles.color = Colors.WithAlpha(Colors.skyBlue, 0.25f);
		Handles.DrawSolidArc(
			Vector3.zero,
			Vector3.back,
			Vector3.right.RotateXYDegrees(CorrectAngle(m_angleRange.min)),
			m_clockwise ? m_angleRange.distance : -m_angleRange.distance,
			m_radius
		);

		// Draw arrow body
		Handles.color = Color.red;
		float arrowAngleDist = Mathf.Min(m_angleRange.distance * 0.30f, 60f);    // Xdeg or 30% of the arc if distance is less than Xdeg
		arrowAngleDist = m_clockwise ? arrowAngleDist : -arrowAngleDist;
		Handles.DrawWireArc(
			Vector3.zero,
			Vector3.back,
			Vector3.right.RotateXYDegrees(CorrectAngle(m_angleRange.min)),
			arrowAngleDist,
			m_radius
		);

		// Draw arrow tip
		float arrowAngle = CorrectAngle(m_angleRange.min + (m_clockwise ? arrowAngleDist : -arrowAngleDist));
		Vector3 arrowTipPoint = Vector3.right * m_radius;
		arrowTipPoint = arrowTipPoint.RotateXYDegrees(arrowAngle);
		Handles.matrix = Handles.matrix * Matrix4x4.Translate(arrowTipPoint);
		Handles.matrix = Handles.matrix * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, arrowAngle));
		float arrowWingsSize = m_radius * 0.1f;	// Proportional to the radius
		Handles.DrawAAPolyLine(
			new Vector3(1f, m_clockwise ? 1f : -1f, 0f).normalized * arrowWingsSize,
			Vector3.zero,
			new Vector3(-1f, m_clockwise ? 1f : -1f, 0f).normalized * arrowWingsSize
		);

		// Restore Handles matrix and color :)
		Handles.matrix = Matrix4x4.identity;
		Handles.color = Color.white;
	}
#endif
}
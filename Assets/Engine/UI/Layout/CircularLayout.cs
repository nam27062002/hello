// CircularLayout.cs
// 
// Created by Alger Ortín Castellví on 03/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// Custom layout to distribute elements circularly.
/// </summary>
[ExecuteInEditMode]
public class CircularLayout : MonoBehaviour {
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
	[SerializeField] private Origin m_origin = Origin.TOP;
	public Origin origin {
		get { return m_origin; }
		set { m_origin = value; Refresh(); }
	}

	[SerializeField] private bool m_rotateTargets = false;
	public bool rotateTargets {
		get { return m_rotateTargets; }
		set { m_rotateTargets = value; Refresh(); }
	}

	[SerializeField] private bool m_clockwise = true;
	public bool clockwise {
		get { return m_clockwise; }
		set { m_clockwise = value; Refresh(); }
	}

	[SerializeField] private bool m_skipMinAngle = false;
	public bool skipMinAngle {
		get { return m_skipMinAngle; }
		set { m_skipMinAngle = value; Refresh(); }
	}

	[SerializeField] private bool m_skipMaxAngle = false;
	public bool skipMaxAngle {
		get { return m_skipMaxAngle; }
		set { m_skipMaxAngle = value; Refresh(); }
	}

	[Space]
	[SerializeField] private float m_radius = 10f;
	public float radius {
		get { return m_radius; }
		set { m_radius = value; Refresh(); }
	}

	[SerializeField] private Range m_angleRange = new Range(0f, 360f);
	public Range angleRange {
		get { return m_angleRange; }
		set { m_angleRange = value; Refresh(); }
	}

	// Internal
	private RectTransform[] m_items = null;

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
		// Refresh items list
		int childCount = transform.childCount;
		int activeItemCount = -1;
		if(m_items == null || m_items.Length != childCount) {
			// New children! Repopulate array
			m_items = new RectTransform[childCount];
			activeItemCount = 0;
			for(int i = 0; i < childCount; ++i) {
				// Store new item ref
				m_items[i] = transform.GetChild(i) as RectTransform;

				// Reuse the loop to already count active items
				if(m_items[i].gameObject.activeSelf) {
					activeItemCount++;
				}
			}
		}

		// If not already done, check how many active items we have
		if(activeItemCount < 0) {
			activeItemCount = 0;
			for(int i = 0; i < childCount; ++i) {
				// If item is null, something went wrong!
				if(m_items[i] == null) {
					// Clear items array and force a new refresh
					m_items = null;
					Refresh();
					return;
				} else if(m_items[i].gameObject.activeSelf) {
					activeItemCount++;
				}
			}
		}

		// Do it
		if(childCount > 0) {
			// Compute angle between items
			float divisions = (float)activeItemCount - 1f;
			if(m_skipMinAngle) divisions += 1f;
			if(m_skipMaxAngle) divisions += 1f;
			if(divisions <= 0f) divisions = 1f;
			float angleBetweenItems = m_angleRange.distance / divisions;

			// Compute initial angle
			float angle = m_angleRange.min;
			if(m_skipMinAngle) angle += angleBetweenItems;

			// Apply to each item!
			float correctedAngle = 0f;
			for(int i = 0; i < childCount; ++i) {
				// Skip if item is not active
				if(!m_items[i].gameObject.activeSelf) continue;

				// Compute actual angle
				correctedAngle = CorrectAngle(angle);

				// Compute position
				Vector2 pos = new Vector2(
					m_radius * Mathf.Cos(correctedAngle * Mathf.Deg2Rad),
					m_radius * Mathf.Sin(correctedAngle * Mathf.Deg2Rad)
				);

				// Apply rotation to target item?
				if(m_rotateTargets) {
					m_items[i].localRotation = Quaternion.Euler(0f, 0f, correctedAngle);
				}

				// Apply offset
				m_items[i].anchoredPosition = pos;

				// Increase angle
				angle += angleBetweenItems;
			}
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
			RotateXYDegrees(Vector3.right, CorrectAngle(m_angleRange.min)),
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
			RotateXYDegrees(Vector3.right, CorrectAngle(m_angleRange.min)),
			arrowAngleDist,
			m_radius
		);

		// Draw arrow tip
		float arrowAngle = CorrectAngle(m_angleRange.min + (m_clockwise ? arrowAngleDist : -arrowAngleDist));
		Vector3 arrowTipPoint = Vector3.right * m_radius;
		arrowTipPoint = RotateXYDegrees(arrowTipPoint, arrowAngle);
		Handles.matrix = Handles.matrix * Matrix4x4.Translate(arrowTipPoint);
		Handles.matrix = Handles.matrix * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, arrowAngle));
		float arrowWingsSize = m_radius * 0.1f; // Proportional to the radius
		Handles.DrawAAPolyLine(
			new Vector3(1f, m_clockwise ? 1f : -1f, 0f).normalized * arrowWingsSize,
			Vector3.zero,
			new Vector3(-1f, m_clockwise ? 1f : -1f, 0f).normalized * arrowWingsSize
		);

		// Restore Handles matrix and color :)
		Handles.matrix = Matrix4x4.identity;
		Handles.color = Color.white;
	}

	/// <summary>
	/// Rotate the given vector in the XY plane.
	/// </summary>
	/// <returns>The original vector rotated in the XY plane.</returns>
	/// <param name="_v">Vector to be rotated.</param>
	/// <param name="_angle">Angle to rotate.</param>
	private Vector3 RotateXYDegrees(Vector3 _v, float _angle) {
		_angle *= Mathf.Deg2Rad;
		float sin = Mathf.Sin(_angle);
		float cos = Mathf.Cos(_angle);
		float x = _v.x;
		float y = _v.y;
		return new Vector3(x * cos - y * sin, x * sin + y * cos);
	}
#endif
}
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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
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
			// New children! Repoppulate array
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
				if(m_items[i].gameObject.activeSelf) {
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
			for(int i = 0; i < childCount; ++i) {
				// Skip if item is not active
				if(!m_items[i].gameObject.activeSelf) continue;

				// Aux vars
				RectTransform rt = m_items[i];

				// Apply rotation to target item
				rt.localRotation = Quaternion.Euler(0f, 0f, -angle);
				rt.anchoredPosition = GameConstants.Vector2.zero;

				// Increase angle
				angle += angleBetweenItems;
			}
		}
	}

	//------------------------------------------------------------------------//
	// EDITOR																  //
	//------------------------------------------------------------------------//
#if UNITY_EDITOR
	/// <summary>
	/// Editor helper.
	/// </summary>
	private void OnDrawGizmosSelected() {
		if(!this.isActiveAndEnabled) return;

		RectTransform rt = this.transform as RectTransform;
		Handles.matrix = rt.localToWorldMatrix;
		Handles.color = Colors.WithAlpha(Colors.skyBlue, 0.25f);
		Handles.DrawSolidArc(
			Vector3.zero,
			Vector3.back,
			Vector3.down.RotateXYDegrees(-m_angleRange.min),
			m_angleRange.distance,
			Mathf.Min(rt.rect.width, rt.rect.height) / 2f
		);
		Handles.matrix = Matrix4x4.identity;
		Handles.color = Color.white;
	}
#endif
}
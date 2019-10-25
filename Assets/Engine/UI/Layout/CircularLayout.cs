// CircularLayout.cs
// 
// Created by Alger Ortín Castellví on 03/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections.Generic;

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
	[Tooltip("Layout's circumference radius")]
	[SerializeField] private float m_radius = 10f;
	public float radius {
		get { return m_radius; }
		set { m_radius = value; Refresh(); }
	}

	[Tooltip("Where to start the layout's arc")]
	[SerializeField] private Origin m_origin = Origin.TOP;
	public Origin origin {
		get { return m_origin; }
		set { m_origin = value; Refresh(); }
	}

	[Tooltip("Direction of items distribution, following hierarchy order")]
	[SerializeField] private bool m_clockwise = true;
	public bool clockwise {
		get { return m_clockwise; }
		set { m_clockwise = value; Refresh(); }
	}

	[SerializeField] private float m_minAngle = 0f;
	public float minAngle {
		get { return m_minAngle; }
		set { m_minAngle = value; Refresh(); }
	}

	[SerializeField] private float m_maxAngle = 360f;
	public float maxAngle {
		get { return m_maxAngle; }
		set { m_maxAngle = value; Refresh(); }
	}

	[Tooltip("Put first item at min angle or at the slot after?\n" +
		"Useful, for example, when doing a 0-360 layout to avoid first and last item overlapping.")]
	[SerializeField] private bool m_skipMinAngle = false;
	public bool skipMinAngle {
		get { return m_skipMinAngle; }
		set { m_skipMinAngle = value; Refresh(); }
	}

	[Tooltip("Put last item at max angle or at the slot before?\n" +
		"Useful, for example, when doing a 0-360 layout to avoid first and last item overlapping.")]
	[SerializeField] private bool m_skipMaxAngle = false;
	public bool skipMaxAngle {
		get { return m_skipMaxAngle; }
		set { m_skipMaxAngle = value; Refresh(); }
	}

	[Space]
	[FormerlySerializedAs("m_rotateTargets")]
	[Tooltip("Rotate items so they follow the curvature?")]
	[SerializeField] private bool m_rotateItemsTangentially = false;
	public bool rotateItemsTangentially {
		get { return m_rotateItemsTangentially; }
		set { m_rotateItemsTangentially = value; Refresh(); }
	}

	[Tooltip("Add extra rotation to each item individually")]
	[SerializeField] private float m_itemRotationOffset = 0f;
	public float itemRotationOffset {
		get { return m_itemRotationOffset; }
		set { m_itemRotationOffset = value; Refresh(); }
	}

	// Internal
	private List<RectTransform> m_items = new List<RectTransform>();

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
		RefreshItems();
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
	/// The list of children of the transform of the GameObject has changed.
	/// </summary>
	private void OnTransformChildrenChanged() {
		// Refresh items!
		RefreshItems();
		Refresh();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the list of items.
	/// Only call when strictly needed!
	/// </summary>
	public void RefreshItems() {
		// Clear current items
		m_items.Clear();

		// Iterate children looking for valid items
		RectTransform item = null;
		LayoutElement element = null;
		int childCount = transform.childCount;
		for(int i = 0; i < childCount; ++i) {
			// Get item
			item = transform.GetChild(i) as RectTransform;

			// Ignore if it doesn't have a rect transform
			if(item == null) continue;

			// Ignore if it has a LayoutElement component telling it to ignore the layout
			element = item.GetComponent<LayoutElement>();
			if(element != null && element.ignoreLayout) continue;

			// Item is valid! Add it to the layout
			m_items.Add(item);
		}
	}

	/// <summary>
	/// Do the layout!
	/// </summary>
	public void Refresh() {
		// Check how many active items we have
		int itemCount = m_items.Count;
		int activeItemCount = 0;
		for(int i = 0; i < itemCount; ++i) {
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

		// Do it
		if(itemCount > 0) {
			// Compute angle between items
			float divisions = (float)activeItemCount - 1f;
			if(m_skipMinAngle) divisions += 1f;
			if(m_skipMaxAngle) divisions += 1f;
			if(divisions <= 0f) divisions = 1f;
			float angleBetweenItems = Mathf.Abs(m_maxAngle - m_minAngle) / divisions;

			// Compute initial angle
			float angle = m_minAngle;
			if(m_skipMinAngle) angle += angleBetweenItems;

			// Apply to each item!
			float correctedAngle = 0f;
			for(int i = 0; i < itemCount; ++i) {
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
				if(m_rotateItemsTangentially) {
					m_items[i].localRotation = Quaternion.Euler(0f, 0f, correctedAngle + m_itemRotationOffset);
				} else {
					m_items[i].localRotation = Quaternion.Euler(0f, 0f, m_itemRotationOffset);
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
	public float CorrectAngle(float _angle) {
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
}
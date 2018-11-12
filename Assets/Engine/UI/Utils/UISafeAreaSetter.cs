// UISafeAreaSetter.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/12/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Utility class to modify object's transform based on safe area settings.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UISafeAreaSetter : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Mode {
		SIZE_DECREASE,
		POSITION,
		SIZE_INCREASE,
		LAYOUT_PADDING
	}

	[Serializable]
	public class Action {
		public Mode mode = Mode.POSITION;
		public UISafeArea scale = new UISafeArea(1f, 1f, 1f, 1f);

		public override string ToString() {
			return mode.ToString() + " | " + scale.ToString();
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Reorderable("ACTION", true, false)]
	[SerializeField] private List<Action> m_actions = new List<Action>();
	public List<Action> actions {
		get { return m_actions; }
	}

	// Debug only
#if DEBUG
	[Separator("Debug")]
	[SerializeField] private Vector2 m_originalOffsetMin = Vector2.zero;
	[SerializeField] private Vector2 m_originalOffsetMax = Vector2.zero;
	[SerializeField] private Vector2 m_originalAnchoredPos = Vector2.zero;
	[SerializeField] private RectOffset m_originalLayoutPadding = new RectOffset();
	private bool m_originalValuesBackedUp = false;
#endif

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
		Apply();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply safe area based on current device.
	/// </summary>
	public void Apply() {
		// Get Rect transform ref
		RectTransform rt = GetComponent<RectTransform>();

		// Debug only: restore original values
#if DEBUG
		RestoreOriginalValues();
#endif

		// Get safe area
		UISafeArea safeArea = UIConstants.safeArea;

		// Perform all actions by order
		for(int i = 0; i < m_actions.Count; ++i) {
			// Apply scale
			UISafeArea scaledSafeArea = new UISafeArea(
				safeArea.left * m_actions[i].scale.left,
				safeArea.top * m_actions[i].scale.top,
				safeArea.right * m_actions[i].scale.right,
				safeArea.bottom * m_actions[i].scale.bottom
			);

			// Apply based on mode
			switch(m_actions[i].mode) {
				case Mode.SIZE_DECREASE: {
						// Adjust both offsets
						rt.offsetMin = new Vector2(
							rt.offsetMin.x + scaledSafeArea.left,
							rt.offsetMin.y + scaledSafeArea.bottom
						);

						rt.offsetMax = new Vector2(
							rt.offsetMax.x - scaledSafeArea.right,
							rt.offsetMax.y - scaledSafeArea.top
						);
					}
					break;

				case Mode.SIZE_INCREASE: {
						// Adjust both offsets
						rt.offsetMin = new Vector2(
							rt.offsetMin.x - scaledSafeArea.left,
							rt.offsetMin.y - scaledSafeArea.bottom
						);

						rt.offsetMax = new Vector2(
							rt.offsetMax.x + scaledSafeArea.right,
							rt.offsetMax.y + scaledSafeArea.top
						);
					}
					break;

				case Mode.POSITION: {
						// Select which margins to apply in each axis based on anchors, and do it
						Vector2 newAnchoredPos = rt.anchoredPosition;

						// [AOC] TODO!! Research interpolating offset based on actual anchor value

						// X
						/*Vector2 anchorMin = rt.anchorMin;
						Vector2 anchorMax = rt.anchorMax;
						if(anchorMin.x < 0.5f && anchorMax.x < 0.5f) {
							newAnchoredPos.x += scaledSafeArea.left;
						} else if(anchorMin.x > 0.5f && anchorMax.x > 0.5f) {
							newAnchoredPos.x -= scaledSafeArea.right;
						} else {
							// Don't move!
						}

						// Y
						if(anchorMin.y < 0.5f && anchorMax.y < 0.5f) {
							newAnchoredPos.y += scaledSafeArea.bottom;
						} else if(anchorMin.y > 0.5f && anchorMax.y > 0.5f) {
							newAnchoredPos.y -= scaledSafeArea.top;
						} else {
							// Don't move!!
						}*/

						// Define the weights using the inspector
						newAnchoredPos.x += scaledSafeArea.left;
						newAnchoredPos.x -= scaledSafeArea.right;
						newAnchoredPos.y += scaledSafeArea.bottom;
						newAnchoredPos.y -= scaledSafeArea.top;

						// Apply!
						rt.anchoredPosition = newAnchoredPos;
					}
					break;

				case Mode.LAYOUT_PADDING: {
						// Requires a layout
						HorizontalOrVerticalLayoutGroup layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
						if(layout == null) break;

						// Adjust padding
						RectOffset newPadding = layout.padding;
						newPadding.left += (int)scaledSafeArea.left;
						newPadding.bottom += (int)scaledSafeArea.bottom;
						newPadding.right += (int)scaledSafeArea.right;
						newPadding.top += (int)scaledSafeArea.top;

						// Apply new padding
						layout.padding = newPadding;
					}
					break;
			}
		}
	}

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
#if DEBUG
	/// <summary>
	/// Restores the values stored as original (if any).
	/// </summary>
	public void RestoreOriginalValues() {
		// Aux vars
		RectTransform rt = GetComponent<RectTransform>();

		// If original values were not backed up, do it now and return (no need to apply anything)
		if(!m_originalValuesBackedUp) {
			BackupOriginalValues();
			return;
		}

		// Apply original values
		rt.offsetMin = m_originalOffsetMin;
		rt.offsetMax = m_originalOffsetMax;
		rt.anchoredPosition = m_originalAnchoredPos;
		HorizontalOrVerticalLayoutGroup layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
		if(layout != null) {
			layout.padding = m_originalLayoutPadding;
		}
	}

	/// <summary>
	/// Backups current values as the original ones.
	/// </summary>
	public void BackupOriginalValues() {
		RectTransform rt = GetComponent<RectTransform>();
		m_originalOffsetMin = rt.offsetMin;
		m_originalOffsetMax = rt.offsetMax;
		m_originalAnchoredPos = rt.anchoredPosition;
		HorizontalOrVerticalLayoutGroup layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
		if(layout != null) {
			m_originalLayoutPadding = layout.padding;
		}

		m_originalValuesBackedUp = true;
	}
#endif

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
// UISafeAreaSetter.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/12/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

using System;

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
		SIZE_INCREASE
	}

	[Serializable]
	public class OverrideData {
		[HideEnumValues(false, true)]
		public UIConstants.SpecialDevice device = UIConstants.SpecialDevice.NONE;
		public UISafeArea safeArea = new UISafeArea();
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Mode m_adjustMode = Mode.POSITION;

	[SerializeField] private OverrideData[] m_customSafeAreas = new OverrideData[0];
	
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

		// Get safe area
		UISafeArea safeArea = UIConstants.safeArea;

		// Override?
		for(int i = 0; i < m_customSafeAreas.Length; ++i) {
			// Matching device?
			if(m_customSafeAreas[i].device == UIConstants.specialDevice) {
				// Yes! Override safe area
				safeArea = m_customSafeAreas[i].safeArea;
				break;
			}
		}

		// Apply based on mode
		switch(m_adjustMode) {
			case Mode.SIZE_DECREASE: {
				// Adjust both offsets
				rt.offsetMin = new Vector2(
					rt.offsetMin.x + safeArea.left,
					rt.offsetMin.y + safeArea.bottom
				);

				rt.offsetMax = new Vector2(
					rt.offsetMax.x - safeArea.right,
					rt.offsetMax.y - safeArea.top
				);
			} break;

			case Mode.SIZE_INCREASE: {
				// Adjust both offsets
				rt.offsetMin = new Vector2(
					rt.offsetMin.x - safeArea.left,
					rt.offsetMin.y - safeArea.bottom
				);

				rt.offsetMax = new Vector2(
					rt.offsetMax.x + safeArea.right,
					rt.offsetMax.y + safeArea.top
				);
			} break;

			case Mode.POSITION: {
				// Select which margins to apply in each axis based on anchors, and do it
				Vector2 newAnchoredPos = rt.anchoredPosition;

				// [AOC] TODO!! Research interpolating offset based on actual anchor value

				// X
				Vector2 anchorMin = rt.anchorMin;
				Vector2 anchorMax = rt.anchorMax;
				if(anchorMin.x < 0.5f && anchorMax.x < 0.5f) {
					newAnchoredPos.x += safeArea.left;
				} else if(anchorMin.x > 0.5f && anchorMax.x > 0.5f) {
					newAnchoredPos.x -= safeArea.right;
				} else {
					// Don't move!
				}

				// Y
				if(anchorMin.y < 0.5f && anchorMax.y < 0.5f) {
					newAnchoredPos.y += safeArea.bottom;
				} else if(anchorMin.y > 0.5f && anchorMax.y > 0.5f) {
					newAnchoredPos.y -= safeArea.top;
				} else {
					// Don't move!!
				}

				// Apply!
				rt.anchoredPosition = newAnchoredPos;
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
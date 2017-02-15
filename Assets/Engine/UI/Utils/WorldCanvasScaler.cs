// WorldCanvasScaler.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to adjust a World Canvas's scale factor to fit a number of world 
/// units while using a reference resolution internally.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(RectTransform))]
public class WorldCanvasScaler : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Vector2 m_worldSize = new Vector2(40f, 30f);
	[SerializeField] private Vector2 m_referenceSize = new Vector2(2048f, 1536f);	// [AOC] Retina display

	// Internal references
	private RectTransform rectTransform {
		get { return transform as RectTransform; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A value has been changed on the inspector.
	/// </summary>
	private void OnValidate() {
		Apply();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do the maths and apply changes.
	/// </summary>
	private void Apply() {
		// Figure out target aspect ratio
		// If > 1f, width bigger than height, otherwise the other way around
		float ar = m_worldSize.x/m_worldSize.y;

		// Use bigger side to compute the target scale with relation to the reference size
		float scale = 1f;
		Vector2 newSizeDelta = m_worldSize;
		if(ar > 1f) {
			// Width bigger than height
			scale = m_worldSize.x/m_referenceSize.x;

			// Compute actual size respecting desired aspect ratio
			newSizeDelta.x = m_referenceSize.x;
			newSizeDelta.y = m_referenceSize.x / ar;
		} else {
			// Height bigger than width (or equal)
			scale = m_worldSize.y/m_referenceSize.y;

			// Compute actual size respecting desired aspect ratio
			newSizeDelta.x = m_referenceSize.y * ar;
			newSizeDelta.y = m_referenceSize.y;
		}

		// Apply scale and target size
		rectTransform.sizeDelta = newSizeDelta;
		rectTransform.localScale = new Vector3(scale, scale, 1f);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
// AspectRatioScaler.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to scale a transform based on screen's aspect ratio.
/// </summary>
public class AspectRatioScaler : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	private class AspectRatioData {
		public float ar = 1.777777776f;
		public float scale = 1f;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[Comment("Sorted from min to max AR")]
	[SerializeField] private AspectRatioData[] m_aspectRatios = new AspectRatioData[0];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		Apply();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do it!
	/// </summary>
	private void Apply() {
		float ar = UIConstants.ASPECT_RATIO;
		float newScale = 1f;
		for(int i = 0; i < m_aspectRatios.Length; ++i) {
			// Find our AR match
			if(i == 0 && ar < m_aspectRatios[i].ar) {
				// First item: if lower AR, use this scale factor
				newScale = m_aspectRatios[i].scale;
				break;
			} else if(i == m_aspectRatios.Length - 1) {
				// Last item: use this scale factor
				newScale = m_aspectRatios[i].scale;
				break;
			} else if(ar >= m_aspectRatios[i].ar && ar < m_aspectRatios[i+1].ar) {
				// In between items: use this scale factor if current AR is between this one and the next
				newScale = m_aspectRatios[i].scale;
				break;
			}
		}
		this.transform.localScale = new Vector3(newScale, newScale, newScale);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
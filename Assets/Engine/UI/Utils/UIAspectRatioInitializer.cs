// UIAspectRatioInitializer.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to initialize an AspectRatioFitter with an Image's original aspect ratio.
/// </summary>
public class UIAspectRatioInitializer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private AspectRatioFitter m_target = null;
	[SerializeField] private Image m_referenceImage = null;
	
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
	/// Initialize the Aspect Ratio fitter with the image's source aspect ratio.
	/// </summary>
	public void Apply() {
		// Check required stuff
		Debug.Assert(m_target != null, Colors.red.Tag("TARGET AspectRatioFitter NOT DEFINED"), this);
		Debug.Assert(m_referenceImage != null, Colors.red.Tag("REFERENCE IMAGE NOT DEFINED"), this);
		Debug.Assert(m_referenceImage.sprite != null, Colors.red.Tag("REFERENCE IMAGE HAS NO SPRITE ASSIGNED"), this);

		// Figure out image's original AR
		float ar = m_referenceImage.sprite.bounds.size.x / m_referenceImage.sprite.bounds.size.y;

		// Define it as aspect ratio fitter target AR
		m_target.aspectRatio = ar;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
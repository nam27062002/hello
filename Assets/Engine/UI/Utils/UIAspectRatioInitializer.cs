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
[ExecuteInEditMode]
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
	[Delayed]
    [SerializeField] private float m_minAspectRatio = 0;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		Apply();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Only while editing
		if(!Application.isPlaying) {
			Apply();
		}
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
        
		// Figure out screen's AR
		float screenW = Screen.width;
		float screenH = Screen.height;
		float screenAR = screenW / screenH;

		// Figure out sprite's original AR
		float spriteAR = m_referenceImage.sprite.bounds.size.x / m_referenceImage.sprite.bounds.size.y;

		// If the screen's AR is lower than the minimum AR we want for the image, override the AR Fitter
		if(screenAR < m_minAspectRatio) {
			// Disable aspect ratio fitter 
			m_target.enabled = false;

			// Make sure anchors are not messing up either
			RectTransform rt = this.transform as RectTransform;
			rt.anchorMin = GameConstants.Vector2.center;
			rt.anchorMax = GameConstants.Vector2.center;

			// Maximum height of the image within this canvas resolution
			Canvas canvas = GetComponentInParent<Canvas>();
			RectTransform canvasRt = canvas.transform as RectTransform;
			float maxH = canvasRt.sizeDelta.x / m_minAspectRatio;
			rt.sizeDelta = new Vector2(
				maxH * spriteAR,	// Keep original image AR
				maxH
			);
		} else {
			// The AR Fitter will keep the image's original AR as target and envelope the screen with it
			m_target.enabled = true;
			m_target.aspectRatio = spriteAR;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
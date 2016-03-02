// AdjustCameraViewportToRectTransform.cs
// 
// Created by Alger Ortín Castellví on 01/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Script to fit a camer viewport into its rect transform dimensions.
/// See http://answers.unity3d.com/questions/956402/convert-ui-image-size-and-position-to-camera-viewp.html
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class AdjustCameraViewportToRectTransform : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private Camera m_cam = null;
	private Canvas m_transformParentCanvas = null;
	[SerializeField] private RectTransform m_rectTransform = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Check required fields
		Debug.Assert(m_rectTransform != null, "Required field!");

		// Find the canvas containing the reference transform
		m_transformParentCanvas = m_rectTransform.GetComponentInParent<Canvas>();
		Debug.Assert(m_transformParentCanvas != null, "The reference rect transform must be within a UI Canvas.");

		// Get the reference to the camera
		m_cam = GetComponent<Camera>();
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Make sure we have required references
		if(m_cam == null) return;

		if(m_rectTransform == null) return;

		if(m_transformParentCanvas == null) {
			m_transformParentCanvas = m_rectTransform.GetComponentInParent<Canvas>();
			if(m_transformParentCanvas == null) return;
		}

		// Make sure viewport is always updated
		// Scale ratio between the parent canvas reference size and the actual screen size
		RectTransform canvasRectTransform = m_transformParentCanvas.transform as RectTransform;
		float canvasWidthRatio = canvasRectTransform.rect.width / Screen.width;
		float canvasHeightRatio = canvasRectTransform.rect.height / Screen.height;

		// Viewport size
		Vector2 viewportSize = new Vector2(
			m_rectTransform.rect.width / canvasWidthRatio,
			m_rectTransform.rect.height / canvasHeightRatio
		);

		// Viewprt position
		// These should correspond to screen space coordinates
		Vector2 viewportPos = Vector2.zero;
		switch(m_transformParentCanvas.renderMode) {
			case RenderMode.ScreenSpaceOverlay: {
				viewportPos = m_rectTransform.TransformPoint(m_rectTransform.rect.position.x, m_rectTransform.rect.position.y, 0f);
			} break;

			case RenderMode.ScreenSpaceCamera: {
				Vector3 worldPos = m_rectTransform.TransformPoint(m_rectTransform.rect.position.x, m_rectTransform.rect.position.y, 0f);
				viewportPos = m_transformParentCanvas.worldCamera.WorldToScreenPoint(worldPos);
			} break;

			case RenderMode.WorldSpace: {
				if(Camera.main != null) {
					Vector3 worldPos = m_rectTransform.TransformPoint(m_rectTransform.rect.position.x, m_rectTransform.rect.position.y, 0f);
					viewportPos = Camera.main.WorldToScreenPoint(worldPos);
				}
			} break;
		}

		// Apply to camera viewport!
		m_cam.pixelRect = new Rect(viewportPos.x, viewportPos.y, viewportSize.x, viewportSize.y);
	}
}
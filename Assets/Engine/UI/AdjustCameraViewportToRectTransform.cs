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
public class AdjustCameraViewportToRectTransform : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Camera m_camera = null;
	public Camera targetCamera {
		get { return m_camera; }
		set { m_camera = value; }
	}

	[SerializeField] private RectTransform m_rectTransform = null;
	public RectTransform targetRectTransform {
		get { return m_rectTransform; }
		set { 
			m_rectTransform = value; 
			m_transformParentCanvas = null;	// Force recapturing the parent canvas
		}
	}

	// Internal
	private Canvas m_transformParentCanvas = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Make sure we have required references
		if(!isActiveAndEnabled) return;
		if(m_camera == null) return;
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
		m_camera.pixelRect = new Rect(viewportPos.x, viewportPos.y, viewportSize.x, viewportSize.y);
	}

	private void OnDrawGizmos() {
		if(m_rectTransform == null) return;

		Vector3[] corners = new Vector3[4];	// left-bot, left-top, right-top, right-bot
		m_rectTransform.GetWorldCorners(corners);

		Vector3 min = corners[0];
		Vector3 max = corners[0];
		for(int i = 1; i < 4; i++) {
			min.x = Mathf.Min(min.x, corners[i].x);
			min.y = Mathf.Min(min.y, corners[i].y);
			min.z = Mathf.Min(min.z, corners[i].z);

			max.x = Mathf.Max(max.x, corners[i].x);
			max.y = Mathf.Max(max.y, corners[i].y);
			max.z = Mathf.Max(max.z, corners[i].z);
		}

		// This only works with non-rotated rect transforms
		// It's ok, since a viewport should always be aligned to the UI
		Vector3 size = max - min;
		Vector3 center = min + (size * 0.5f);
		Gizmos.color = Colors.WithAlpha(Colors.purple, 0.5f);
		Gizmos.DrawCube(center, size);
	}
}
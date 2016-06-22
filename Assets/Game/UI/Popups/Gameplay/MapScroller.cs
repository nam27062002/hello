﻿// PopupPauseMapScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/06/2016.
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
/// 
/// </summary>
public class MapScroller : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private ScrollRect m_scrollRect = null;
	[SerializeField] [Range(0, 1f)] private float m_zoomSpeed = 0.5f;
	[SerializeField] private float m_minZoom = 20f;

	// Internal references
	private Camera m_camera = null;
	private LevelMapData m_levelMapData = null;

	// Aux vars
	private Vector2 m_cameraHalfSize = Vector2.one;
	private Vector2 m_contentSize = Vector2.one;
	private bool m_popupAnimating = false;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_scrollRect != null, "Required field not initialized!");

		// Get map camera and data from the scene
		// [AOC] This is super-expensive, but we only do it once per level so we should be able to afford it
		GameObject mapDataObj = GameObject.FindGameObjectWithTag("MapCamera");
		Debug.Assert(mapDataObj != null, "No object with the tag \"MapCamera\" could be found in the current scene");

		m_camera = mapDataObj.GetComponent<Camera>();
		Debug.Assert(m_camera != null, "The object tagged \"MapCamera\" doesn't have a Camera component");

		m_levelMapData = mapDataObj.GetComponent<LevelMapData>();
		Debug.Assert(m_levelMapData != null, "The object tagged \"MapCamera\" doesn't have a LevelMapData component");
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure camera object is enabled
		EnableCamera(true);

		// Setup camera
		// Fit camera viewport to the scrollrect viewport
		AdjustCameraViewportToRectTransform adjuster = m_camera.ForceGetComponent<AdjustCameraViewportToRectTransform>();
		adjuster.targetCamera = m_camera;
		adjuster.targetRectTransform = m_scrollRect.viewport;

		// Initialize camera at the same position as the game camera (pointing at the dragon)
		Vector3 initialPos = Camera.main.transform.position;	// [AOC] Not safe, make sure the main camera exists and it's the game camera
		initialPos.z = m_camera.transform.position.z;			// Keep Z
		m_camera.transform.position = initialPos;

		// Refresh sizes
		RefreshCameraSize();

		// Move camera to current dragon's position
		// Set initial scroll position based on camera's current position
		ScrollToWorldPos(m_camera.transform.position);

		// Detect scroll events
		m_scrollRect.onValueChanged.AddListener(OnScrollChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Disable camera object
		EnableCamera(false);

		// Stop detecting scroll events
		m_scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Detect zoom
		if(m_zoomSpeed > 0f) {
			// Aux vars
			bool zoomChanged = false;

			// In editor, use mouse wheel
			#if UNITY_EDITOR
			// If mouse wheel has been scrolled
			if(Input.mouseScrollDelta.sqrMagnitude > Mathf.Epsilon) {
				// Change orthographic size based on mouse wheel
				m_camera.orthographicSize += Input.mouseScrollDelta.y * m_zoomSpeed;

				// Mark dirty
				zoomChanged = true;
			}

			// In device, use pinch gesture
			// From https://unity3d.com/learn/tutorials/topics/mobile-touch/pinch-zoom
			#else
			// If there are two touches on the device...
			if(Input.touchCount == 2) {
				// Store both touches.
				Touch touchZero = Input.GetTouch(0);
				Touch touchOne = Input.GetTouch(1);

				// Find the position in the previous frame of each touch.
				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

				// Find the difference in the distances between each frame.
				float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

				// ... change the orthographic size based on the change in distance between the touches.
				m_camera.orthographicSize += deltaMagnitudeDiff * m_zoomSpeed;
				
				// Mark dirty
				zoomChanged = true;
			}
			#endif

			// Perform some common stuff if zoom was changed
			if(zoomChanged) {
				// Make sure the orthographic size is within limits
				m_camera.orthographicSize = Mathf.Clamp(m_camera.orthographicSize, m_minZoom, m_levelMapData.mapCameraBounds.height/2f);	// orthographicSize is half the viewport's height!

				// Refresh scroll rect's sizes to match new camera viewport
				RefreshCameraSize();

				// Update camera position so it doesn't go out of bounds
				ScrollToWorldPos(m_camera.transform.position);
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update the camera position in the world to match the position of the scroll rect content.
	/// </summary>
	private void UpdateCameraPosition() {
		// Content matches level bounds, scrollrect viewport matches camera viewport
		// Scroll position is the top-left corner of the content rectangle, take in consideration camera size to make the maths
		m_camera.transform.SetPosX(
			Mathf.Lerp(
				m_levelMapData.mapCameraBounds.xMin + m_cameraHalfSize.x, 
				m_levelMapData.mapCameraBounds.xMax - m_cameraHalfSize.x, 
				m_scrollRect.horizontalNormalizedPosition
			)
		);
		m_camera.transform.SetPosY(
			Mathf.Lerp(
				m_levelMapData.mapCameraBounds.yMin + m_cameraHalfSize.y, 
				m_levelMapData.mapCameraBounds.yMax - m_cameraHalfSize.y, 
				m_scrollRect.verticalNormalizedPosition
			)
		);
	}

	/// <summary>
	/// Update camera size and scroll components size.
	/// </summary>
	private void RefreshCameraSize() {
		// Setup scroll content to match map's boundaries
		// [AOC] The idea is to match the scrollrect viewport to the camera's viewport and the scrollrect content to the camera limits
		// 		 Constant values are camera's orthoSize, aspectRatio, limits and scrollrect's viewport size
		//		 The only value to change is the scrollrect's content size, so we can easily compute the relation
		Vector2 cameraLimits = m_levelMapData.mapCameraBounds.size;
		Vector2 viewportSize = m_scrollRect.viewport.rect.size;
		Vector2 cameraSize = new Vector2(m_camera.orthographicSize * m_camera.aspect * 2f, m_camera.orthographicSize * 2f);	// [AOC] orthographicSize gives us half the height of the camera viewport in world coords
		m_cameraHalfSize = cameraSize/2f;
		m_contentSize = new Vector2(viewportSize.x/cameraSize.x * cameraLimits.x, viewportSize.y/cameraSize.y * cameraLimits.y);
		m_scrollRect.content.sizeDelta = m_contentSize;
	}

	/// <summary>
	/// Move map to a given world position.
	/// No animation, snapped to limits.
	/// </summary>
	/// <param name="_pos">The position to scroll to (in world coords).</param>
	private void ScrollToWorldPos(Vector3 _pos) {
		// Ideally the ScrollRect will already snap to edges
		// Scroll position is the top-left corner of the content rectangle, take in consideration camera size to make the maths
		m_scrollRect.horizontalNormalizedPosition = Mathf.InverseLerp(
			m_levelMapData.mapCameraBounds.xMin + m_cameraHalfSize.x, 
			m_levelMapData.mapCameraBounds.xMax - m_cameraHalfSize.x, 
			_pos.x
		);
		m_scrollRect.verticalNormalizedPosition = Mathf.InverseLerp(
			m_levelMapData.mapCameraBounds.yMin + m_cameraHalfSize.y, 
			m_levelMapData.mapCameraBounds.yMax - m_cameraHalfSize.y, 
			_pos.y
		);

		// Update camera position to match scroll rect's
		UpdateCameraPosition();
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Perform all the necessary actions to enable/disable map camera rendering.
	/// </summary>
	/// <param name="_enable">Wether to enable or disable the map camera.</param>
	private void EnableCamera(bool _enable) {
		// Obviously camera is required
		if(m_camera == null) return;

		// Override while popup is opening
		_enable = _enable && !m_popupAnimating;

		// Apply to camera component
		m_camera.enabled = _enable;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The ScrollRect content's position has changed.
	/// </summary>
	/// <param name="_scrollPos">The new position.</param>
	private void OnScrollChanged(Vector2 _scrollPos) {
		// Update camera!
		UpdateCameraPosition();
	}

	/// <summary>
	/// The popup's open animation is about to start.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Disable camera
		m_popupAnimating = true;
		EnableCamera(false);
	}

	/// <summary>
	/// The popup's open animation has finished.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Enable camera
		m_popupAnimating = false;
		EnableCamera(this.isActiveAndEnabled);

		// Subscribe to other popups opening
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// The popup's close animation is about to start.
	/// </summary>
	public void OnClosePreAnimation() {
		// Disable camera
		EnableCamera(false);

		// Subscribe to other popups opening
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// A popup has been opened.
	/// </summary>
	/// <param name="_popup">The popup.</param>
	public void OnPopupOpened(PopupController _popup) {
		// Disable camera
		EnableCamera(false);
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup.</param>
	public void OnPopupClosed(PopupController _popup) {
		// Re-enable camera (if this component is enabled)
		EnableCamera(this.isActiveAndEnabled);
	}
}
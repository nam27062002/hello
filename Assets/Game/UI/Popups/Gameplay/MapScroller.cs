// PopupPauseMapScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
	[Space]
	[InfoBox("Zoom units represent how many world units fit in the map camera's viewport (vertically)")]
	[SerializeField] private float m_initialZoom = 100f;
	[SerializeField] private Range m_zoomRange = new Range(40f, 400f);
	[SerializeField] [Range(0, 1f)] private float m_zoomSpeed = 0.5f;

	// Internal references
	private Camera m_camera = null;
	private LevelData m_levelData = null;

	// Aux vars
	private Vector2 m_cameraHalfSize = Vector2.one;
	private Vector2 m_contentSize = Vector2.one;
	private bool m_popupAnimating = false;
	private int m_popupCount = 0;

	// Animations
	private Tweener m_zoomTween = null;
	private Tweener m_scrollTween = null;

	// Public properties
	// Absolute zoom value
	public float zoom {
		get {
			if(m_camera != null) {
				return m_camera.orthographicSize * 2f;	// orthographicSize is half the viewport's height!
			} else {
				return m_initialZoom; 
			}
		}
		set { SetZoom(value); }
	}

	// Zoom percentage relative to initial zoom
	public float zoomFactor {
		get { return m_initialZoom/zoom; }
		set { SetZoom(1f/value * m_initialZoom); }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_scrollRect != null, "Required field not initialized!");

		// Get current level's data and map camera
		m_levelData = LevelManager.currentLevelData;
		m_camera = InstanceManager.mapCamera.camera;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Nothing to do if we don't have all the required elements
		if(m_camera == null) return;
		if(m_levelData == null) return;

		// Make sure camera object is enabled
		EnableCamera(true);

		// Setup camera
		// Fit camera viewport to the scrollrect viewport
		AdjustCameraViewportToRectTransform adjuster = m_camera.ForceGetComponent<AdjustCameraViewportToRectTransform>();
		adjuster.targetCamera = m_camera;
		adjuster.targetRectTransform = m_scrollRect.viewport;

		// Set initial zoom or keep zoom level between openings?
		if(Prefs.GetBoolPlayer(DebugSettings.MAP_ZOOM_RESET, true)) {
			SetZoom(m_initialZoom); 
		}

		// Move camera to current dragon's position or keep scroll position between openings?
		if(Prefs.GetBoolPlayer(DebugSettings.MAP_POSITION_RESET, true)) {
			ScrollToPlayer();
		}

		// Detect scroll events
		m_scrollRect.onValueChanged.AddListener(OnScrollChanged);

		// Subscribe to external events
		Messenger.AddListener<float>(GameEvents.UI_MAP_CENTER_TO_DRAGON, OnCenterToDragon);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Set initial zoom
		SetZoom(m_initialZoom);

		// Refresh sizes
		RefreshScrollSize();

		// Move camera to current dragon's position
		ScrollToPlayer();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Nothing to do if we don't have all the required elements
		if(m_camera == null) return;
		if(m_levelData == null) return;

		// Disable camera object
		EnableCamera(false);

		// Stop detecting scroll events
		m_scrollRect.onValueChanged.RemoveListener(OnScrollChanged);

		// Unsubscribe from external events
		Messenger.RemoveListener<float>(GameEvents.UI_MAP_CENTER_TO_DRAGON, OnCenterToDragon);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Don't scroll while zooming (zooming logic already updates the position)
		bool zoomChanged = UpdateZoom();
		if(!zoomChanged) {
			UpdateCameraPosition();
		}
	}

	/// <summary>
	/// Detect and apply zoom.
	/// </summary>
	private bool UpdateZoom() {
		// Nothing to do if we don't have all the required elements
		if(m_camera == null) return false;
		if(m_levelData == null) return false;

		// Check for debug override
		float zoomSpeed = Prefs.GetFloatPlayer(DebugSettings.MAP_ZOOM_SPEED, m_zoomSpeed);

		// Detect zoom
		bool zoomChanged = false;
		if(zoomSpeed > 0f) {
			// Aux vars
			float newZoom = zoom;

			// In editor, use mouse wheel
			#if UNITY_EDITOR
			// If mouse wheel has been scrolled
			if(Input.mouseScrollDelta.sqrMagnitude > Mathf.Epsilon) {
				// Change orthographic size based on mouse wheel
				newZoom += Input.mouseScrollDelta.y * zoomSpeed;

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
				//Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				//Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
				Vector2 touchZeroPrevPos = touchZero.position - FixTouchDelta(touchZero);
				Vector2 touchOnePrevPos = touchOne.position - FixTouchDelta(touchOne);

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

				// Find the difference in the distances between each frame.
				float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

				// ... change the orthographic size based on the change in distance between the touches.
				newZoom += deltaMagnitudeDiff * zoomSpeed;
				
				// Mark dirty
				zoomChanged = true;
			}
			#endif

			// Disable scrolling while zooming to prevent weird behaviours
			m_scrollRect.horizontal = !zoomChanged;
			m_scrollRect.vertical = !zoomChanged;

			// Has zoom changed?
			if(zoomChanged) {
				// Disable scrolling while zooming to prevent weird behaviours
				m_scrollRect.StopMovement();

				// Apply the zoom change!
				SetZoom(newZoom);
			}
		}

		return zoomChanged;
	}

	/// <summary>
	/// Compute the touch delta to be screen-independent.
	/// http://answers.unity3d.com/questions/209030/android-touch-variation-correction.html
	/// </summary>
	/// <returns>The corrected touch delta.</returns>
	/// <param name="_touch">Touch.</param>
	public Vector2 FixTouchDelta(Touch _touch) {
		// From Unity's doc:
		// The absolute position of the touch is recorded periodically and available 
		// in the position property. The deltaPosition value is a Vector2 that represents 
		// the difference between the touch position recorded on the most recent update and 
		// that recorded on the previous update. The deltaTime value gives the time that 
		// elapsed between the previous and current updates; you can calculate the touch's 
		// speed of motion by dividing deltaPosition.magnitude by deltaTime.
		float dt = Time.unscaledDeltaTime / _touch.deltaTime;
		if(float.IsNaN(dt) || float.IsInfinity(dt)) {
			dt = 1.0f;
		}
		return _touch.deltaPosition * dt;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// [AOC] Instantiated map prefab should be destroyed with the scene, so don't do anything

		// Destroy tweens
		if(m_scrollTween != null) {
			m_scrollTween.Kill(false);
			m_scrollTween = null;
		}

		if(m_zoomTween != null) {
			m_zoomTween.Kill(false);
			m_zoomTween = null;
		}
	}

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Scroll camera to player's position.
	/// </summary>
	/// <param name="_speed">Optional animation speed (units/sec). Use 0 for none.</param>
	public void ScrollToPlayer(float _speed = 0f) {
		// Move camera to the same position as the game camera (which is pointing at the dragon)
		Vector3 initialPos = InstanceManager.gameSceneControllerBase.mainCamera.transform.position;
		initialPos.z = m_camera.transform.position.z;			// Keep Z

		// Limit bounds and set camera position
		ScrollToWorldPos(initialPos, _speed);
	}

	/// <summary>
	/// Move map to a given world position.
	/// No animation, snapped to limits.
	/// </summary>
	/// <param name="_pos">The position to scroll to (in world coords).</param>
	/// <param name="_speed">Optional animation speed (units/sec). Use 0 for none.</param>
	public void ScrollToWorldPos(Vector3 _pos, float _speed = 0f) {
		// Make it safe
		if(m_levelData == null) return;

		// Ideally the ScrollRect will already snap to edges
		// Scroll position is the top-left corner of the content relative to the bottom-left corner of the viewport rectangle, take in consideration camera size to make the maths
		Vector2 targetNormalizedPos = new Vector2(
			// Horizontal
			Mathf.InverseLerp(
				m_levelData.bounds.xMin + m_cameraHalfSize.x, 
				m_levelData.bounds.xMax - m_cameraHalfSize.x, 
				_pos.x
			),

			// Vertical
			Mathf.InverseLerp(
				m_levelData.bounds.yMin + m_cameraHalfSize.y, 
				m_levelData.bounds.yMax - m_cameraHalfSize.y, 
				_pos.y
			)
		);

		// Stop any running tween
		if(m_scrollTween != null) m_scrollTween.Pause();

		// Animate?
		if(_speed > 0f) {
			// If tween is not yet created, do it now
			// Otherwise change start and end values and restart
			if(m_scrollTween == null) {
				m_scrollTween = m_scrollRect.DONormalizedPos(targetNormalizedPos, _speed)
					.SetAutoKill(false)
					.SetUpdate(UpdateType.Normal, true)	// Ignore timescale (game is paused in map popup)
					.SetEase(Ease.OutCubic)
					.SetSpeedBased(true)				// Speed based to look good with any distance
					.OnUpdate(
						() => {
							UpdateCameraPosition(); 	// Make camera follow
						}
					);
			} else {
				m_scrollTween.ChangeValues(m_scrollRect.normalizedPosition, targetNormalizedPos, _speed);
			}

			// Launch animation!
			m_scrollTween.Restart();
		} else {
			// Do it instantly!
			m_scrollRect.normalizedPosition = targetNormalizedPos;	// Apply target position
			UpdateCameraPosition();	// Update camera position to match scroll rect's
		}
	}

	/// <summary>
	/// Set the target amount of zoom.
	/// Will be clamped based on zoomRange and map limits.
	/// Will be ignored if required vars are not initialized.
	/// </summary>
	/// <param name="_zoom">New zoom level. World units to fit in the camera's vertical viewport.</param>
	/// <param name="_speed">Optional animation speed (units/sec). Use 0 for none.</param>
	public void SetZoom(float _zoom, float _speed = 0f) {
		// Check required params
		if(m_camera == null) return;
		if(m_levelData == null) return;

		// Make sure new zoom level is within limits
		float newOrthoSize = Mathf.Clamp(
			_zoom,
			m_zoomRange.min,
			Mathf.Min(m_zoomRange.max, m_levelData.bounds.height)	// Never bigger than level's boundaries!
		);
		newOrthoSize /= 2f;	// orthographicSize is half the viewport's height!

		// Stop any running tween
		if(m_zoomTween != null) m_zoomTween.Pause();

		// Animate?
		if(_speed > 0f) {
			// If tween is not yet created, do it now
			// Otherwise change start and end values and restart
			if(m_zoomTween == null) {
				m_zoomTween = m_camera.DOOrthoSize(newOrthoSize, _speed)
					.SetAutoKill(false)
					.SetUpdate(UpdateType.Normal, true)	// Ignore timescale (game is paused in map popup)
					.SetEase(Ease.OutCubic)
					.SetSpeedBased(true)				// Speed based to look good with any distance
					.OnUpdate(
						() => {
							RefreshScrollSize();							// Refresh scroll rect's sizes to match new camera viewport
							ScrollToWorldPos(m_camera.transform.position);	// Update camera position so it doesn't go out of bounds
							Messenger.Broadcast<float>(GameEvents.UI_MAP_ZOOM_CHANGED, zoomFactor);	// Notify game! (map markers)
						}
					);
			} else {
				m_zoomTween.ChangeValues(m_camera.orthographicSize, newOrthoSize, _speed);
			}

			// Launch animation!
			m_zoomTween.Restart();
		} else {
			// Do it instantly!
			m_camera.orthographicSize = newOrthoSize;		// Apply new zoom level
			RefreshScrollSize();							// Refresh scroll rect's sizes to match new camera viewport
			ScrollToWorldPos(m_camera.transform.position);	// Update camera position so it doesn't go out of bounds
			Messenger.Broadcast<float>(GameEvents.UI_MAP_ZOOM_CHANGED, zoomFactor);	// Notify game! (map markers)
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update the camera position in the world to match the position of the scroll rect content.
	/// </summary>
	private void UpdateCameraPosition() {
		// Make it safe
		if(m_camera == null) return;
		if(m_levelData == null) return;

		// Content matches level bounds, scrollrect viewport matches camera viewport
		// Scroll position is the top-left corner of the content rectangle, take in consideration camera size to make the maths
		m_camera.transform.SetPosX(
			Mathf.Lerp(
				m_levelData.bounds.xMin + m_cameraHalfSize.x, 
				m_levelData.bounds.xMax - m_cameraHalfSize.x, 
				m_scrollRect.horizontalNormalizedPosition
			)
		);
		m_camera.transform.SetPosY(
			Mathf.Lerp(
				m_levelData.bounds.yMin + m_cameraHalfSize.y, 
				m_levelData.bounds.yMax - m_cameraHalfSize.y, 
				m_scrollRect.verticalNormalizedPosition
			)
		);
	}

	/// <summary>
	/// Update scroll components size based on new camera size.
	/// </summary>
	private void RefreshScrollSize() {
		// Make it safe
		if(m_camera == null) return;
		if(m_levelData == null) return;

		// Setup scroll content to match map's boundaries
		// [AOC] The idea is to match the scrollrect viewport to the camera's viewport and the scrollrect content to the camera limits
		// 		 Constant values are camera's orthoSize, aspectRatio, limits and scrollrect's viewport size
		//		 The only value to change is the scrollrect's content size, so we can easily compute the relation
		Vector2 cameraLimits = m_levelData.bounds.size;
		Vector2 viewportSize = m_scrollRect.viewport.rect.size;
		Vector2 cameraSize = new Vector2(m_camera.orthographicSize * m_camera.aspect * 2f, m_camera.orthographicSize * 2f);	// [AOC] orthographicSize gives us half the height of the camera viewport in world coords
		m_cameraHalfSize = cameraSize/2f;
		m_contentSize = new Vector2(viewportSize.x/cameraSize.x * cameraLimits.x, viewportSize.y/cameraSize.y * cameraLimits.y);
		m_scrollRect.content.sizeDelta = m_contentSize;
	}

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

		// Do the same with scroll rect
		m_scrollRect.viewport.gameObject.SetActive(_enable);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The ScrollRect content's position has changed.
	/// </summary>
	/// <param name="_scrollPos">The new position.</param>
	private void OnScrollChanged(Vector2 _scrollPos) {
		// Interrupt any tween!
		// [AOC] Can't do it, OnScrollChanged is called when the tween changes the scroll pos :(
		//		 We could do a specialization of Unity's ScrollRect component with exposed events for the BeginDrag, Drag and EndDrag events, but it's not worth for now
		//		 Another alternative could be to just watch for input touches (anywhere in the screen) and stop the tween there, but again it's not worth the extra work
		//if(m_scrollTween != null) m_scrollTween.Pause();
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
		m_popupCount = 0;
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
		// Increase stack count
		m_popupCount++;

		// Disable camera
		EnableCamera(false);
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup.</param>
	public void OnPopupClosed(PopupController _popup) {
		// Decrease stack count
		m_popupCount--;

		// If all popups have been closed, re-enable camera (if this component is enabled)
		if(m_popupCount == 0) {
			EnableCamera(this.isActiveAndEnabled);
		}
	}

	/// <summary>
	/// Test button to zoom in.
	/// </summary>
	/// <param name="_speed">Zoom speed.</param>
	public void OnZoomIn(float _speed) {
		SetZoom(m_zoomRange.min, _speed);
	}

	/// <summary>
	/// Test button to zoom out.
	/// </summary>
	/// <param name="_speed">Zoom speed.</param>
	public void OnZoomOut(float _speed) {
		SetZoom(m_zoomRange.max, _speed);
	}

	/// <summary>
	/// A game component has requested to scroll to the player.
	/// </summary>
	/// <param name="_speed">Scroll speed.</param>
	private void OnCenterToDragon(float _speed) {
		// Just do it!
		ScrollToPlayer(_speed);
	}
}
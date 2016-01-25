// GameCameraController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controller for the camera in the game scene.
/// In this particular game, the camera will always be looking at Z-0 and never rotate, 
/// making everything much easier.
/// </summary>
public class GameCameraController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[Separator("Movement")]
	[SerializeField] [Range(0, 1)] [Tooltip("The delay towards the dragon position. [0..1] -> [DragonPos..CurrentPos] -> [Hard..Smooth].")] 
	private float m_movementSmoothing = 0.85f;
	[SerializeField] [Range(0, 1)] [Tooltip("The delay when adapting the forward offset to the dragon's direction. [0..1] -> [DragonDir..CurrentDir]. -> [Hard..Smooth]")]
	private float m_forwardSmoothing = 0.95f;
	[SerializeField] [Tooltip("Extra distance to look ahead in front of the dragon")] 
	private float m_forwardOffsetNormal = 3f;
	[SerializeField] [Tooltip("Extra distance to look ahead in front of the dragon on Fury mode")] 
	private float m_forwardOffsetFury = 5f;
	[SerializeField] [Tooltip("Horizontal scroll limits in world coords")]
	private Range m_limitX = new Range(-100, 100);

	[Separator("Zoom")]
	[SerializeField] [Range(0, 1)] [Tooltip("Initial zoom value")]
	private float m_defaultZoom = 0.5f;
	[Tooltip("Zoom factor, distance from Z-0 in world units. Change this based on dragon type.")]
	public Range m_zoomRange = new Range(500f, 2000f);
	[InfoBox("All zoom related values are in relative terms [0..1] to Zoom Range")]

	[Separator("Shaking")]
	[SerializeField] private Vector3 m_shakeDefaultAmount = new Vector3(0.5f, 0.5f, 0f);
	[SerializeField] private float m_shakeDefaultDuration = 0.15f;
	[SerializeField] private bool m_shakeDecayOverTime = true;

	[Separator("Entity management")]
	[SerializeField] private float m_activationDistance = 10f;
	[SerializeField] private float m_activationRange = 5f;
	[SerializeField] private float m_deactivationDistance = 20f;


	// References
	private DragonMotion m_dragonMotion = null;
	private Transform m_interest = null;
	private bool m_furyOn;

	// Positioning
	private Vector3 m_targetPos = Vector3.zero;
	private Vector3 m_forward = Vector3.right;
	private float m_forwardOffset;

	// Zoom
	private Interpolator m_zInterpolator = new Interpolator();

	// Shake
	private Vector3 m_shakeAmount = Vector3.one;
	private float m_shakeDuration = 0f;
	private float m_shakeTimer = 0f;

	// Aux vars for in-game tuning
	private Range m_zoomRangeStart;
	private float m_nearStart;
	private float m_farStart;

	// Camera bounds
	private Bounds m_frustum = new Bounds();
	private Bounds m_activationMin = new Bounds();
	private Bounds m_activationMax = new Bounds();
	private Bounds m_deactivation = new Bounds();

	private Transform m_transform;
	private bool m_update = false;
	private float m_accumulatedTime = 0;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Current zoom level [0..1]
	public float zoom {
		get { return m_zoomRange.InverseLerp(-transform.position.z); }
	}

	// Default zoom level
	public float defaultZoom {
		get { return m_defaultZoom; }
	}

	public void ZoomRangeOffset(float _value) {
		m_zoomRange = m_zoomRangeStart + _value;
		Camera.main.nearClipPlane = Mathf.Max(1, m_nearStart + _value);
		Camera.main.farClipPlane = Mathf.Max(60, m_farStart + _value);
	}

	// Internal
	private Vector3 playerPos {
		get { return InstanceManager.player.transform.position + Vector3.up * 2f; }
	}



	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_transform = transform;
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Acquire external references
		m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();

		// Reset camera target
		m_interest = null;
		m_targetPos = playerPos;
		m_forwardOffset = m_forwardOffsetNormal;
		m_furyOn = false;

		// Initialize zoom interpolator
		m_zInterpolator.Start(m_defaultZoom, m_defaultZoom, 0f);

		m_zoomRangeStart = m_zoomRange;
		m_nearStart = Camera.main.nearClipPlane;
		m_farStart = Camera.main.farClipPlane;

		ZoomRangeOffset(InstanceManager.player.data.def.cameraZoomOffset);

		// Register to Fury events
		//Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);
		Messenger.AddListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void LateUpdate() 
	{
		m_accumulatedTime += Time.deltaTime;
		Vector3 newPos = m_transform.position;

		// it depends on previous fixed updates
		if ( m_update )
		{
			Vector3 dragonDirection = m_dragonMotion.GetVelocity().normalized;

			// Compute new target position
			// Is there a danger nearby?
			if(m_interest != null) {
				// Yes!! Look between the danger and the dragon
				// [AOC] TODO!! Smooth factor might need to be adapted in this particular case
				m_targetPos = Vector3.Lerp(playerPos, m_interest.position, 0.5f);
			} else {
				// No!! Just look towards the dragon
				if (dragonDirection.sqrMagnitude > 0.1f * 0.1f) {
					m_targetPos = m_dragonMotion.head.position;
				} else {
					m_targetPos = playerPos;
				}
			}

			// Update forward direction and apply forward offset to look a bit ahead in the direction the dragon is moving
			m_forward = Vector3.Lerp(dragonDirection, m_forward, m_forwardSmoothing);
			m_targetPos = m_targetPos + m_forward * m_forwardOffset;

			// Clamp X to defined limits
			m_targetPos.x = Mathf.Clamp(m_targetPos.x, m_limitX.min, m_limitX.max);

			// Compute Z, defined by the zoom factor
			m_targetPos.z = -m_zoomRange.Lerp(m_zInterpolator.GetExponential());	// Reverse-signed

			// Apply movement smoothing
			newPos = Vector3.Lerp(m_targetPos, transform.position, m_movementSmoothing);
			newPos.z = m_targetPos.z;	// Except Z, we don't want to smooth zoom - it's already smoothed by the interpolator, using custom speed/duration

			newPos = UpdateByShake(newPos);

			m_update = false;
			m_accumulatedTime = 0;
		}

		// DONE! Apply new position
		transform.position = newPos;

		UpdateFrustumBounds();
	}

	private Vector3 UpdateByShake( Vector3 position)
	{
		// Apply shaking - after smoothing, we don't want shaking to be affected by it
		if (m_shakeTimer > 0f){
			// Update timer
			m_shakeTimer -= m_accumulatedTime;
			
			// Compute a random shaking optionally decaying over time
			if (m_shakeTimer > 0) {
				Vector3 decayedShakeAmt = m_shakeAmount;
				if (m_shakeDecayOverTime) {
					decayedShakeAmt.x *= Mathf.InverseLerp(0, m_shakeDuration, m_shakeTimer);
					decayedShakeAmt.y *= Mathf.InverseLerp(0, m_shakeDuration, m_shakeTimer);
					decayedShakeAmt.z *= Mathf.InverseLerp(0, m_shakeDuration, m_shakeTimer);
				}
				
				position.x += Random.Range(-decayedShakeAmt.x, decayedShakeAmt.x);
				position.y += Random.Range(-decayedShakeAmt.y, decayedShakeAmt.y);
				position.z += Random.Range(-decayedShakeAmt.z, decayedShakeAmt.z);
			}
		}

		return position;
	}

	private void FixedUpdate()
	{
		m_update = true;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// Bounds															//
	//------------------------------------------------------------------//

	public bool IsInsideActivationMinArea(Vector3 _point) {
		_point.z = 0;
		return m_activationMin.Contains(_point);
	}

	public bool IsInsideActivationMinArea(Bounds _bounds) {
		Vector3 center = _bounds.center;
		center.z = 0;
		_bounds.center = center;
		return m_activationMin.Intersects(_bounds);
	}

	public bool IsInsideActivationMaxArea(Vector3 _point) {
		_point.z = 0;
		return m_activationMax.Contains(_point);
	}

	public bool IsInsideActivationMaxArea(Bounds _bounds) {
		Vector3 center = _bounds.center;
		center.z = 0;
		_bounds.center = center;
		return m_activationMax.Intersects(_bounds);
	}

	public bool IsInsideActivationArea(Vector3 _point) {
		_point.z = 0;
		return !m_activationMin.Contains(_point) && m_activationMax.Contains(_point);
	}

	public bool IsInsideActivationArea(Bounds _bounds) {
		Vector3 center = _bounds.center;
		center.z = 0;
		_bounds.center = center;
		return !m_activationMin.Intersects(_bounds) && m_activationMax.Intersects(_bounds);
	}

	public bool IsInsideDeactivationArea(Vector3 _point) {
		_point.z = 0;
		return !m_deactivation.Contains(_point);
	}

	public bool IsInsideDeactivationArea(Bounds _bounds) {
		Vector3 center = _bounds.center;
		center.z = 0;
		_bounds.center = center;
		return !m_deactivation.Intersects(_bounds);
	}

	// update camera bounds for Z = 0, this can change with dinamic zoom in/out animations
	private void UpdateFrustumBounds() {
		float frustumHeight = 2.0f * Mathf.Abs(transform.position.z) * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
		float frustumWidth = frustumHeight * Camera.main.aspect;

		Vector3 center = transform.position;
		center.z = 0;

		m_frustum.center = center;
		m_frustum.size = new Vector3(frustumWidth, frustumHeight, 4f);

		m_activationMin.center = center;
		m_activationMin.size = new Vector3(frustumWidth + m_activationDistance * 2f, frustumHeight + m_activationDistance * 2f, 4f);

		m_activationMax.center = center;
		m_activationMax.size = new Vector3(frustumWidth + (m_activationDistance + m_activationRange) * 2f, frustumHeight + (m_activationDistance + m_activationRange) * 2f, 4f);

		m_deactivation.center = center;
		m_deactivation.size = new Vector3(frustumWidth + m_deactivationDistance * 2f, frustumHeight + m_deactivationDistance * 2f, 4f);
	}

	//------------------------------------------------------------------//
	// Callbacks														//
	//------------------------------------------------------------------//
	private void OnFury(bool _enabled) {
		m_furyOn = _enabled;

		if (m_furyOn) {
			if (m_interest == null) {
				m_forwardOffset = m_forwardOffsetFury;
				Zoom(0.8f, 2f);
			}
		} else {
			if (m_interest == null) {
				m_forwardOffset = m_forwardOffsetNormal;
				Zoom(m_defaultZoom, 2f);
			} else {
				SetEntityOfInterest(m_interest);
			}
		}
	}

	//------------------------------------------------------------------//
	// ZOOM																//
	//------------------------------------------------------------------//
	/// <summary>
	/// Zoom to a specific zoom level.
	/// </summary>
	/// <param name="_zoomLevel">The level to zoom to [0..1].</param>
	/// <param name="_duration">The duration in seconds of the zoom animation.</param>
	public void Zoom(float _zoomLevel, float _duration) {
		// Override any previous zoom anim
		m_zInterpolator.Start(zoom, _zoomLevel, _duration);
	}
	
	/// <summary>
	/// Zoom to a specific zoom level using speed rather than a fixed duration.
	/// </summary>
	/// <param name="_zoomLevel">The level to zoom to [0..1].</param>
	/// <param name="_speed">The speed of the zoom animation in zoom units per second.</param>
	public void ZoomAtSpeed(float _zoomLevel, float _speed) {
		// Compute the actual distance to go
		float dist = _zoomLevel - zoom;
		
		// Compute the time required to go that distance at the given speed
		float duration = Mathf.Abs(dist)/_speed;
		
		// Launch the zoom animation
		Zoom(_zoomLevel, duration);
	}



	//------------------------------------------------------------------//
	// SHAKING															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Trigger a shaking using the default values defined in the inspector.
	/// </summary>
	public void Shake() {
		m_shakeDuration = m_shakeDefaultDuration;
		m_shakeAmount = m_shakeDefaultAmount;

		m_shakeTimer = m_shakeDuration;
	}

	/// <summary>
	/// Trigger a shaking using custom values.
	/// </summary>
	/// <param name="_duration">How long must the shake last.</param>
	/// <param name="_shakeAmount">Intensity of the shaking.</param>
	public void Shake(float _duration, Vector3 _shakeAmount) {
		m_shakeDuration = _duration;
		m_shakeAmount = _shakeAmount;

		m_shakeTimer = m_shakeDuration;
	}



	//------------------------------------------------------------------//
	// Entity of Interest												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Define a given transform as a point of interest.
	/// The camera will react in consequence.
	/// </summary>
	/// <param name="_interest">The dangerous object, set to null to clear it.</param>
	public void SetEntityOfInterest(Transform _interest) {
		m_interest = _interest;
		if (_interest == null) {
			if (m_furyOn) {
				OnFury(true);
			} else {
				Zoom(defaultZoom, 0.25f);	// Danger cleared, go back to normal zoom level
			}
		} else {
			Zoom(1f, 2f);	// Entity of Interest Zoom out
		}
	}



	//------------------------------------------------------------------//
	// Debug															//
	//------------------------------------------------------------------//
	void OnDrawGizmos() {
		if (!Application.isPlaying) {
			UpdateFrustumBounds();
		}
			
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(m_frustum.center, m_frustum.size);

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(m_activationMin.center, m_activationMin.size);
		Gizmos.DrawWireCube(m_activationMax.center, m_activationMax.size);

		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(m_deactivation.center, m_deactivation.size);
	}
}


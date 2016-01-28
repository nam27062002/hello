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
using System.Collections;

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
	private float m_forwardOffsetNormal = 1f;
	[SerializeField] [Tooltip("Extra distance to look ahead in front of the dragon on Fury mode")] 
	private float m_forwardOffsetFury = 3f;
	[SerializeField] [Tooltip("Extra distance to look ahead in front of the dragon on Boost mode")] 
	private float m_forwardOffsetBoost = 2f;
	[SerializeField] [Tooltip("Horizontal scroll limits in world coords")]
	private Range m_limitX = new Range(-100, 100);

	[Separator("Zoom")]
	[SerializeField] [Range(0, 1)] [Tooltip("Initial zoom value")]
	private float m_defaultZoom = 0.5f;
	[Tooltip("Zoom factor, distance from Z-0 in world units. Change this based on dragon type.")]
	public Range m_zoomRange = new Range(500f, 2000f);
	[InfoBox("All zoom related values are in relative terms [0..1] to Zoom Range")]
	private float m_currentZoom;

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
	private bool m_slowMotionOn;
	private bool m_slowMoJustStarted;
	private bool m_boostOn;

	// Positioning
	private Vector3 m_forward = Vector3.right;


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
	IEnumerator Start() {
		while( !InstanceManager.GetSceneController<GameSceneControllerBase>().IsLevelLoaded())
		{
			yield return null;
		}

		// Acquire external references
		m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();

		// Reset camera target
		m_interest = null;
		m_furyOn = false;
		m_slowMotionOn = false;
		m_slowMoJustStarted = false;
		m_boostOn = false;

		m_zoomRangeStart = m_zoomRange;
		m_currentZoom = m_defaultZoom;
		m_nearStart = Camera.main.nearClipPlane;
		m_farStart = Camera.main.farClipPlane;

		ZoomRangeOffset(InstanceManager.player.data.def.cameraZoomOffset);

		// Register to Fury events
		//Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);

		Messenger.AddListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.AddListener<bool>(GameEvents.SLOW_MOTION_TOGGLED, OnSlowMotion);
		Messenger.AddListener<bool>(GameEvents.BOOST_TOGGLED, OnBoost);

		transform.position = playerPos;

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
			Vector3 dragonVelocity = m_dragonMotion.GetVelocity();
			Vector3 dragonDirection = dragonVelocity.normalized;
			m_forward = Vector3.Lerp(dragonDirection, m_forward, m_forwardSmoothing);

			Vector3 targetPos;
			// Compute new target position
			// Is there a danger nearby?
			if(m_interest != null) 
			{
				// Yes!! Look between the danger and the dragon
				// [AOC] TODO!! Smooth factor might need to be adapted in this particular case
				targetPos = Vector3.Lerp(playerPos, m_interest.position, 0.25f);
			} 
			else 
			{
				// No!! Just look towards the dragon
				if (dragonDirection.sqrMagnitude > 0.1f * 0.1f) {
					targetPos = m_dragonMotion.head.position;
				} else {
					targetPos = playerPos;
				}
			}

			// Update forward direction and apply forward offset to look a bit ahead in the direction the dragon is moving
			if (m_furyOn)
			{
				targetPos = targetPos + m_forward * m_forwardOffsetFury;
			}
			else if ( m_boostOn )
			{
				targetPos = targetPos + m_forward * m_forwardOffsetBoost;
			}
			else
			{
				targetPos = targetPos + m_forward * m_forwardOffsetNormal;
			}


			// Clamp X to defined limits
			targetPos.x = Mathf.Clamp(targetPos.x, m_limitX.min, m_limitX.max);


			// Compute Z, defined by the zoom factor
			float targetZoom = 0.5f;
			if ( m_slowMoJustStarted )
			{
				targetZoom = 0.9f;
				m_currentZoom = targetZoom;
				m_slowMoJustStarted = false;
			}
			else
			{
				if ( m_interest != null )
				{
					targetZoom = 1;
				}
				else if ( m_slowMotionOn )
				{
					targetZoom = 0.9f;
				}
				else if ( m_furyOn )
				{
					targetZoom = 0.8f;
				}
				m_currentZoom = Mathf.Lerp( m_currentZoom, targetZoom, m_accumulatedTime * 0.9f);
			}

			targetPos.z = -m_zoomRange.Lerp(m_currentZoom);


			// Apply movement smoothing
			newPos = Vector3.Lerp(targetPos, transform.position, m_movementSmoothing);
			newPos.z = targetPos.z;	// Except Z, we don't want to smooth zoom - it's already smoothed by the interpolator, using custom speed/duration

			newPos = UpdateByShake(newPos);

			// Rotation
			float maxSpeed = m_dragonMotion.GetMaxSpeed();
			Quaternion q = Quaternion.Euler( dragonVelocity.y / maxSpeed * -3f, dragonVelocity.x / maxSpeed * 7.5f, 0);
			transform.rotation = Quaternion.Lerp( transform.rotation, q, 0.9f * m_accumulatedTime);

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
	}

	private void OnSlowMotion( bool _enabled)
	{
		m_slowMotionOn = _enabled;
		m_slowMoJustStarted = _enabled;
	}

	private void OnBoost( bool _enabled)
	{
		m_boostOn = _enabled;
	}


	//------------------------------------------------------------------//
	// ZOOM																//
	//------------------------------------------------------------------//
	/// <summary>
	/// Zoom to a specific zoom level.
	/// </summary>
	/// <param name="_zoomLevel">The level to zoom to [0..1].</param>
	/// <param name="_duration">The duration in seconds of the zoom animation.</param>
	/*
	public void Zoom(float _zoomLevel, float _duration) 
	{
		// Override any previous zoom anim
		m_zInterpolator.Start(zoom, _zoomLevel, _duration);
	}
	*/
	
	/// <summary>
	/// Zoom to a specific zoom level using speed rather than a fixed duration.
	/// </summary>
	/// <param name="_zoomLevel">The level to zoom to [0..1].</param>
	/// <param name="_speed">The speed of the zoom animation in zoom units per second.</param>
	/*
	public void ZoomAtSpeed(float _zoomLevel, float _speed) {
		// Compute the actual distance to go
		float dist = _zoomLevel - zoom;
		
		// Compute the time required to go that distance at the given speed
		float duration = Mathf.Abs(dist)/_speed;
		
		// Launch the zoom animation
		Zoom(_zoomLevel, duration);
	}
	*/


	//------------------------------------------------------------------//
	// SHAKING															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Trigger a shaking using the default values defined in the inspector.
	/// </summary>
	public void Shake() 
	{
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


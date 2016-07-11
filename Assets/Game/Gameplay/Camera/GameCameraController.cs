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
	[SerializeField] [Tooltip("Horizontal scroll limits in world coords")]
	private Range m_limitX = new Range(-100, 100);

	[Separator("Zoom")]
	[SerializeField] [Tooltip("Default Zoom distance")]
	private float m_defaultZoom = 30;
	[SerializeField] [Tooltip("Far Zoom distance")]
	private float m_farZoom = 50f;
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
	private DragonBreathBehaviour m_dragonBreath = null;
	private Transform m_interest = null;
	private float m_interestLerp = 0;
	private Vector3 m_interestPosition = Vector3.zero;
	private bool m_furyOn = false;
	private bool m_slowMotionOn;
	private bool m_slowMoJustStarted;
	private bool m_boostOn;
	private Camera m_camera;

	// Positioning
	private float m_forwardOffset = 0;


	// Shake
	private Vector3 m_shakeAmount = Vector3.one;
	private float m_shakeDuration = 0f;
	private float m_shakeTimer = 0f;

	// Camera bounds
	private Bounds m_frustum = new Bounds();
	private Bounds m_activationMin = new Bounds();
	private Bounds m_activationMax = new Bounds();
	private Bounds m_deactivation = new Bounds();

	private Transform m_transform;

	enum State
	{
		INTRO,
		PLAY
	};
	State m_state = State.INTRO;
	Vector3 m_dampedPosition;
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	// Default zoom level
	public float defaultZoom {
		get 
		{ 
			return m_defaultZoom; 
		}
		set
		{
			m_defaultZoom = value;
		}
	}

	// Far zoom
	public float farZoom {
		get 
		{ 
			return m_farZoom; 
		}
		set
		{
			m_farZoom = value;
		}
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
		m_state = State.INTRO;
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
		m_dragonBreath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();

		// Reset camera target
		m_interest = null;
		m_furyOn = false;
		m_slowMotionOn = false;
		m_slowMoJustStarted = false;
		m_boostOn = false;

		defaultZoom = InstanceManager.player.data.def.GetAsFloat("cameraDefaultZoom");
		farZoom = InstanceManager.player.data.def.GetAsFloat("cameraFarZoom");
		m_currentZoom = m_defaultZoom * 2;

		// Register to Fury events
		//Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);

		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.AddListener<bool>(GameEvents.SLOW_MOTION_TOGGLED, OnSlowMotion);
		Messenger.AddListener<bool>(GameEvents.BOOST_TOGGLED, OnBoost);
		Messenger.AddListener(GameEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);

		GameObject spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + InstanceManager.player.data.def.sku);
		if(spawnPointObj == null) 
		{
			// We couldn't find a spawn point for this specific type, try to find a generic one
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME);
		}
		Vector3 pos = spawnPointObj.transform.position;
		pos.z = -m_currentZoom;
		transform.position = pos;
		m_dampedPosition = pos;

		m_camera = GetComponent<Camera>();

		if ( InstanceManager.GetSceneController<LevelEditor.LevelEditorSceneController>() )
		{
			m_state = State.PLAY;
		}

	}

	private void CountDownEnded()
	{
		m_state = State.PLAY;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void LateUpdate() 
	{
		
		Vector3 newPos = m_transform.position;

		switch( m_state )
		{
			case State.INTRO:
			{	
				m_currentZoom = Mathf.Lerp( m_currentZoom, m_defaultZoom, Time.deltaTime);
				newPos.z = -m_currentZoom;
				m_dampedPosition = newPos;
			}break;
			case State.PLAY:
			{
				// it depends on previous fixed updates
				if ( m_dragonMotion != null)
				{

					Vector3 targetPos;
					// Compute new target position
					// Is there a danger nearby?
					/*if(m_interest != null) 
					{
						m_interestLerp += Time.deltaTime * 0.5f;
						m_interestLerp = Mathf.Min( m_interestLerp, 0.25f);
						m_interestPosition = m_interest.position - m_dragonMotion.cameraLookAt.position;
					} 
					else 
					*/
					{
						m_interestLerp -= Time.deltaTime * 0.5f;
						m_interestLerp = Mathf.Max( m_interestLerp, 0);
					}

					targetPos = Vector3.Lerp(m_dragonMotion.transform.position, m_dragonMotion.transform.position + m_interestPosition, m_interestLerp);


					Vector3 dragonVelocity = m_dragonMotion.velocity;
					Vector3 dragonDirection = dragonVelocity.normalized;

					

					// Update forward direction and apply forward offset to look a bit ahead in the direction the dragon is moving
					if (m_furyOn)
					{
						m_forwardOffset = Mathf.Lerp( m_forwardOffset, (m_dragonBreath.actualLength * 0.5f), Time.deltaTime );
					}
					else
					{
						m_forwardOffset = Mathf.Lerp( m_forwardOffset, 0, Time.deltaTime );
					}
					targetPos = targetPos + dragonDirection * m_forwardOffset;

					// Clamp X to defined limits
					targetPos.x = Mathf.Clamp(targetPos.x, m_limitX.min, m_limitX.max);


					// Compute Z, defined by the zoom factor
					float targetZoom = m_defaultZoom;
					if ( m_slowMoJustStarted )
					{
						targetZoom = m_farZoom;
						m_currentZoom = targetZoom;
						m_slowMoJustStarted = false;
					}
					else
					{
						if ( m_interest != null || m_slowMotionOn || m_furyOn || m_boostOn || (m_dragonMotion.state == DragonMotion.State.InsideWater && !m_dragonMotion.canDive))
							targetZoom = m_farZoom;
						m_currentZoom = Mathf.Lerp( m_currentZoom, targetZoom, Time.deltaTime);
					}
					targetPos.z = -m_currentZoom;

					float dampFactor = 0.9f;
					m_dampedPosition = Damping( m_dampedPosition, targetPos, Time.deltaTime, dampFactor);

					float m = ( targetPos - m_dampedPosition ).magnitude * 0.5f;
					// float multiplierFactor = (1 / ( 1 + Mathf.Pow(2,-(m-2)))) * 1.5f;
					float multiplierFactor = (1 / ( 1 + Mathf.Pow(2f,-((m))))) * 1.375f;
					// float multiplierFactor = (1 / ( 1 + Mathf.Pow(2,-(m-2)))) * 1.375f;

					newPos = m_dampedPosition + (( targetPos - m_dampedPosition ) * multiplierFactor);


					// FACTOR = función sigmoide  = 1 / (1+e^-x)
					// Desplazar y ajustar alturas (1/(1+e^-(x-2))*2
				
				}
			}break;
		}

		newPos = UpdateByShake(newPos);

		// DONE! Apply new position
		transform.position = newPos;

		UpdateFrustumBounds();
	}

	Vector3 Damping( Vector3 src, Vector3 dst, float dt, float factor)
	{
		return ((src * factor) + (dst * dt)) / (factor + dt);
	}

	private Vector3 UpdateByShake( Vector3 position)
	{
		// Apply shaking - after smoothing, we don't want shaking to be affected by it
		if (m_shakeTimer > 0f){
			// Update timer
			m_shakeTimer -= Time.deltaTime;
			
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

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() 
	{
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.RemoveListener<bool>(GameEvents.SLOW_MOTION_TOGGLED, OnSlowMotion);
		Messenger.RemoveListener<bool>(GameEvents.BOOST_TOGGLED, OnBoost);
		Messenger.RemoveListener(GameEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
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

	public bool IsInsideFrustrum( Vector3 _point)
	{
		_point.z = 0;
		return m_frustum.Contains(_point);
	}

	public bool IsInsideFrustrum( Bounds _bounds)
	{
		Vector3 center = _bounds.center;
		center.z = 0;
		_bounds.center = center;
		return m_frustum.Intersects(_bounds);
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
	private void OnFury(bool _enabled, DragonBreathBehaviour.Type _type) {
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


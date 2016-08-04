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
	private TouchControlsDPad	m_touchControls = null;
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
	private FastBounds2D m_frustum = new FastBounds2D();
	private FastBounds2D m_backgroundWorldBounds = new FastBounds2D();
	private FastBounds2D m_activationMin = new FastBounds2D();
	private FastBounds2D m_activationMax = new FastBounds2D();
	private FastBounds2D m_deactivation = new FastBounds2D();

	private Transform m_transform;

	enum State
	{
		INTRO,
		PLAY
	};
	State m_state = State.INTRO;
	Vector3 m_dampedPosition;

	Vector3 m_position;

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
		enabled = !DebugSettings.newCameraSystem;
	}

	/// <summary>
	/// First update.
	/// </summary>
	IEnumerator Start() {
		
		while( !InstanceManager.GetSceneController<GameSceneControllerBase>().IsLevelLoaded())
		{
			yield return null;
		}

		GameObject gameInputObj = GameObject.Find("PF_GameInput");
		if(gameInputObj != null) 
		{
			m_touchControls = gameInputObj.GetComponent<TouchControlsDPad>();
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
		m_position = pos;

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

		if ( DebugSettings.newCameraSystem )
		{
			GetComponent<GameCamera>().enabled = true;
			enabled = false;
		}

		switch( m_state )
		{
			case State.INTRO:
			{	
				m_currentZoom = Mathf.Lerp( m_currentZoom, m_defaultZoom, Time.deltaTime);
				m_position.z = -m_currentZoom;
				m_dampedPosition = m_position;
				m_trackAheadPos = m_position;
			}break;
			case State.PLAY:
			{
				// if ( DebugSettings.newCameraSystem )
				//	NewVersion();
				// else
					OldVersion();
			}break;
		}

		Vector3 newPos = UpdateByShake(m_position);
		// DONE! Apply new position
		transform.position = newPos;

		UpdateFrustumBounds();
	}

	private Vector3 m_trackAheadVector = Vector3.zero;
	private const float         m_maxTrackAheadScaleX = 0.15f;
 	private const float         m_maxTrackAheadScaleY = 0.2f; //JO
	private const float			m_trackBlendRate = 1.0f;
	private const float 		m_maxRotationAngleX = 22.5f; //JO 
    private const float         m_maxRotationAngleY = 20.0f;
	private bool				m_snap = true;
	private float               m_rotateLerpTimer = 0.0f;
	private float 				m_rotateLerpDuration = 1.0f;
	private float               m_camDelayLerpT = 0.0f;
	private Vector3             m_trackAheadPos = Vector3.zero;
	private float 				m_bossInAngleLerp = 0.0f;
	private float 				m_trackAheadScale = 1.0f;

	private void UpdateTrackAheadVector( Vector3 velocity)
   	{
		Vector3 trackingVelocity = velocity;
		if ( trackingVelocity.sqrMagnitude > 1 )
			trackingVelocity.Normalize();
         float dt = Time.deltaTime;
         float trackAheadRangeX = m_frustum.w * m_maxTrackAheadScaleX; // todo: have maxTrackAheadScale account for size of target?
         float trackAheadRangeY = m_frustum.h * m_maxTrackAheadScaleY;
         float trackBlendRate = trackAheadRangeX * m_trackBlendRate;
         Vector3 desiredTrackAhead = trackingVelocity;
         desiredTrackAhead.x *= trackAheadRangeX;
         desiredTrackAhead.y *= trackAheadRangeY;
         if(m_snap)
             m_trackAheadVector = desiredTrackAhead;
         else
			Util.MoveTowardsVector3XYWithDamping(ref m_trackAheadVector, ref desiredTrackAhead,trackBlendRate*dt, 1.0f);
		
     }

	void UpdateCameraDelayLerp()
	{
		// update the camera delay lerp
		m_rotateLerpTimer += Time.deltaTime;
		m_camDelayLerpT = m_rotateLerpTimer / m_rotateLerpDuration; // 0 - 1
		m_camDelayLerpT = Mathf.Clamp01 (m_camDelayLerpT);
	}

	void UpdateZoom()
	{
		m_currentZoom = Mathf.Lerp( m_currentZoom, m_defaultZoom, Time.deltaTime);
		m_position.z = -m_currentZoom;
	}
	 
	void UpdateRotation()
	{
		if((m_dragonMotion != null))
		{
			Vector3 targetTrackAhead = m_trackAheadVector * m_trackAheadScale;
			Vector3 targetTrackPos =  m_dragonMotion.transform.position + targetTrackAhead;
			m_trackAheadPos = Vector3.Lerp(m_trackAheadPos, targetTrackPos, m_camDelayLerpT);
			Vector3 currentPos = m_position;
			currentPos.z = m_trackAheadPos.z;

			Vector3 lookAtPos = Vector3.Lerp(m_trackAheadPos, currentPos, m_bossInAngleLerp);
			m_transform.LookAt(lookAtPos);

            // clamp the rotation at a maximum of 30 degrees either way
            Vector3 rot = m_transform.rotation.eulerAngles;
			if(rot.y > 180.0f)
			{
				rot.y = rot.y - 360.0f;
			}
			if(rot.x > 180.0f)
			{
				rot.x = rot.x - 360.0f;
			}
			rot.y = Mathf.Clamp(rot.y, -m_maxRotationAngleY, m_maxRotationAngleY);
			rot.x = Mathf.Clamp(rot.x, -m_maxRotationAngleX, m_maxRotationAngleX);

            m_transform.rotation = Quaternion.Euler(rot);
		}

	}

	void NewVersion()
	{
		if ( m_dragonMotion != null )
		{
			// have we changed direction in this Update()
			/*
			if(m_touchControls.directionChanged)	
			{
				m_rotateLerpTimer = 0.0f;
			}
			*/
			Vector3 targetPos = m_dragonMotion.transform.position;
			UpdateTrackAheadVector( m_dragonMotion.velocity / m_dragonMotion.absoluteMaxSpeed);
			Vector3 desiredPos = targetPos - m_trackAheadVector;
			UpdateCameraDelayLerp();
			m_position = Vector3.Lerp( m_position, desiredPos, m_camDelayLerpT);

			UpdateRotation();
			UpdateZoom();
			UpdateFrustumBounds();


			m_snap = false;
		}
	}


	void OldVersion()
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

			m_position = m_dampedPosition + (( targetPos - m_dampedPosition ) * multiplierFactor);

			// FACTOR = función sigmoide  = 1 / (1+e^-x)
			// Desplazar y ajustar alturas (1/(1+e^-(x-2))*2
		
		}
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

	public bool IsInsideBackgroundActivationArea(Vector3 _point) {
		return m_backgroundWorldBounds.Contains(_point);
	}

	public bool IsInsideBackgroundActivationArea(Bounds _bounds) {
		return m_backgroundWorldBounds.Intersects(_bounds);
	}

	public bool IsInsideDeactivationArea(Vector3 _point) {
		_point.z = 0;
		return !m_deactivation.Contains(_point);
	}

	public bool IsInsideDeactivationArea(Bounds _bounds) {
		return !m_backgroundWorldBounds.Intersects(_bounds);
	}


	public bool IsInsideBackgroundDeactivationArea(Vector3 _point) {
		return !m_backgroundWorldBounds.Contains(_point);
	}

	public bool IsInsideBackgroundDeactivationArea(Bounds _bounds) {
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
	private void UpdateFrustumBounds() 
	{
		if ( m_camera == null )
			return;

		float z = -m_position.z;

		// Now that we tilt the camera a bit, need to modify how it gets the world bounds 
		Ray[] cameraRays = new Ray[4];
		cameraRays[0] = m_camera.ScreenPointToRay(new Vector3(0.0f, 0.0f, z));
		cameraRays[1] = m_camera.ScreenPointToRay(new Vector3(m_camera.pixelWidth, 0.0f, z));
		cameraRays[2] = m_camera.ScreenPointToRay(new Vector3(m_camera.pixelWidth, m_camera.pixelHeight, z));
		cameraRays[3] = m_camera.ScreenPointToRay(new Vector3(0.0f, m_camera.pixelHeight, z));
		
		// generate two world bounds, one for z=0, one for background spawners
		for(int j=0; j<2; j++)
		{
			bool bg = (j==1);
			
			Plane plane = new Plane(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, bg ? SpawnerManager.BackgroundLayerZ : 0.0f));
			Vector3[] pts = new Vector3[4];
			FastBounds2D bounds = bg ? m_backgroundWorldBounds : m_frustum;
			
			for(int i=0; i<4; i++)
			{
				Vector3? intersect = Util.RayPlaneIntersect(cameraRays[i], plane);
				if(intersect != null)
				{
					pts[i] = (Vector3)intersect;
					if(i == 0)	// initialize bounds with first point and zero size
						bounds.Set(pts[i].x, pts[i].y, 0.0f, 0.0f);
					else
						bounds.Encapsulate(ref pts[i]);
				}
			}
			
			#if DEBUG_DRAW_BOUNDS
			DebugDraw.DrawLine(pts[0], pts[1]);
			DebugDraw.DrawLine(pts[1], pts[2]);
			DebugDraw.DrawLine(pts[2], pts[3]);
			DebugDraw.DrawLine(pts[3], pts[0]);
			#endif
		}


		float expand = 0;

		m_activationMin.Set( m_frustum );
		expand = m_activationDistance;
		m_activationMin.ExpandBy(expand, expand);
		m_activationMin.ExpandBy(-expand, -expand);

		m_activationMax.Set( m_frustum );
		expand = m_activationDistance + m_activationRange;
		m_activationMax.ExpandBy( expand, expand );
		m_activationMax.ExpandBy( -expand, -expand );

		m_deactivation.Set( m_frustum );
		expand = m_deactivationDistance;
		m_deactivation.ExpandBy( expand, expand );
		m_deactivation.ExpandBy( -expand, -expand );
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

		if ( enabled )
		{
			Vector3 center;
			Vector3 size;

			Gizmos.color = Color.yellow;
			m_frustum.GetCentre( out center );
			m_frustum.GetSize(out size);
			Gizmos.DrawWireCube(center, size);

			Gizmos.color = Color.cyan;
			m_activationMin.GetCentre( out center );
			m_activationMin.GetSize(out size);
			Gizmos.DrawWireCube(center, size);
			m_activationMax.GetCentre( out center );
			m_activationMax.GetSize(out size);
			Gizmos.DrawWireCube(center, size);

			Gizmos.color = Color.magenta;
			m_deactivation.GetCentre( out center );
			m_deactivation.GetSize(out size);
			Gizmos.DrawWireCube(center, size);
		}
	}
}


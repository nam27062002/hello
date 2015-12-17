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
	private float m_forwardOffset = 300f;
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
	private Transform m_danger = null;

	// Positioning
	private Vector3 m_targetPos = Vector3.zero;
	private Vector3 m_forward = Vector3.right;

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

	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Acquire external references
		m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();

		// Reset camera target
		m_danger = null;
		m_targetPos = playerPos;

		// Initialize zoom interpolator
		m_zInterpolator.Start(m_defaultZoom, m_defaultZoom, 0f);

		m_zoomRangeStart = m_zoomRange;
		m_nearStart = Camera.main.nearClipPlane;
		m_farStart = Camera.main.farClipPlane;

		ZoomRangeOffset(InstanceManager.player.data.cameraZoomOffset);
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Compute new target position
		// Is there a danger nearby?
		if(m_danger != null) {
			// Yes!! Look between the danger and the dragon
			// [AOC] TODO!! Smooth factor might need to be adapted in this particular case
			m_targetPos = Vector3.Lerp(playerPos, m_danger.position, 0.5f);
		} else {
			// No!! Just look towards the dragon
			m_targetPos = playerPos;
		}

		// Update forward direction and apply forward offset too look a bit ahead in the direction the dragon is moving
		m_forward = Vector3.Lerp(m_dragonMotion.GetDirection(), m_forward, m_forwardSmoothing);
		m_targetPos = m_targetPos + m_forward * m_forwardOffset;

		// Clamp X to defined limits
		m_targetPos.x = Mathf.Clamp(m_targetPos.x, m_limitX.min, m_limitX.max);

		// Compute Z, defined by the zoom factor
		m_targetPos.z = -m_zoomRange.Lerp(m_zInterpolator.GetExponential());	// Reverse-signed

		// Apply movement smoothing
		Vector3 newPos = Vector3.Lerp(m_targetPos, transform.position, m_movementSmoothing);
		newPos.z = m_targetPos.z;	// Except Z, we don't want to smooth zoom - it's already smoothed by the interpolator, using custom speed/duration

		// Apply shaking - after smoothing, we don't want shaking to be affected by it
		if(m_shakeTimer > 0f){
			// Update timer
			m_shakeTimer -= Time.deltaTime;
			
			// Compute a random shaking optionally decaying over time
			if(m_shakeTimer > 0) {
				Vector3 decayedShakeAmt = m_shakeAmount;
				if(m_shakeDecayOverTime) {
					decayedShakeAmt.x *= Mathf.InverseLerp(0, m_shakeDuration, m_shakeTimer);
					decayedShakeAmt.y *= Mathf.InverseLerp(0, m_shakeDuration, m_shakeTimer);
					decayedShakeAmt.z *= Mathf.InverseLerp(0, m_shakeDuration, m_shakeTimer);
				}
				
				newPos.x += Random.Range(-decayedShakeAmt.x, decayedShakeAmt.x);
				newPos.y += Random.Range(-decayedShakeAmt.y, decayedShakeAmt.y);
				newPos.z += Random.Range(-decayedShakeAmt.z, decayedShakeAmt.z);
			}
		}

		// DONE! Apply new position
		transform.position = newPos;

		UpdateFrustumBounds();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

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

		m_deactivation.center =center;
		m_deactivation.size = new Vector3(frustumWidth + m_deactivationDistance * 2f, frustumHeight + m_deactivationDistance * 2f, 4f);
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
	// DANGER															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Define a given transform as a danger.
	/// The camera will react in consequence.
	/// </summary>
	/// <param name="_danger">The dangerous object, set to null to clear it.</param>
	public void SetDanger(Transform _danger) {
		m_danger = _danger;
		if(_danger == null) {
			Zoom(defaultZoom, 0.25f);	// Danger cleared, go back to normal zoom level
		} else {
			Zoom(1f, 0.5f);	// Danger!! Zoom out
		}
	}



	//------------------------------------------------------------------//
	// DANGER															//
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


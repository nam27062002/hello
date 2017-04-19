// AmbientHazard.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/08/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Generic controller for all passive hazards in the level (fire jets, poison steam, etc.)
/// Based on HSX's EnvironmentHazard class.
/// TODO:
/// - Damage logic
/// - Debris (on hit fx)
/// - Move damage to content?
/// - Text Feedback
/// - SFX
/// </summary>
public class AmbientHazard : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		INITIAL_DELAY,	// One-time only state
		IDLE,
		ACTIVATING,
		ACTIVE
	}

	public enum CollisionShape {
		SPHERE = 0,
		CUBOID,
		//CONE,
		CUSTOM
	}

	private static readonly float VISUAL_ACTIVATION_CHECK_INTERVAL = 0.2f;
	// No need to check every frame!

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Setup
	// [AOC] Some of these could go into content
	[Separator("Stats")]
	[Tooltip("hp per second while poisoned")]
	[SerializeField] private float m_damageBase = 5f;
	[Tooltip("To optionally tweak individual objects")]
	[SerializeField] private float m_damageMultiplier = 1f;
	[Tooltip("Duration of the poison effect")]
	[SerializeField] private float m_damageDuration = 0.5f;
	[SerializeField] private DamageType m_damageType = DamageType.POISON;
	[Tooltip("How hard the hazard knocks back the player on contact.\nDisable the \"trigger\" flag on the collider in order to use knockback.")]
	[SerializeField] private float m_knockBackIntensity = 0f;

	[Separator("Behaviour Setup")]
	[Tooltip("The distance from the player at which the hazard shows up.\nNegative value for always visible.")]
	[SerializeField] private float m_visualActivationRadius = 60f;
	[Tooltip("Initial state of the hazard upon activation")]
	[SerializeField] private State m_initialState = State.IDLE;
	[Tooltip("One time only delay at the start of the game. Use it to sync sequences of hazards.")]
	[SerializeField] private float m_initialDelay = 0f;
	[Header("State Durations")]
	[Tooltip("The cooldown between activations")]
	[SerializeField] private float m_idleDuration = 5f;
	[Tooltip("Time between starting the activation effect and the moment we actually start dealing damage.\nUse it to sync with the animation/VFX.")]
	[SerializeField] private float m_activationDuration = 1f;
	[Tooltip("The time the hazard is active.\nNegative value for continuous activity.")]
	[SerializeField] private float m_activeDuration = 5f;

	// References
	[Separator("References")]
	[SerializeField] private GameObject m_particlesPrefab = null;
	// [AOC] Will be pooled
	[SerializeField] private GameObject m_view = null;
	// We want the logic to keep running when the object is not visible, so keep the view apart
	[SerializeField] private Animator m_animator = null;
	// Optional

	[Separator("Sound")]
	[SerializeField] private string m_onActiveSound;

	// Collision
	[Separator("Collider")]
	[SerializeField] private CollisionShape m_collisionShape = CollisionShape.CUBOID;

	// Cone-specific params
	[SerializeField] private Vector3 m_coneOrigin = Vector3.zero;
	[SerializeField] private Vector3 m_coneRotation = Vector3.zero;
	[SerializeField] private float m_coneLength = 2f;
	[SerializeField] private float m_coneArc = 45f;

	[Separator("Anim Events")]
	[SerializeField] private AmbientHazardAnimEvents m_animEvents;

	// Internal references
	private GameObject m_particlesObj = null;
	private Collider m_collider = null;

	// Dynamic references
	private DragonHealthBehaviour m_dragonHealthBehaviour = null;

	private DragonHealthBehaviour dragonHealthBehaviour {
		get {
			if(m_dragonHealthBehaviour == null) {
				if(InstanceManager.player != null) m_dragonHealthBehaviour = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
			}
			return m_dragonHealthBehaviour;
		}
	}

	private DragonMotion m_dragonMotion = null;

	private DragonMotion dragonMotion {
		get {
			if(m_dragonMotion == null) {
				if(InstanceManager.player != null) m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
			}
			return m_dragonMotion;
		}
	}

	// Internal logic
	private bool m_visible = true;
	private State m_state = State.IDLE;

	// Internal timers
	private float m_stateTimer = 0f;
	private float m_stateTargetTime = 0f;
	private float m_visualActivationCheckTimer = 0f;

	// Internal values
	[SerializeField][HideInInspector] private float m_visualActivationRadiusSqr = 3600f;
	// Use squared distance for better performance
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize internal references
		m_collider = GetComponent<Collider>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		if ( m_visualActivationRadius < 0 )
			m_visualActivationRadius = 40;

		// Always start with the initial delay state, provided there is some initial delay. Otherwise go straight to the initial state.
		if(m_initialDelay > 0f) {
			SetState(State.INITIAL_DELAY);
		} else {
			SetState(m_initialState);
		}

		if ( m_animEvents != null )
		{
			m_animEvents.onOpenEvent += OnAnimOpenEvent;
		}

		// Visibility logic
		if(m_visualActivationRadius <= 0f) {
			// Always visible
			SetVisible(true);
		}
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Update state timer
		if(m_stateTargetTime > 0f) {
			m_stateTimer += Time.deltaTime;
			if(m_stateTimer >= m_stateTargetTime) {
				// Timer up! Different actions based on state
				switch(m_state) {
					case State.INITIAL_DELAY: {
						// Move to initial state
						SetState(m_initialState);
					} break;

					case State.IDLE: {
						SetState(State.ACTIVATING);
					} break;

					case State.ACTIVATING: {
						SetState(State.ACTIVE);
					} break;

					case State.ACTIVE: {
						SetState(State.IDLE);
					} break;
				}
			}
		}

		// Update visual activation
		// Only if visual activation is enabled (radius > 0)
		if(m_visualActivationRadius > 0f) {
			// Update detection timer (optimization to avoid performing the check every frame)
			m_visualActivationCheckTimer += Time.deltaTime;
			if(m_visualActivationCheckTimer >= VISUAL_ACTIVATION_CHECK_INTERVAL) {
				// Reset timer
				m_visualActivationCheckTimer = 0f;

				// Check whether player is within the activation distance
				if(InstanceManager.player != null) {
					bool isWithinDistance = Mathf.Abs((InstanceManager.player.transform.position - this.transform.position).sqrMagnitude) < m_visualActivationRadiusSqr;

					// Show/hide based on trigger
					if(m_visible && !isWithinDistance) {
						SetVisible(false);
					} else if(!m_visible && isWithinDistance) {
						SetVisible(true);
					}
				}
			}
		}
	}

	void OnAnimOpenEvent ()
	{
		ActivateParticles( true );
	}

	/// <summary>
	/// A value has been changed on the inspector.
	/// </summary>
	private void OnValidate() {
		// Update detection area squared
		m_visualActivationRadiusSqr = m_visualActivationRadius * m_visualActivationRadius;

		// Validate some parameters
		m_idleDuration = Mathf.Max(0f, m_idleDuration);

		// Initial state can't be INITIAL_DELAY, this is an internal state
		if(m_initialState == State.INITIAL_DELAY) m_initialState = State.IDLE;
	}

	#if UNITY_EDITOR
	/// <summary>
	/// Editor scene helpers.
	/// </summary>
	void OnDrawGizmosSelected() {
		// Draw activation area
		if(m_visualActivationRadius > 0f) {
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Colors.WithAlpha(Colors.lime, 0.25f);
			Gizmos.DrawSphere(this.transform.position, m_visualActivationRadius);
		}
	}
	#endif

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Sets hazard's visibility state.
	/// </summary>
	/// <param name="_visible">Whether to show or hide the hazard.</param>
	private void SetVisible(bool _visible) {
		// Store new state amd reset check timer
		m_visible = _visible;
		m_visualActivationCheckTimer = 0f;

		// Apply to model
		if(m_view != null) m_view.SetActive(_visible);

		// Particles
		if (m_visible) {
			// If particles are not created, do it now
			if (m_particlesObj == null && m_particlesPrefab != null) {
				m_particlesObj = ParticleManager.Spawn(m_particlesPrefab);
				if (m_particlesObj != null) {
					// As children of ourselves
					// Particle system should already be created to match the zero position
					m_particlesObj.transform.SetParentAndReset(this.transform);

					// Particles are automatically started when spawned from the pool
					// Stop them if hazard is not active
					if(m_state == State.IDLE || m_state == State.ACTIVATING) ActivateParticles(false);
				}
			}

			if ( m_state == State.ACTIVATING || m_state == State.ACTIVE )
			{
				// In case always
				if(m_animator != null && m_animator.isInitialized) {
					m_animator.SetBool("active", true);
					if ( m_state == State.ACTIVE )
					{
						m_animator.Play("ACTIVATE", 0, 1);
						ActivateParticles(true);
					}
				}
			}
			else if ( m_state == State.IDLE )
			{
				m_animator.Play("DEACTIVATE", 0, 1);
			}
		} else {
			// Return them to the pool
			if (m_particlesObj != null) {
				ParticleManager.ReturnInstance(m_particlesObj);
				m_particlesObj = null;
			}
		}
	}

	/// <summary>
	/// Sets the logical state of the hazard.
	/// </summary>
	/// <param name="_newState">The hazard's new logical state.</param>
	private void SetState(State _newState) {
		// Different actions based on state
		switch(_newState) {
			case State.INITIAL_DELAY: {
				// Same as IDLE state, but with a different timer
				SetState(State.IDLE);

				// Change target time
				m_stateTargetTime = m_initialDelay;
			} break;

			case State.IDLE: {
				// Stop particles
				ActivateParticles(false);

				// Disable collider
				m_collider.enabled = false;

				// Launch deactivation animation
				if(m_animator != null && m_animator.isInitialized) {
					m_animator.SetBool("active", false);
				}

				// Reset timer
				m_stateTimer = 0f;
				m_stateTargetTime = m_idleDuration;
			} break;

			case State.ACTIVATING: {
				// Launch activation animation
				if(m_animator != null && m_animator.isInitialized) {
					m_animator.SetBool("active", true);
				}

				// Reset timer
				m_stateTimer = 0f;
				m_stateTargetTime = m_activationDuration;

				if ( !string.IsNullOrEmpty(m_onActiveSound) )
					AudioController.Play(m_onActiveSound, transform.position);

				// Start particles! -> done on animation event
				// ActivateParticles(true);
			} break;

			case State.ACTIVE: {

				// Enable collision
				m_collider.enabled = true;

				// Reset timer
				m_stateTimer = 0f;
				m_stateTargetTime = m_activeDuration;	// If negative, the hazard will never leave this state
			} break;
		}

		// Save new state
		m_state = _newState;
	}

	/// <summary>
	/// Play/Stop particle effect, if any.
	/// </summary>
	/// <param name="_activate"><c>true</c> to play the particle systems, <c>false</c> to stop them.</param>
	private void ActivateParticles(bool _activate) {
		// Only if we actually have a particle system!
		if(m_particlesObj == null) return;

		// Propagate to all subsystems
		ParticleSystem[] subsystems = m_particlesObj.GetComponentsInChildren<ParticleSystem>();
		for(int i = 0; i < subsystems.Length; i++) {
			if(_activate) {
				subsystems[i].Clear();
				subsystems[i].Play();
			} else {
				subsystems[i].Stop();
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// An object has entered the collision area.
	/// </summary>
	/// <param name="_collision">Collision information.</param>
	private void OnCollisionEnter(Collision _collision) {
		// Is it the player?
		DragonPlayer player = InstanceManager.player;
		if(player != null && _collision.collider.CompareTag("Player")) {
			// Apply initial damage
			if(dragonHealthBehaviour != null) {
				dragonHealthBehaviour.ReceiveDamageOverTime(m_damageBase * m_damageMultiplier, m_damageDuration, m_damageType, transform,true);	// Resetting all current DOTs
			}

			// Apply knockback
			Vector3 repulseDirection = Vector3.zero;
			for(int i = 0; i < _collision.contacts.Length; i++) {
				repulseDirection += _collision.contacts[i].normal;
			}

			if(dragonMotion != null) {
				//dragonMotion.Stop(); ??
				dragonMotion.AddForce(-repulseDirection.normalized * m_knockBackIntensity);
			}

			// [AOC] TODO!! Show debris
			// [AOC] TODO!! Show text feedback
			// [AOC] TODO!! Play SFX
		}
	}

	/// <summary>
	/// An object is within the collision area.
	/// </summary>
	/// <param name="_collider">The object within the area.</param>
	private void OnTriggerStay(Collider _collider) {
		// Is it the player?
		DragonPlayer player = InstanceManager.player;
		if(player != null && _collider.CompareTag( "Player")) {
			// Reset dot timer
			if(dragonHealthBehaviour != null) {
				dragonHealthBehaviour.ReceiveDamageOverTime(m_damageBase * m_damageMultiplier, m_damageDuration, m_damageType, transform,true);	// Resetting all current DOTs
			}

			// Apply knockback
			if (m_knockBackIntensity > 0)
			{
				Vector3 repulseDirection = dragonMotion.position - transform.position;
				if(dragonMotion != null) {
					//dragonMotion.Stop(); ??
					dragonMotion.AddForce(repulseDirection.normalized * m_knockBackIntensity);
				}
			}

			// [AOC] TODO!! Play SFX
		}
	}
}
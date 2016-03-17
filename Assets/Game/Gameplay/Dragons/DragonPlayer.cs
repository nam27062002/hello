// DragonPlayer.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 05/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main access point to a dragon controlled by the player.
/// Contains references to its most used components as well as some common stats
/// such as health, energy, etc.
/// </summary>
public class DragonPlayer : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[Header("Type and general data")]
	[SerializeField] private string m_sku = "";
	[SerializeField] private float m_invulnerableTime = 5f;

	private DragonData m_data = null;
	public DragonData data { get { return m_data; }}

	[Header("Life & energy")]
	private float m_health;
	public float health { get { return m_health; } }
	
	private float m_energy;
	public float energy { get { return m_energy; } }

	private float[] m_fury = new float[2];	//we'll use a secondary variable to store all the fury got while in Rush mode 
	private bool m_furyActive = false;
	public float fury { get { return m_fury[0]; } }

	private float m_superFury;
	private bool m_superFuryActive = false;
	public float superFury{ get { return m_superFury; } }

	// Cache content data
	private float m_energyMax = 1f;
	private float m_furyMax = 1f;
	private float m_healthWarningThreshold = 1f;

	// Interaction
	public bool playable {
		set {
			// Enable/disable all the components that make the dragon playable
			// Add as many as needed
			GetComponent<DragonControlPlayer>().enabled = value;	// Move around
			GetComponent<DragonEatBehaviour>().enabled = value;		// Eat stuff
			GetComponent<DragonHealthBehaviour>().enabled = value;	// Receive damage
			GetComponent<DragonBoostBehaviour>().enabled = value;	// Boost
		}
	}

	// References
	private DragonBreathBehaviour m_breathBehaviour = null;

	// Internal
	private float m_speedMultiplier;
	private float m_invulnerableAfterReviveTimer;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake () {
		// Get data from dragon manager
		m_data = DragonManager.GetDragonData(m_sku);
		DebugUtils.Assert(m_data != null, "Attempting to instantiate a dragon player with an ID not defined in the manager.");

		// Store reference into Instance Manager for immediate global access
		InstanceManager.player = this;

		// Cache content data
		m_energyMax = m_data.def.GetAsFloat("energyMax");
		m_furyMax = m_data.def.GetAsFloat("furyMax");
		m_healthWarningThreshold = Definitions.GetDefinition(Definitions.Category.SETTINGS, "dragonSettings").GetAsFloat("healthWarningThreshold");

		// Initialize stats
		ResetStats(false);

		// Initialize fury
		m_fury[0] = 0;
		m_fury[1] = 0;
		m_furyActive = false;

		// Initiate super fury
		m_superFury = 0;
		m_superFuryActive = false;

		// Get external refernces
		m_breathBehaviour = GetComponent<DragonBreathBehaviour>();

		gameObject.AddComponent<WindTrailManagement>();
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);

		// Make sure the dragon has the scale according to its level
		gameObject.transform.localScale = Vector3.one * data.scale;
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
	}

	private void Update() {
		if (m_invulnerableAfterReviveTimer > 0) {
			m_invulnerableAfterReviveTimer -= Time.deltaTime;
			if (m_invulnerableAfterReviveTimer <= 0) {
				m_invulnerableAfterReviveTimer = 0;
			}
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Reset some variable stats for this dragon.
	/// </summary>
	public void ResetStats(bool _revive) {
		m_health = data.maxHealth;
		m_energy = m_energyMax;

		playable = true;

		m_speedMultiplier = 1f;

		if (_revive) {
			m_invulnerableAfterReviveTimer = m_invulnerableTime;
		} else {
			m_invulnerableAfterReviveTimer = 0;
		}
	}

	/// <summary>
	/// Add/remove health to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of health to be added/removed.</param>
	public void AddLife(float _offset) {
		// If invulnerable and taking damage, don't apply
		if(IsInvulnerable() && _offset < 0) return;

		// Aux vars
		bool wasStarving = IsStarving();

		// Update health
		m_health = Mathf.Min(m_data.maxHealth, Mathf.Max(0, m_health + _offset));

		// Check for death!
		if(m_health <= 0f) {
			// Send global even
			Messenger.Broadcast(GameEvents.PLAYER_KO);
			Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, false);

			// Make dragon unplayable (xD)
			playable = false;
		}

		// Check for starvation
		else if(wasStarving != IsStarving()) {
			Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, IsStarving());
		}
	}

	/// <summary>
	/// Add/remove energy to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of energy to be added/removed.</param>
	public void AddEnergy(float _offset) {
		m_energy = Mathf.Min(m_energyMax, Mathf.Max(0, m_energy + _offset));
	}
		
	/// <summary>
	/// Add/remove fury to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of fury to be added/removed.</param>
	public void AddFury(float _offset) {
		if ( !m_superFuryActive )
		{
			if (m_furyActive && _offset >= 0) {
				m_fury[1] = Mathf.Min(m_furyMax, Mathf.Max(0, m_fury[1] + _offset)); 
			} else {
				m_fury[0] = Mathf.Min(m_furyMax, Mathf.Max(0, m_fury[0] + _offset)); 
			}
		}
	}

	/// <summary>
	/// Start fury rush.
	/// </summary>
	public void StartFury() {
		m_furyActive = true;
	}

	/// <summary>
	/// Determines whether this instance is fury on.
	/// </summary>
	/// <returns><c>true</c> if this instance is fury on; otherwise, <c>false</c>.</returns>
	public bool IsFuryOn() {
		
		return m_furyActive;
	}


	/// <summary>
	/// End fury rush.
	/// </summary>
	public void StopFury() {
		//when player used all the fury, we swap all the fury we got while throwing fire
		m_furyActive = false;
		m_fury[0] = m_fury[1];
		m_fury[1] = 0;
	}


	/// <summary>
	/// Add/remove super fury to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of super fury to be added/removed.</param>
	public void AddSuperFury(float _offset) {
		m_superFury = Mathf.Min(m_furyMax, Mathf.Max(0, m_superFury + _offset)); 
	}

	/// <summary>
	/// Start super fury rush.
	/// </summary>
	public void StartSuperFury() {
		m_superFuryActive = true;
	}

	/// <summary>
	/// Determines whether this instance is super fury on.
	/// </summary>
	/// <returns><c>true</c> if this instance is super fury on; otherwise, <c>false</c>.</returns>
	public bool IsSuperFuryOn() {
		
		return m_superFuryActive;
	}


	/// <summary>
	/// End super fury rush.
	/// </summary>
	public void StopSuperFury() {
		//when player used all the super fury, we swap all the super fury we got while throwing super fire
		m_superFuryActive = false;
		m_superFury = 0;
	}


	/// <summary>
	/// Sets the speed multiplier.
	/// </summary>
	/// <param name="_value">The new speed multiplier.</param>
	public void SetSpeedMultiplier(float _value) {
		m_speedMultiplier = _value;
	}

	/// <summary>
	/// Compute and get the accumulated speed multiplier.
	/// </summary>
	/// <returns>The current speed multiplier.</returns>
	public float GetSpeedMultiplier() {
		//we'll return the current speed factor. The dragon can be slowed down because it grabbed something or speed up by boost
		//calculate the final value, multiplying all the factors
		return m_speedMultiplier;
	}

	/// <summary>
	/// Moves this dragon to its default spawn point in the current level.
	/// If there is no specific spawn point for this dragon's id, move it to the
	/// default spawn point for all dragons.
	/// If there is no level loaded or no spawn points could be found, dragon stays
	/// at its current position.
	/// Uses GameObject.Find, so don't abuse it!
	/// </summary>
	public void MoveToSpawnPoint() {
		// Look for a default spawn point for this dragon type in the scene and move the dragon there
		GameObject spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + data.def.sku);
		if(spawnPointObj == null) {
			// We couldn't find a spawn point for this specific type, try to find a generic one
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME);
		}
		if(spawnPointObj != null) {
			transform.position = spawnPointObj.transform.position;

			if (InstanceManager.pet != null) {
				InstanceManager.pet.transform.position = spawnPointObj.transform.position;
			}
		}
	}

	//------------------------------------------------------------------//
	// GETTERS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Is the dragon alive?
	/// </summary>
	/// <returns><c>true</c> if the dragon is not dead or dying; otherwise, <c>false</c>.</returns>
	public bool IsAlive() {
		return health > 0;
	}
	
	/// <summary>
	/// Is the dragon starving?
	/// </summary>
	/// <returns><c>true</c> if the dragon is alive and its current life under the specified warning threshold; otherwise, <c>false</c>.</returns>
	public bool IsStarving() {
		return (health < data.maxHealth * m_healthWarningThreshold);
	}
	
	/// <summary>
	/// Whether the dragon can take damage or not.
	/// </summary>
	/// <returns><c>true</c> if the dragon currently is invulnerable; otherwise, <c>false</c>.</returns>
	public bool IsInvulnerable() {
		// After revive, we're invulnerable
		if(m_invulnerableAfterReviveTimer > 0) return true;

		// During fire, we're invulnerable
		if(m_breathBehaviour.IsFuryOn()) return true;
		
		// If cheat is enable
		if(DebugSettings.invulnerable) return true;
		
		// All checks passed, we're not invulnerable
		return false;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The dragon has leveled up.
	/// </summary>
	/// <param name="_data">The data of the dragon that just leveled up.</param>
	private void OnLevelUp(DragonData _data) {
		// Assume it's this dragon
		// Make sure the dragon has the scale according to its level
		gameObject.transform.localScale = Vector3.one * data.scale;
	}
}

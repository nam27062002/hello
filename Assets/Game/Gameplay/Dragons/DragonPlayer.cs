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

	private DragonBreathBehaviour m_dragonBreath;

	[Header("Life & energy")]
	private float m_health;
	public float health { get { return m_health; } }
	
	private float m_energy;
	public float energy { get { return m_energy; } }

	// Cache content data
	private float m_healthMax = 1f;
	public float healthMax {get{return m_healthMax;}}
	private float m_healthBase = 1f;
	public float healthBase {get{return m_healthBase;}}
	public float healthFraction{ get{ return m_health / m_healthMax; } }

	private float m_energyMax = 1f;
	public float energyMax {get{return m_energyMax;}}
	private float m_energyBase = 1f;
	public float energyBase {get{return m_energyBase;}}

	private float m_healthWarningThreshold = 1f;

	// Power up addition done to the max value ( tant per cent to add)
	private float m_healthModifier = 0;
	private float m_energyModifier = 0;

	private int m_mineShield;
	private int m_freeRevives = 0;
	private int m_tierIncreaseBreak = 0;

	public float furyProgression
	{
		get { return m_dragonBreath.GetFuryProgression(); }
	}

	public float superFuryProgression
	{
		get{ return m_dragonBreath.GetSuperFuryProgression(); }
	}

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
		m_healthMax = m_data.maxHealth;
		m_energyMax = m_data.energySkill.value;
		m_healthWarningThreshold = DefinitionsManager.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings").GetAsFloat("healthWarningThreshold");

		m_healthModifier = 0;
		m_energyModifier = 0;

		// Check avoid first hit modifiers
		m_mineShield = 0;
		m_freeRevives = 0;
		m_tierIncreaseBreak = 0;

		// Initialize stats
		ResetStats(false);

		// Get external refernces
		m_breathBehaviour = GetComponent<DragonBreathBehaviour>();

		gameObject.AddComponent<WindTrailManagement>();

		m_dragonBreath = GetComponent<DragonBreathBehaviour>();

		// Subscribe to external events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
	}

	void OnDestroy()
	{
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() 
	{
		// Make sure the dragon has the scale according to its level
		gameObject.transform.localScale = Vector3.one * data.scale;
		SetHealthModifier( m_healthModifier );
		SetBoostModifier( m_energyModifier );
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
		m_health = m_healthMax;
		m_energy = m_energyMax;

		playable = true;

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
		m_health = Mathf.Min(m_healthMax, Mathf.Max(0, m_health + _offset));

		// Check for death!
		if(m_health <= 0f) 
		{
			// Check if free revive
			if (m_freeRevives > 0)
			{
				m_freeRevives--;
				ResetStats(true);
				Messenger.Broadcast(GameEvents.PLAYER_FREE_REVIVE);
			}
			else
			{
				// Send global even
				Messenger.Broadcast(GameEvents.PLAYER_KO);
				Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, false);

				// Make dragon unplayable (xD)
				playable = false;
			}
		}

		// Check for starvation
		if(wasStarving != IsStarving()) {
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
	/// Determines whether this instance is fury on.
	/// </summary>
	/// <returns><c>true</c> if this instance is fury on; otherwise, <c>false</c>.</returns>
	public bool IsFuryOn() {
		
		return m_dragonBreath.IsFuryOn() && m_dragonBreath.type == DragonBreathBehaviour.Type.Standard;
	}


	/// <summary>
	/// Determines whether this instance is super fury on.
	/// </summary>
	/// <returns><c>true</c> if this instance is super fury on; otherwise, <c>false</c>.</returns>
	public bool IsSuperFuryOn() {
		
		return m_dragonBreath.IsFuryOn() && m_dragonBreath.type == DragonBreathBehaviour.Type.Super;
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

	public void StartIntroMovement()
	{
		// Look for a default spawn point for this dragon type in the scene and move the dragon there
		GameObject spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + data.def.sku);
		if(spawnPointObj == null) {
			// We couldn't find a spawn point for this specific type, try to find a generic one
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME);
		}
		if(spawnPointObj != null) 
		{
			Vector3 introPos = spawnPointObj.transform.position;
			transform.position = introPos + Vector3.left * 30;
			if (InstanceManager.pet != null) {
				InstanceManager.pet.transform.position = introPos;
			}
			GetComponent<DragonMotion>().StartIntroMovement(introPos);
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
		return (health < m_healthMax * m_healthWarningThreshold);
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

		SetHealthModifier( m_healthModifier );
		SetBoostModifier( m_energyModifier );
	}

	public void LoseMineShield()
	{
		m_mineShield--;
	}
	public bool HasMineShield()
	{
		return m_mineShield > 0;
	}

	public int GetReminingLives()
	{
		return m_freeRevives;
	}

	/// <summary>
	/// Gets the tier when breaking. Because we can have the "Destroy" power up wich increases the 
	/// tier on the dragon breaking things we have this function to ask on the proper places
	/// </summary>
	/// <returns>The tier when breaking.</returns>
	public DragonTier GetTierWhenBreaking()
	{
		DragonTier ret = m_data.tier + m_tierIncreaseBreak;
		return ret;
	}

	public void SetOnBreakIncrease( int increase )
	{
		m_tierIncreaseBreak = increase;
	}

	// Increases health max by value where value is a tant per cent
	public void SetHealthModifier( float value )
	{
		m_healthBase = m_data.maxHealth;
		m_healthModifier = value;
		m_healthMax = m_healthBase + (m_healthModifier / 100.0f * m_healthBase);
		m_health = m_healthMax;
	}

	public void SetBoostModifier( float value )
	{
		m_energyBase = m_data.energySkill.value;
		m_energyModifier = value;
		m_energyMax = m_energyBase + ( m_energyModifier / 100.0f * m_energyBase );
		m_energy = m_energyMax;
	}

	public void SetFreeRevives( int revives )
	{
		m_freeRevives = revives;
	}

	public void SetMineShields( int numHits )
	{
		m_mineShield = numHits;
	}
}

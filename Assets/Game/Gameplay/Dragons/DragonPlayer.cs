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
using System.Collections.Generic;

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
	private float m_healthCriticalThreshold = 1;

	// Power up addition done to the max value ( tant per cent to add)
	private float m_healthModifier = 0;
	private float m_energyModifier = 0;

	private int m_mineShield;
	private int m_freeRevives = 0;
	private int m_tierIncreaseBreak = 0;

	private HoldPreyPoint[] m_holdPreyPoints = null;
	public HoldPreyPoint[] holdPreyPoints { get{ return m_holdPreyPoints; } }

	private int m_numLatching = 0;

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
	public DragonBreathBehaviour breathBehaviour
	{
		get{ return m_breathBehaviour; }
	}

	private DragonMotion m_dragonMotion = null;
	public DragonMotion dragonMotion
	{
		get{ return m_dragonMotion; }
	}

	private DragonEatBehaviour m_dragonEatBehaviour = null;
	public DragonEatBehaviour dragonEatBehaviour
	{
		get{ return m_dragonEatBehaviour; }
	}

	private DragonHealthBehaviour m_dragonHeatlhBehaviour = null;
	public DragonHealthBehaviour dragonHealthBehaviour
	{
		get{ return m_dragonHeatlhBehaviour; }
	}

	private DragonBoostBehaviour m_dragonBoostBehaviour = null;
	public DragonBoostBehaviour dragonBoostBehaviour
	{
		get{ return m_dragonBoostBehaviour; }
	}

	public float furyProgression
	{
		get { return m_breathBehaviour.GetFuryProgression(); }
	}

	public float superFuryProgression
	{
		get{ return m_breathBehaviour.GetSuperFuryProgression(); }
	}

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
		m_energyMax = m_data.def.GetAsFloat("energyBase");
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings");
		m_healthWarningThreshold = def.GetAsFloat("healthWarningThreshold");
		m_healthCriticalThreshold = def.GetAsFloat("healthCriticalThreshold");


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
		m_dragonMotion = GetComponent<DragonMotion>();
		m_dragonEatBehaviour =  GetComponent<DragonEatBehaviour>();
		m_dragonHeatlhBehaviour = GetComponent<DragonHealthBehaviour>();
		m_dragonBoostBehaviour = GetComponent<DragonBoostBehaviour>();

		// gameObject.AddComponent<WindTrailManagement>();
		m_holdPreyPoints = transform.GetComponentsInChildren<HoldPreyPoint>();

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

	public void ReviveScale()
	{
		gameObject.transform.localScale = Vector3.zero;
		StartCoroutine("ReviveScaleCoroutine");
	}

	IEnumerator ReviveScaleCoroutine()
	{
		float duration = 1;
		float timer = 0;

		while( timer < duration )
		{
			timer += Time.deltaTime;
			gameObject.transform.localScale = Vector3.one * data.scale * Mathf.Clamp01( timer / duration);
			yield return null;
		}
		playable = true;
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

		bool wasStarving = IsStarving();
		bool wasCritical = IsCritical();

		m_health = m_healthMax;
		m_energy = m_energyMax;



		if (_revive) {
			m_invulnerableAfterReviveTimer = m_invulnerableTime;
		} else {
			m_invulnerableAfterReviveTimer = 0;
		}

		if ( _revive )
		{
			m_dragonMotion.Revive();
			ReviveScale();

			bool isStarving = IsStarving();
			if(wasStarving != isStarving) {
				Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, isStarving);
			}

			bool isCritical = IsCritical();
			if(wasCritical != isCritical) {
				Messenger.Broadcast<bool>(GameEvents.PLAYER_CRITICAL_TOGGLED, isCritical);
			}

			Messenger.Broadcast(GameEvents.PLAYER_REVIVE);
		}
		else
		{
			playable = true;
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
		bool wasCritical = IsCritical();
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
			// If I have an angel pet and aura still playing

			else
			{
				m_dragonMotion.Die();

				// Send global even
				Messenger.Broadcast(GameEvents.PLAYER_KO);
					// Hode Starving and Critical effects
				if (wasStarving)
					Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, false);
				if (wasCritical)
					Messenger.Broadcast<bool>(GameEvents.PLAYER_CRITICAL_TOGGLED, false);
				// Make dragon unplayable (xD)
				playable = false;
			}
		}
		else
		{
			// Check for starvation
			bool isStarving = IsStarving();
			if(wasStarving != isStarving) {
				Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, isStarving);
			}

			bool isCritical = IsCritical();
			if(wasCritical != isCritical) {
				Messenger.Broadcast<bool>(GameEvents.PLAYER_CRITICAL_TOGGLED, isCritical);

				// Special case: if we're leaving the critical stat but we're still starving, toggle starving mode
				if(!isCritical && isStarving) {
					Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, isStarving);
				}
			}
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
		
		return m_breathBehaviour.IsFuryOn() && m_breathBehaviour.type == DragonBreathBehaviour.Type.Standard;
	}


	/// <summary>
	/// Determines whether this instance is super fury on.
	/// </summary>
	/// <returns><c>true</c> if this instance is super fury on; otherwise, <c>false</c>.</returns>
	public bool IsSuperFuryOn() {
		
		return m_breathBehaviour.IsFuryOn() && m_breathBehaviour.type == DragonBreathBehaviour.Type.Super;
	}


	/// <summary>
	/// Moves this dragon to its default spawn point in the current level.
	/// If there is no specific spawn point for this dragon's id, move it to the
	/// default spawn point for all dragons.
	/// If there is no level loaded or no spawn points could be found, dragon stays
	/// at its current position.
	/// Uses GameObject.Find, so don't abuse it!
	/// </summary>
	/// <param name="_levelEditor">Try to use the level editor's spawn point?</param>
	public void MoveToSpawnPoint(bool _levelEditor) {
		// Use level editor's spawn point or try to use specific's dragon spawn point?
		GameObject spawnPointObj = null;
		if(_levelEditor) {
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + LevelEditor.LevelTypeSpawners.LEVEL_EDITOR_SPAWN_POINT_NAME);
			if ( spawnPointObj == null )
				spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + data.def.sku);
		} else {
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + data.def.sku);
		}

		// If we couldn't find a valid spawn point, try to find a generic one
		if(spawnPointObj == null) {
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME);
		}

		// Move to position
		if(spawnPointObj != null) {
			transform.position = spawnPointObj.transform.position;
			/*
			if (InstanceManager.pet != null) {
				InstanceManager.pet.transform.position = spawnPointObj.transform.position;
			}
			*/
		}
	}

	public void StartIntroMovement()
	{
		// Look for a default spawn point for this dragon type in the scene and move the dragon there
		GameObject spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME +"_" + data.def.sku);
		if(spawnPointObj == null) {
			// We couldn't find a spawn point for this specific type, try to find a generic one
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME);
		}
		if(spawnPointObj != null) 
		{
			Vector3 introPos = spawnPointObj.transform.position;
			m_dragonMotion.StartIntroMovement(introPos);
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
		return (m_health < m_healthMax * m_healthWarningThreshold);
	}

	/// <summary>
	/// Is dragon in critical confition?
	/// </summary>
	/// <returns><c>true</c> if this instance is critical; otherwise, <c>false</c>.</returns>
	public bool IsCritical()
	{
		return (m_health < m_healthMax * m_healthCriticalThreshold);
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
		m_energyBase = m_data.def.GetAsFloat("energyBase");
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

	public void StartLatchedOn()
	{
		m_numLatching++;
		if ( m_numLatching == 1 )
		{
			m_dragonMotion.StartLatchedOnMovement();
			m_dragonEatBehaviour.PauseEating();
		}
	}

	public void EndLatchedOn()
	{
		m_numLatching--;
		if ( m_numLatching == 0 )
		{
			m_dragonMotion.EndLatchedOnMovement();
			m_dragonEatBehaviour.ResumeEating( 2.5f );
		}
	}

	public bool BeingLatchedOn()
	{
		return m_numLatching > 0;
	}

	public bool HasFreeHoldPoint()
	{
		for( int i = 0; i< m_holdPreyPoints.Length; i++ )
			if ( !m_holdPreyPoints[i].holded ) return true;
		return false;
	}
}

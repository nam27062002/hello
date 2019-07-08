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
public class DragonPlayer : MonoBehaviour, IBroadcastListener {

	//------------------------------------------------------------------//
	//------------------------------------------------------------------//
	public enum ReviveReason
	{
		AD,
		PAYING,
		FREE_REVIVE_PET,
        MUMMY,
		UNKNOWN
	};

    public enum Form {
        NORMAL = 0,
        MUMMY
    };


	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[Header("Type and general data")]
	[SerializeField] private string m_sku = "";
	public string sku
	{
		get{ return m_sku; }
	}
	[SerializeField] private float m_invulnerableTime = 5f;

	private IDragonData m_data = null;
	public IDragonData data { get { return m_data; }}

	[Header("Life & energy")]
	[SerializeField] private float m_health;
	public float health { get { return m_health; } }

	[SerializeField] private float m_energy;
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

	// Health modifiers
	private DragonHealthModifier[] m_healthModifiers = null;	// Sorted by threshold
	private DragonHealthModifier m_currentHealthModifier = null;
	public DragonHealthModifier currentHealthModifier {
		get { return m_currentHealthModifier; }
	}

	// Power up addition done to the max value ( tant per cent to add)
	private float m_healthBonus = 0;
	private float m_energyBonus = 0;

	public Dictionary<DamageType, int> m_shield;
	public Dictionary<DamageType, float> m_shieldTimers;
	public const float m_shieldsDuration = 1.0f;

	private int m_freeRevives = 0;
	private int m_tierIncreaseBreak = 0;

	private HoldPreyPoint[] m_holdPreyPoints = null;
	public HoldPreyPoint[] holdPreyPoints { get{ return m_holdPreyPoints; } }

	private int m_numLatching = 0;

	// default size
	private float m_defaultSize = 1;

	// Super size transformation
	private float m_superSizeTarget = 1;
	private float m_superSizeSize = 1;
	private float m_superSizeStart = 1;
	private float m_superSizeTimer = 0f;
	private float m_superSizeDuration = 0.5f;


	// Interaction
	private bool m_playable = true;
	public bool playable {
		set {
			// Store new value
			m_playable = value;

            // Enable/disable all the components that make the dragon playable
            // Add as many as needed
            // GetComponent<DragonControlPlayer>().enabled = value;	// Move around
            if (m_dragonEatBehaviour != null) m_dragonEatBehaviour.enabled = value;// Eat stuff
			GetComponent<DragonHealthBehaviour>().enabled = value;	// Receive damage
			GetComponent<DragonBoostBehaviour>().enabled = value;	// Boost
		}

		get { return m_playable; }
	}

    // References
    DragonParticleController m_particleController = null;
    public DragonParticleController particleController {
        get { return m_particleController; }
    }

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
    
    private DragonShieldBehaviour m_dragonShieldBehaviour = null;
    public DragonShieldBehaviour dragonShieldBehaviour
    {
        get{ return m_dragonShieldBehaviour; }
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
    
    public float shield
    {
        get{ return m_dragonShieldBehaviour != null ? m_dragonShieldBehaviour.m_currentShield : 0; }
    }
    
    public float shieldMax
    {
        get{ return m_dragonShieldBehaviour != null ? m_dragonShieldBehaviour.m_maxShield : 0; }
    }

	// Internal
	private float m_invulnerableAfterReviveTimer;

	private bool m_changingArea = false;
	public bool changingArea
	{
		get{ return m_changingArea; }
		set{ m_changingArea = value; }
	}


	private bool m_superSizeInvulnerable = false;
	public bool superSizeInvulnerable
	{
		get{ return m_superSizeInvulnerable; }
		set{ m_superSizeInvulnerable = value; }
	}

	private bool m_modInvulnerable = false;
	public bool modInvulnerable {
		get { return m_modInvulnerable; }
		set { m_modInvulnerable = value; }
	}

	public DragonCommonSettings m_dragonCommonSettings;


    private Form m_form;
    public Form form { get { return m_form; } }

    private int m_mummyPowerStacks;
    private float m_mummyHealthFactor;
    private float m_mummyTime;
    private float m_mummyDrain;
    private List<Modifier> m_mummyModifiers;

    public float mummyHealthMax { get { return m_healthMax * m_mummyHealthFactor; } }

    public bool m_alwaysSpawnCorpse = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake () {
		DamageTypeComparer comparer = new DamageTypeComparer();
		m_shield = new Dictionary<DamageType, int>(comparer);
		m_shieldTimers = new Dictionary<DamageType, float>(comparer);

        if (DebugSettings.useSpecialDragon)
        {
           m_data = DragonManager.GetDragonData(m_sku);
        }
        else
        {
    		// Get data from dragon manager
    		if ( SceneController.mode == SceneController.Mode.TOURNAMENT )
    		{
				HDTournamentData tournamentData = HDLiveDataManager.tournament.data as HDTournamentData;
				HDTournamentDefinition def = tournamentData.definition as HDTournamentDefinition;
				m_data = def.dragonData;
			}
    		else
    		{
    			m_data = DragonManager.GetDragonData(m_sku);
    		}
        }

		DebugUtils.Assert(m_data != null, "Attempting to instantiate a dragon player with an ID not defined in the manager.");

		m_defaultSize = m_data.scale;

		// Store reference into Instance Manager for immediate global access
		InstanceManager.player = this;

		// Cache content data
		m_healthMax = m_data.maxHealth;
		m_energyMax = m_data.baseEnergy;

		// Init health modifiers
		List<DefinitionNode> healthModifierDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DRAGON_HEALTH_MODIFIERS);
		DefinitionsManager.SharedInstance.SortByProperty(ref healthModifierDefs, "threshold", DefinitionsManager.SortType.NUMERIC);		// Sort by threshold
		m_healthModifiers = new DragonHealthModifier[healthModifierDefs.Count];
		for(int i = 0; i < healthModifierDefs.Count; i++) {
			m_healthModifiers[i] = new DragonHealthModifier(healthModifierDefs[i]);
		}

		// Init bonuses
		m_healthBonus = 0;
		m_energyBonus = 0;

		m_freeRevives = 0;
		m_tierIncreaseBreak = 0;

		// Initialize stats
		ResetStats(false);

        // Get external refernces
        m_particleController = GetComponentInChildren<DragonParticleController>();
		m_breathBehaviour = GetComponent<DragonBreathBehaviour>();
		m_dragonMotion = GetComponent<DragonMotion>();
		m_dragonEatBehaviour =  GetComponent<DragonEatBehaviour>();
		m_dragonHeatlhBehaviour = GetComponent<DragonHealthBehaviour>();
        m_dragonShieldBehaviour = GetComponent<DragonShieldBehaviour>();
		m_dragonBoostBehaviour = GetComponent<DragonBoostBehaviour>();

		// gameObject.AddComponent<WindTrailManagement>();
		m_holdPreyPoints = transform.GetComponentsInChildren<HoldPreyPoint>();

		// Subscribe to external events
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, OnLevelUp);
        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
		Messenger.AddListener<DragonBreathBehaviour.Type, float>(MessengerEvents.PREWARM_FURY_RUSH, OnPrewardmFuryRush);

		Messenger.AddListener(MessengerEvents.PLAYER_ENTERING_AREA, OnEnteringArea);
		Messenger.AddListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, OnLeavingArea);

		Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_DESTROYED, OnEntityDestroyed);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
		if ( ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST )
		{
			DebugSettings.invulnerable = true;
            DebugSettings.infiniteBoost = true;			
		}

        m_mummyModifiers = new List<Modifier>();
        m_mummyHealthFactor = m_data.def.GetAsFloat("mummyHealthFactor");
        m_mummyTime = m_data.def.GetAsFloat("mummyDuration");
        m_form = Form.NORMAL;
	}

	public void RemovePowerUps()
	{
		m_healthBonus = 0;
		SetHealthBonus( m_healthBonus );
		m_energyBonus = 0;
		SetBoostBonus( m_energyBonus );
		
		m_freeRevives = 0;
		m_tierIncreaseBreak = 0;
		m_mummyPowerStacks = 0;

		m_shield.Clear();
		m_shieldTimers.Clear();

		m_dragonHeatlhBehaviour.RemovePowerUps();
	}

	void OnDestroy()
	{
		// Unsubscribe from external events
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
		Messenger.RemoveListener<DragonBreathBehaviour.Type, float>(MessengerEvents.PREWARM_FURY_RUSH, OnPrewardmFuryRush);
		Messenger.RemoveListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, OnLeavingArea);
		Messenger.RemoveListener(MessengerEvents.PLAYER_ENTERING_AREA, OnEnteringArea);
		Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_DESTROYED, OnEntityDestroyed);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
             case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
            case BroadcastEventType.FURY_RUSH_TOGGLED:
            {
                FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                OnFuryToggled(furyRushToggled.activated, furyRushToggled.type);
            }break;
        }
    }
    
	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable()
	{
		// Make sure the dragon has the scale according to its level
		gameObject.transform.localScale = Vector3.one * m_defaultSize;
		SetHealthBonus( m_healthBonus );
		SetBoostBonus( m_energyBonus );
	}

	public void ReviveScale()
	{
		gameObject.transform.localScale = Vector3.zero;
		StartCoroutine("ReviveScaleCoroutine");
	}

	IEnumerator ReviveScaleCoroutine()
	{
		float duration = 1.3f;
		float timer = 0;

		while( timer < duration )
		{
			timer += Time.deltaTime;
			gameObject.transform.localScale = Vector3.one * m_defaultSize * m_dragonCommonSettings.m_reviveScaleCurve.Evaluate( timer );
			yield return null;
		}
		gameObject.transform.localScale = Vector3.one * m_defaultSize;

        if (m_form == Form.MUMMY) {
            m_particleController.StartMummySmoke();
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

		if (m_superSizeTimer > 0 )
		{
			m_superSizeTimer -= Time.deltaTime;
			if ( m_superSizeTimer > 0 )
			{
				m_superSizeSize = Mathf.Lerp( m_superSizeTarget, m_superSizeStart,m_superSizeTimer / m_superSizeDuration);
			}
			else
			{
				m_superSizeSize = m_superSizeTarget;
			}
			gameObject.transform.localScale = Vector3.one * m_defaultSize * m_superSizeSize;
			if (m_breathBehaviour.IsFuryOn())
				m_breathBehaviour.RecalculateSize();
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Reset some variable stats for this dragon.
	/// </summary>
	public void ResetStats(bool _revive, ReviveReason reason = ReviveReason.UNKNOWN) {
		// Store some previous values
		DragonHealthModifier oldHealthModifier = ComputeHealthModifier();

		// Reset stats
		m_health = m_healthMax;
		m_energy = m_energyMax;

		// Update health modifier
		m_currentHealthModifier = ComputeHealthModifier();

		// When reviving, do some special logic
		if ( _revive )
		{
			m_invulnerableAfterReviveTimer = m_invulnerableTime;
			m_dragonMotion.Revive();

			//TONI START
			m_dragonHeatlhBehaviour.SetReviveBonusTime();
			//TONI END
			// If health modifier changed, notify game
			if(m_currentHealthModifier != oldHealthModifier) {
				Messenger.Broadcast<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, oldHealthModifier, m_currentHealthModifier);
			}

			// Notify revive to game
			Messenger.Broadcast<ReviveReason>(MessengerEvents.PLAYER_REVIVE, reason);

			ReviveScale();
		}
		else
		{
			m_invulnerableAfterReviveTimer = 0;
			playable = true;
		}
	}


    /// <summary>
    /// Starts the mummy mode.
    /// </summary>
    public void StartMummyPower() {
        m_form = Form.MUMMY;

        // Store some previous values
        DragonHealthModifier oldHealthModifier = ComputeHealthModifier();

        // Reset stats
        m_health = m_healthMax * m_mummyHealthFactor;
        m_energy = m_energyMax;


        // Update health modifier
        m_currentHealthModifier = null;

        m_invulnerableAfterReviveTimer = m_invulnerableTime;
        m_dragonMotion.Revive();

        // TONI START
        m_dragonHeatlhBehaviour.SetReviveBonusTime();
        // TONI END

        // Modifiers
        m_mummyModifiers.Add(new ModDragonInvulnerable());
        m_mummyModifiers.Add(new ModDragonBoostUnlimited());
        m_mummyModifiers.Add(new ModEntityScore(300f));
        m_mummyModifiers.Add(new ModEntitySC(200f));
        for (int i = 0; i < m_mummyModifiers.Count; ++i) {
            m_mummyModifiers[i].Apply();
        }
        Broadcaster.Broadcast(BroadcastEventType.APPLY_ENTITY_POWERUPS);

        // If health modifier changed, notify game
        if (m_currentHealthModifier != oldHealthModifier) {
            Messenger.Broadcast<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, oldHealthModifier, m_currentHealthModifier);
        }

        //----------------------------------------------------------------
        m_mummyDrain = -((healthMax * m_mummyHealthFactor) / m_mummyTime);
        //----------------------------------------------------------------

        // Notify revive to game
        Messenger.Broadcast<ReviveReason>(MessengerEvents.PLAYER_REVIVE, ReviveReason.MUMMY);

        ReviveScale();
    }

    public void MummyHealthDrain() {
        float drain = m_mummyDrain * Time.deltaTime;

        // Update health
        float lastHealth = m_health;
        m_health = Mathf.Min(m_healthMax, Mathf.Max(0, m_health + drain));

        // Check for death!
        if (lastHealth > 0f && m_health <= 0f) {
            OnHealthZero(DamageType.DRAIN, null);
        }
    }

    private void EndMummyPower() {
        m_particleController.EndMummySmoke();

        // Modifiers
        dragonBoostBehaviour.modInfiniteBoost = false;

        for (int i = 0; i < m_mummyModifiers.Count; ++i) {
            m_mummyModifiers[i].Remove();
        }
        m_mummyModifiers.Clear();
        Broadcaster.Broadcast(BroadcastEventType.APPLY_ENTITY_POWERUPS);

        m_form = Form.NORMAL;
    }


	/// <summary>
	/// Add/remove health to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of health to be added/removed.</param>
	/// <param name="_type">Type of damage, if any.</param>
	/// <param name="_source">Source of the damage, if any.</param>
	public void AddLife(float _offset, DamageType _type, Transform _source) {
		// If invulnerable and taking damage, don't apply
		if(IsInvulnerable() && _offset < 0) return;

		// If cheat is enable
		if(DebugSettings.invulnerable && _offset < 0) return;

        if (m_form == Form.MUMMY && _offset > 0) return;

        // Update health
        float lastHealth = m_health;
        m_health = Mathf.Min(m_healthMax, Mathf.Max(0, m_health + _offset));

		// Check for death!
		if(lastHealth > 0f && m_health <= 0f)
		{
            OnHealthZero(_type, _source);
        }
		else if (lastHealth > 0f && m_health > 0f) {
            // Store some variables
            DragonHealthModifier oldHealthModifier = m_currentHealthModifier;
            // Update health modifier
            m_currentHealthModifier = ComputeHealthModifier();
			if(oldHealthModifier != m_currentHealthModifier) {
				//Debug.Log("HEALTH MODIFIER CHANGE FROM " + (oldHealthModifier == null ? "none" : oldHealthModifier.def.sku) + " TO " + (m_currentHealthModifier == null ? "none" : m_currentHealthModifier.def.sku));
				Messenger.Broadcast<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, oldHealthModifier, m_currentHealthModifier);
			}
		}
	}

    private void OnHealthZero(DamageType _type, Transform _source) {
        // Store some variables
        DragonHealthModifier oldHealthModifier = m_currentHealthModifier;

        m_dragonMotion.Die();

        if (m_form == Form.MUMMY) {
            EndMummyPower();
        }

        // Check if free revive
        if (CanUseMummyPower()) {
            Messenger.Broadcast(MessengerEvents.PLAYER_MUMMY_REVIVE);
            m_mummyPowerStacks--;
        }
        else if (CanUseFreeRevives()) {   
            Messenger.Broadcast(MessengerEvents.PLAYER_FREE_REVIVE);
            m_freeRevives--;
        }
        // If I have an angel pet and aura still playing
        else {
            // Send global event
            Messenger.Broadcast<DamageType, Transform>(MessengerEvents.PLAYER_KO, _type, _source);  // Reason

            // Clear any health modifiers
            m_currentHealthModifier = null;
            if (oldHealthModifier != m_currentHealthModifier) {
                Messenger.Broadcast<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, oldHealthModifier, m_currentHealthModifier);
            }

            // Make dragon unplayable (xD)
            playable = false;
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
	/// Determines whether any type of fury is on.
	/// </summary>
	/// <returns><c>true</c> if this instance is fury on; otherwise, <c>false</c>.</returns>
	public bool IsFuryOn()
	{
		return m_breathBehaviour.IsFuryOn();
	}

	/// <summary>
	/// Determines whether this instance is fury on.
	/// </summary>
	/// <returns><c>true</c> if this instance is fury on; otherwise, <c>false</c>.</returns>
	public bool IsStandardFuryOn() {

		return m_breathBehaviour.IsFuryOn() && m_breathBehaviour.type == DragonBreathBehaviour.Type.Standard;
	}

	/// <summary>
	/// Determines whether this instance is super fury on.
	/// </summary>
	/// <returns><c>true</c> if this instance is super fury on; otherwise, <c>false</c>.</returns>
	public bool IsMegaFuryOn() {

		return m_breathBehaviour.IsFuryOn() && m_breathBehaviour.type == DragonBreathBehaviour.Type.Mega;
	}

	public void OnLeavingArea(float estimatedLeavingTime){
		PauseEating();
	}

	public void OnEnteringArea(){
		TryResumeEating();
	}

	protected bool CanIResumeEating()
	{
		bool ret = true;
		if ( m_breathBehaviour.IsFuryOn() || BeingLatchedOn() || !m_dragonMotion.CanIResumeEating())
			ret = false;
		return ret;
	}

	public void OnGameEnded() {
			if (m_form == Form.MUMMY) {
					EndMummyPower();
			}
	}

	private void OnEntityDestroyed(Transform _t, IEntity _e, Reward _reward) {
		if (_reward.health >= 0) {
			AddLife(_reward.health, DamageType.NONE, _t);
		}
		AddEnergy(_reward.energy);
	}

	/// <summary>
	/// Compute the health modifier to be applied based on current health percentage.
	/// </summary>
	/// <returns>The health modifier to be applied. <c>null</c> if none.</returns>
	public DragonHealthModifier ComputeHealthModifier() {
		// Modifiers are sorted, so this should work
		for(int i = 0; i < m_healthModifiers.Length; i++) {
			if(m_health < m_healthMax * m_healthModifiers[i].threshold) {
				return m_healthModifiers[i];
			}
		}
		return null;
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
	public Vector3 MoveToSpawnPoint(bool _levelEditor) {
		// Use level editor's spawn point or try to use specific's dragon spawn point?
		GameObject spawnPointObj = null;
		if(_levelEditor) {
			string selectedSP = LevelEditor.LevelEditor.settings.spawnPoint;
			if (!string.IsNullOrEmpty(selectedSP)) {
				spawnPointObj = GameObject.Find(selectedSP);
			}

			if (spawnPointObj == null) {
				spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + LevelEditor.LevelTypeSpawners.LEVEL_EDITOR_SPAWN_POINT_NAME);
				if (spawnPointObj == null) {
					spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + data.def.sku);
				}
			}
		} else {
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + data.def.sku);
		}

		// If we couldn't find a valid spawn point, try to find a generic one
		if(spawnPointObj == null) {
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME);
		}

		// Move to position
		if(spawnPointObj != null) {
			m_dragonMotion.MoveToSpawnPosition(spawnPointObj.transform.position);
			return spawnPointObj.transform.position;
		}

		return GameConstants.Vector3.zero;
	}


	/// <summary>
	/// Search for a valid starting point.
	/// </summary>
	/// <returns>Returns the starting position.</returns>
	public Vector3 StartIntroMovement( bool useLevelEditor = false )
	{
        if(m_dragonEatBehaviour != null)
		    m_dragonEatBehaviour.enabled = true;
		GameObject spawnPointObj = null;
		
		if(useLevelEditor) {
			string selectedSP = LevelEditor.LevelEditor.settings.spawnPoint;
			if (!string.IsNullOrEmpty(selectedSP)) {
				spawnPointObj = GameObject.Find(selectedSP);
			}

			if (spawnPointObj == null) {
				spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + LevelEditor.LevelTypeSpawners.LEVEL_EDITOR_SPAWN_POINT_NAME);
				if (spawnPointObj == null) {
					spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + data.def.sku);
				}
			}
		} else {
			// maybe we are inside a tournament
			if (HDLiveDataManager.tournament.isActive) {
				HDTournamentDefinition tournamentDef = HDLiveDataManager.tournament.tournamentData.tournamentDef;
				string selectedSP = tournamentDef.m_goal.m_spawnPoint;
				if (!string.IsNullOrEmpty(selectedSP)) {
					spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + selectedSP);
				}
			}
			
			if (spawnPointObj == null) {
				spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME + "_" + data.def.sku);
			}
		}
		// If we couldn't find a valid spawn point, try to find a generic one
		if(spawnPointObj == null) {
			spawnPointObj = GameObject.Find(LevelEditor.LevelTypeSpawners.DRAGON_SPAWN_POINT_NAME);
		}

		if(spawnPointObj != null)
		{
			Vector3 introPos = spawnPointObj.transform.position;
			m_dragonMotion.StartIntroMovement(introPos);

			return introPos;
		}

		return GameConstants.Vector3.zero;
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

    public bool IsIntroMovement(){
        return m_dragonMotion.state == DragonMotion.State.Intro;
    }

	/// <summary>
	/// Whether the dragon can take damage or not.
	/// </summary>
	/// <returns><c>true</c> if the dragon currently is invulnerable; otherwise, <c>false</c>.</returns>
	public bool IsInvulnerable() {
		if (m_modInvulnerable) return true;

		// After revive, we're invulnerable
		if(m_invulnerableAfterReviveTimer > 0) return true;

		// During fire, we're invulnerable
		if(m_breathBehaviour.IsFuryOn()) return true;

		if ( m_changingArea ) return true;

		if ( m_superSizeInvulnerable ) return true;

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
	private void OnLevelUp(IDragonData _data) {
		// Assume it's this dragon
		// Make sure the dragon has the scale according to its level
		// gameObject.transform.localScale = Vector3.one * m_defaultSize;
		gameObject.transform.localScale = Vector3.one * m_defaultSize * m_superSizeSize;
		if (m_breathBehaviour.IsFuryOn())
			m_breathBehaviour.RecalculateSize();

		SetHealthBonus( m_healthBonus );
		SetBoostBonus( m_energyBonus );
		//TONI
		m_dragonMotion.RecalculateDragonForce();
	}

	void OnPrewardmFuryRush(DragonBreathBehaviour.Type type, float duration)
	{
		PauseEating();
	}

	void OnFuryToggled( bool toogle, DragonBreathBehaviour.Type type)
	{
		if (toogle)
		{
			PauseEating();
		}
		else
		{
			TryResumeEating();
		}
	}

	public void LoseShield( DamageType _type, Transform _origin )
	{
		if ( m_shield.ContainsKey( _type ) )
		{
			m_shield[_type] = m_shield[_type] - 1;
			// Checks if needs activation
			if ( _type == DamageType.POISON )
			{
				if ( m_shieldTimers.ContainsKey( _type ) )
				{
					m_shieldTimers[_type] = Time.time;
				}
				else
				{
					m_shieldTimers.Add(_type, Time.time);
				}
			}

			// event shield lost
			Messenger.Broadcast<DamageType, Transform>(MessengerEvents.PLAYER_LOST_SHIELD, _type, _origin);
		}
	}

	public bool HasShield( DamageType _type )
	{
		if ( m_shield.ContainsKey( _type ) )
		{
			return m_shield[_type] > 0;
		}
		return false;
	}

	public bool HasShieldActive( DamageType _type )
	{
		if ( m_shieldTimers.ContainsKey( _type ) )
		{
			return Time.time <= m_shieldTimers[_type] + m_shieldsDuration;
		}
		return false;
	}

	public int GetReminingLives()
	{
		return m_freeRevives;
	}

    public bool HasMummyPowerAvailable() {
        return m_mummyPowerStacks > 0;
    }

    public bool CanUseMummyPower() {
        return !CanUseFreeRevives() && HasMummyPowerAvailable();
    }

    public bool CanUseFreeRevives() {
        return GetReminingLives() > 0;
    }

	/// <summary>
	/// Gets the tier when breaking. Because we can have the "Destroy" power up wich increases the
	/// tier on the dragon breaking things we have this function to ask on the proper places
	/// </summary>
	/// <returns>The tier when breaking.</returns>
	public DragonTier GetTierWhenBreaking()
	{
		int ret = (int)m_data.tier + m_tierIncreaseBreak;
		if ( ret >= (int)DragonTier.COUNT )
		{
			ret = (int)(DragonTier.COUNT - 1);
		}
		return (DragonTier)ret;
	}

	public void SetOnBreakIncrease( int increase )
	{
		m_tierIncreaseBreak = increase;
	}

	// Increases health max by value where value is a tant per cent
	public void SetHealthBonus( float value )
	{
		m_healthBase = m_data.maxHealth;
		m_healthBonus = value;
		m_healthMax = m_healthBase + (m_healthBonus / 100.0f * m_healthBase);

        if (m_form == Form.NORMAL)
            m_health = m_healthMax;
	}

	public void AddHealthBonus(float value)
	{
		m_healthBonus += value;
		SetHealthBonus( m_healthBonus );
	}

	public void SetBoostBonus( float value )
	{
		m_energyBase = m_data.baseEnergy;
		m_energyBonus = value;
		m_energyMax = m_energyBase + ( m_energyBonus / 100.0f * m_energyBase );
		m_energy = m_energyMax;
	}

	public void AddBoostBonus( float value )
	{
		m_energyBonus += value;
		SetBoostBonus( m_energyBonus );
	}

    public void AddMummyPower(int _stacks) {
        m_mummyPowerStacks += _stacks;
    }

	public void AddFreeRevives( int revives )
	{
		m_freeRevives += revives;
	}

	public void AddShields( DamageType _type, int _numHits )
	{
		if ( m_shield.ContainsKey( _type ) )
		{
			m_shield[_type] += _numHits;
		}
		else
		{
			m_shield.Add( _type, _numHits );
		}
	}



	public void StartLatchedOn()
	{
		m_numLatching++;
		if ( m_numLatching == 1 )
		{
			m_dragonMotion.StartLatchedOnMovement();
			PauseEating();
		}
	}

	public void EndLatchedOn()
	{
		m_numLatching--;
		if ( m_numLatching == 0)
		{
			m_dragonMotion.EndLatchedOnMovement();
			TryResumeEating();
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

	public void SetSuperSize( float size )
	{
		m_superSizeTarget = size;
		m_superSizeStart = m_superSizeSize;
		m_superSizeDuration = m_superSizeTimer = 0.5f;
	}

	public void OverrideSize( float size )
	{
		m_defaultSize = size;
		gameObject.transform.localScale = Vector3.one * m_defaultSize;
	}
	public void PauseEating()
	{
        if (m_dragonEatBehaviour != null)
		    m_dragonEatBehaviour.PauseEating();
	}

	public void TryResumeEating()
	{
		if (CanIResumeEating() && m_dragonEatBehaviour != null)
			m_dragonEatBehaviour.ResumeEating();
	}

	public bool IsBreakingMovement()
	{
		bool ret = false;
		if ( m_dragonBoostBehaviour.IsBoostActive() || m_dragonMotion.IsBreakingMovement() )
		{
			ret = true;
		}
		return ret;
	}

}

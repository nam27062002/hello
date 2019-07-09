using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Entity : IEntity, IBroadcastListener {
	private static readonly string RESOURCES_DIR = "Game/Entities";

	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	// Exposed to inspector
	[EntitySkuList]
	[SerializeField] protected string m_sku;
	public override string sku { get { return m_sku; } }

	[SerializeField] private bool m_hideNeedTierMessage = false;
	public bool hideNeedTierMessage{
		get{ return m_hideNeedTierMessage; }
	}

	public float lastTargetedTime{get;set;}

	[SerializeField] private bool m_dieOutsideFrustum = true;
	public bool dieOutsideFrustum {
		get { return m_dieOutsideFrustum; }
		set { m_dieOutsideFrustum = value; }
	}

	/************/

	protected CircleArea2D m_bounds;
	public override CircleArea2D circleArea { get{ return m_bounds; } }

	protected Reward m_reward;
	public Reward reward { get { return m_reward; }}
	public override int score { get { return m_reward.score; } }

	private float m_goldenChance = 0f;
	public float goldenChance { get { return m_goldenChance; }}

	private float m_pcChance = 0f;
	public float pcChance { get { return m_pcChance; }}

	private bool m_isBurnable;
	private DragonTier m_burnableFromTier = 0;
	public DragonTier burnableFromTier { get { return m_burnableFromTier; } }

	private bool m_isEdibleByZ;
	private bool m_isEdible;

	private DragonTier m_edibleFromTier = 0;
	public override DragonTier edibleFromTier { get { return m_edibleFromTier; } set { m_edibleFromTier = value; } }

	private bool m_canBeGrabbed;
	public bool canBeGrabbed { get{ return m_canBeGrabbed; } }
	private DragonTier m_grabFromTier = 0;
	public DragonTier grabFromTier { get { return m_grabFromTier; } }

	private bool m_canBeLatchedOn;
	public bool canBeLatchedOn { get{ return m_canBeLatchedOn; } }
	private DragonTier m_latchFromTier = 0;
	public DragonTier latchFromTier { get { return m_latchFromTier; } }

	private FeedbackData m_feedbackData = new FeedbackData();
	public FeedbackData feedbackData { get { return m_feedbackData; }}

	private bool m_isPC = false;
	public bool isPC { get { return m_isPC; } }

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private ISpawner m_spawner;
    private bool m_spawned = false;

    private float m_checkOnScreenTimer = 0;

	protected GameCamera m_newCamera;


	//-----------------------------------------------
	// Power up static values
	//-----------------------------------------------
	private static bool sm_goldenModifier = false;
	private static float m_powerUpSCMultiplier = 0;	// Soft currency power up multiplier
	private static float m_powerUpScoreMultiplier = 0;	// Score power up multiplier
	private static float m_powerUpXpMultiplier = 0;	// XP power up multiplier

	/************/
	protected override void Awake() {
		base.Awake();
		// [AOC] Obtain the definition and initialize important data
		InitFromDef();
		m_bounds = GetComponentInChildren<CircleArea2D>();
		Broadcaster.AddListener(BroadcastEventType.APPLY_ENTITY_POWERUPS, this);
	}

	void OnDestroy() {
		if (ApplicationManager.IsAlive) {
			if (EntityManager.instance != null) {
				EntityManager.instance.UnregisterEntity (this);
			}
			Broadcaster.RemoveListener (BroadcastEventType.APPLY_ENTITY_POWERUPS, this);
		}
	}
    
    void OnDisable()
    {
        if ( Application.isPlaying && FreezingObjectsRegistry.instance != null )
        {
            FreezingObjectsRegistry.instance.UnregisterEntity( this );
        }
    }

    public virtual void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.APPLY_ENTITY_POWERUPS:
            {
                ApplyPowerUpMultipliers();
            }break;
        }
    }
    

	protected void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ENTITIES, sku);

		if (m_def != null) {
			// Cache some frequently accessed values from the definition for faster access
			// Reward
			// m_reward.score = m_def.GetAsInt("rewardScore");
			// m_reward.coins = m_def.GetAsInt("rewardCoins");
			// m_reward.xp = m_def.GetAsFloat("rewardXp");

			m_reward.pc = m_def.GetAsInt("rewardPC");
			m_reward.health = m_def.GetAsFloat("rewardHealth");
			m_reward.energy = m_def.GetAsFloat("rewardEnergy");
			m_reward.fury = m_def.GetAsFloat("rewardFury", 0);

			m_reward.origin = m_def.Get("sku");
			m_reward.category = m_def.Get("category");

			OnRewardCreated();


			// Simple data
			m_goldenChance = m_def.GetAsFloat("goldenChance");
			if (sm_goldenModifier && m_goldenChance > 0)
				m_goldenChance = 1f;

			m_pcChance = m_def.GetAsFloat("pcChance");

			m_isBurnable = m_def.GetAsBool("isBurnable");
			m_burnableFromTier = DragonTierGlobals.GetFromInt(m_def.GetAsInt("burnableFromTier"));

			m_isEdible = m_def.GetAsBool("isEdible");
			m_edibleFromTier = DragonTierGlobals.GetFromInt(m_def.GetAsInt("edibleFromTier"));

			m_canBeGrabbed = m_def.GetAsBool("canBeGrabed", false);
			m_grabFromTier = DragonTierGlobals.GetFromInt(m_def.GetAsInt("grabFromTier"));

			m_canBeLatchedOn = m_def.GetAsBool("canBeLatchedOn", false);
			m_latchFromTier = DragonTierGlobals.GetFromInt(m_def.GetAsInt("latchOnFromTier"));

			m_maxHealth = m_def.GetAsFloat("maxHealth", 1);
			if (InstanceManager.player != null) {
				m_maxHealth *= (1f + (m_def.GetAsFloat("healthScalePerDragonTier", 0f) * (int)InstanceManager.player.data.tier));
			}

			// Feedback data
			m_feedbackData.InitFromDef(m_def);

			ApplyPowerUpMultipliers();
		}
	}
	

	override public void Spawn(ISpawner _spawner) {        
        base.Spawn(_spawner);

		m_spawner = _spawner;

		if (InstanceManager.player != null) {
			DragonTier tier = InstanceManager.player.data.tier;
			m_isPC = ((edibleFromTier <= tier) && (Random.Range(0f, 1f) <= pcChance));
		} else {
			m_isPC = false;
		}

		m_isOnScreen = false;
		m_checkOnScreenTimer = 0;

		m_health = m_maxHealth;

		m_isEdibleByZ = true;

		m_newCamera = InstanceManager.gameCamera;
        
        // Register to freeze
        FreezingObjectsRegistry.instance.RegisterEntity(this);

        m_spawned = true;
    }

    public void SetFreezingLevel(float freezingMultiplier) 
    {
        if ( m_pilot )
            m_pilot.SetFreezeFactor(1.0f - freezingMultiplier);
        // float freezingLevel = (freezingMultiplier - 1.0f) / (FreezingObjectsRegistry.m_minFreezeSpeedMultiplier);
        if ( m_viewControl != null)
            m_viewControl.Freezing(freezingMultiplier);
    }
        
	public override void SetGolden(Spawner.EntityGoldMode _mode) {
		switch (_mode) {
			case Spawner.EntityGoldMode.Normal:
				m_isGolden = false;
				break;

			case Spawner.EntityGoldMode.Gold:
				m_isGolden = true;
				break;

			case Spawner.EntityGoldMode.ReRoll:
				m_isGolden = ((edibleFromTier <= InstanceManager.player.data.tier) && (Random.Range(0f, 1f) <= goldenChance));
				break;
		}
	}

	public void ForceGolden(){
		m_spawner.ForceGolden( this );
		m_viewControl.ForceGolden();
	}

    public override void Disable(bool _destroyed) {		
        // Remove from freeze
        if ( FreezingObjectsRegistry.instance != null )
        {
            FreezingObjectsRegistry.instance.UnregisterEntity( this );
        }
        
		if (m_viewControl != null)
			m_viewControl.PreDisable();
		
		base.Disable(_destroyed);

		if (m_spawner != null) {
			m_spawner.RemoveEntity(this, _destroyed);
		}

        m_spawned = false;		
    }

    /// <summary>
    /// Get a Reward struct initialized with the reward to be given when killing this
    /// prey, taking in account its golden/pc chances and status.
    /// </summary>
    /// <returns>The reward to be given to the player when killing this unit.</returns>
    /// <param name="_burnt">Set to <c>true</c> if the cause of the death was fire - affects the reward.</param>
    public override Reward GetOnKillReward(DyingReason _reason) {
		// Create a copy of the base rewards and tune them
		Reward newReward = reward;	// Since it's a struct, this creates a new copy rather than being a reference

		// Give coins? True if the entity was golden or has been burnt
		if(!m_isGolden && !InstanceManager.player.breathBehaviour.IsFuryOn()) {
			newReward.coins = 0;
		}

		// Give PC?
		if (!m_isPC) {
			newReward.pc = 0;
		}

        if (m_machine.IsBubbled()) {
            newReward.energy *= 2f;
        }

		return newReward;
	}

	public bool IsBurnable() {
		return m_isBurnable;
	}

	public bool IsBurnable(DragonTier _tier) {
		return IsBurnable() && (m_burnableFromTier <= _tier);
	}

	public bool IsEdible() {
		return m_isEdibleByZ && m_isEdible;
	}

	public bool IsEdible(DragonTier _tier) {
		return m_isEdibleByZ && m_isEdible && (m_edibleFromTier <= _tier);
	}

	public bool CanBeHolded(DragonTier _tier) {
		return m_isEdibleByZ && (CanBeGrabbed(_tier) || CanBeLatchedOn(_tier));
	}

	public bool CanBeGrabbed( DragonTier _tier ){
		return m_isEdibleByZ && m_canBeGrabbed && m_grabFromTier <= _tier;
	}

	public bool CanBeLatchedOn( DragonTier _tier){
		return m_isEdibleByZ && m_canBeLatchedOn && m_latchFromTier <= _tier;
	}

	public bool hasToShowTierNeeded(DragonTier _tier) {
		return m_isEdible && !((m_edibleFromTier <= _tier) || (m_canBeGrabbed && m_grabFromTier <= _tier) || (m_canBeLatchedOn && m_latchFromTier <= _tier));
	}

	public bool CanBeSmashed( DragonTier _tier ) {
		return IsEdible(_tier) || CanBeHolded( _tier );
	}

	public DragonTier MinSmashTier(){
		DragonTier ret = DragonTier.COUNT;
		if ( m_isEdible && m_isEdibleByZ ){
			ret = m_edibleFromTier;
		} else if ( m_canBeGrabbed && m_canBeLatchedOn) {
			if ( m_grabFromTier < m_latchFromTier ){
				ret = m_grabFromTier;
			}else{
				ret = m_latchFromTier;
			}
		}else if ( m_canBeGrabbed ){
			ret = m_grabFromTier;
		}else if ( m_canBeLatchedOn ){
			ret = m_latchFromTier;
		}
		return ret;
	}

	public bool IntersectsWith(Vector2 _center, float _radius) {
		if (m_bounds != null) {
			return m_bounds.Overlaps(_center, _radius);
		} 

		// return _r.Contains(transform.position);
		float sqrMagnitude = (_center - (Vector2)transform.position).sqrMagnitude;
		return ( sqrMagnitude <= _radius * _radius );	
	}


    /*****************/
    // Private stuff //
    /*****************/

    // Update is called once per frame
    public override void CustomUpdate() { 
    //void Update () {
		base.CustomUpdate();
        if (m_spawned) {
            m_checkOnScreenTimer -= Time.deltaTime;
            if (m_checkOnScreenTimer <= 0) {
                if (m_newCamera != null) m_isOnScreen = m_newCamera.IsInsideActivationMinArea(transform.position);
                m_checkOnScreenTimer = 0.5f;
            }

			m_isEdibleByZ = m_machine.position.z <= 15f;
        }
	}

    public override bool CanDieOutsideFrustrum() {
        return m_spawned && m_dieOutsideFrustum;
    }   

	/**************************************************************/
	// STATIC UTILS MIGRATED FROM LEVEL EDITOR'S SECTION SPAWNERS //
	/**************************************************************/
	public static List<string> Entities_GetFileNames()
	{
		List<string> returnValue = new List<string>();

		string dirPath = Application.dataPath + "/Resources/" + RESOURCES_DIR;
		DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
		Entities_AddFileNamesInDirectory(dirInfo, returnValue);

		return returnValue;
	}

	private static void Entities_AddFileNamesInDirectory(DirectoryInfo dirInfo, List<string> list)
	{
		if (list != null)
		{                             
			// Format dir path to something that Unity Resources API understands
			string resourcePath = dirInfo.FullName.Replace('\\', '/'); // [AOC] Windows uses backward slashes, which Unity doesn't recognize
			resourcePath = resourcePath.Substring(resourcePath.IndexOf(RESOURCES_DIR));
			string[] tokens = resourcePath.Split('/');
			if (tokens != null && tokens.Length > 0)
			{
				resourcePath = tokens[tokens.Length - 1] + "/";
			}
			else
			{
				resourcePath = "";
			}                

			// Get all prefabs in the target directory, but don't include subdirectories
			FileInfo[] files = dirInfo.GetFiles("*.prefab");
			int i;
			int count = files.Length;
			for (i = 0; i < count; i++)
			{
				list.Add(resourcePath + Path.GetFileNameWithoutExtension(files[i].Name));
			}

			// Iterate subdirectories and create group for each of them as well!
			DirectoryInfo[] subdirs = dirInfo.GetDirectories();
			for (i = 0; i < subdirs.Length; i++)
			{
				// Ignore "Assets" directories
				if (subdirs[i].Name != "Assets")
				{
					Entities_AddFileNamesInDirectory(subdirs[i], list);
				}
			}
		}
	}

	void ApplyPowerUpMultipliers()
	{
		m_reward.score = m_def.GetAsInt("rewardScore");
		m_reward.score += Mathf.FloorToInt((m_reward.score * m_powerUpScoreMultiplier) / 100.0f);

		m_reward.coins = m_def.GetAsInt("rewardCoins");
		m_reward.coins += ((m_reward.coins * m_powerUpSCMultiplier) / 100.0f);

		m_reward.xp = m_def.GetAsFloat("rewardXp");
		m_reward.xp += (m_reward.xp * m_powerUpXpMultiplier) / 100.0f;

        OnRewardCreated();
    }

    protected virtual void OnRewardCreated() {}


	public static void AddSCMultiplier( float value )
	{
		m_powerUpSCMultiplier += value;
	}

	public static void AddScoreMultiplier( float value )
	{
		m_powerUpScoreMultiplier += value;
	}

	public static void AddXpMultiplier( float value )
	{
		m_powerUpXpMultiplier += value;
	}

	public static void SetGoldenModifier(bool _value) {
		sm_goldenModifier = _value;
	}
}

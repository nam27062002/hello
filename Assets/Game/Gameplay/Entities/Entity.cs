using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Serialization;

public class Entity : IEntity {
	private static readonly string RESOURCES_DIR = "Game/Entities";

	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	// Exposed to inspector
	[EntitySkuList]
	[SerializeField] private string m_sku;
	public override string sku { get { return m_sku; } }

	[SerializeField] private bool m_dieOutsideFrustum = true;
	public bool dieOutsideFrustum
	{
		get{return m_dieOutsideFrustum;}
		set{m_dieOutsideFrustum = value;}
	}

	/************/

	private CircleArea2D m_bounds;
	public override CircleArea2D circleArea { get{ return m_bounds; } }

	private Reward m_reward;
	public Reward reward { get { return m_reward; }}
	public override int score { get { return m_reward.score; } }

	private float m_goldenChance = 0f;
	public float goldenChance { get { return m_goldenChance; }}

	private float m_pcChance = 0f;
	public float pcChance { get { return m_pcChance; }}

	private bool m_isBurnable;
	private DragonTier m_burnableFromTier = 0;
	public DragonTier burnableFromTier { get { return m_burnableFromTier; } }

	private bool m_isEdible;
	private DragonTier m_edibleFromTier = 0;
	public DragonTier edibleFromTier { get { return m_edibleFromTier; } }

	private bool m_canBeGrabbed;
	private DragonTier m_grabFromTier = 0;

	private bool m_canBeLatchedOn;
	private DragonTier m_latchFromTier = 0;

	private FeedbackData m_feedbackData = new FeedbackData();
	public FeedbackData feedbackData { get { return m_feedbackData; }}

	private bool m_isGolden = false;
	public bool isGolden { get { return m_isGolden; } }

	private bool m_isPC = false;
	public bool isPC { get { return m_isPC; } }

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private ISpawner m_spawner;
    private bool m_spawned = false;

    private float m_checkOnScreenTimer = 0;

	private GameCamera m_newCamera;


	//-----------------------------------------------
	// Power up static values
	//-----------------------------------------------

	private static float m_powerUpSCMultiplier = 0;	// Soft currency power up multiplier
	private static float m_powerUpScoreMultiplier = 0;	// Score power up multiplier
	private static float m_powerUpXpMultiplier = 0;	// XP power up multiplier

	/************/
	protected override void Awake() {
		base.Awake();
		// [AOC] Obtain the definition and initialize important data
		InitFromDef();
		m_bounds = GetComponentInChildren<CircleArea2D>();
		Messenger.AddListener(GameEvents.APPLY_ENTITY_POWERUPS, ApplyPowerUpMultipliers);
	}

	void OnDestroy() {
		if (EntityManager.instance != null) {
			EntityManager.instance.UnregisterEntity(this);
		}
		Messenger.RemoveListener(GameEvents.APPLY_ENTITY_POWERUPS, ApplyPowerUpMultipliers);
	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ENTITIES, sku);

		// Cache some frequently accessed values from the definition for faster access
		// Reward
		// m_reward.score = m_def.GetAsInt("rewardScore");
		// m_reward.coins = m_def.GetAsInt("rewardCoins");
		// m_reward.xp = m_def.GetAsFloat("rewardXp");
		ApplyPowerUpMultipliers();
		m_reward.pc = m_def.GetAsInt("rewardPC");
		m_reward.health = m_def.GetAsFloat("rewardHealth");
		m_reward.energy = m_def.GetAsFloat("rewardEnergy");
		// m_reward.fury = m_def.GetAsFloat("rewardFury");

		m_reward.alcohol = m_def.GetAsFloat("alcohol",0);
		m_reward.origin = m_def.Get("sku");
		m_reward.category = m_def.Get("category");

		// Simple data
		m_goldenChance = m_def.GetAsFloat("goldenChance");
		m_pcChance = m_def.GetAsFloat("pcChance");

		m_isBurnable = m_def.GetAsBool("isBurnable");
		m_burnableFromTier = (DragonTier)m_def.GetAsInt("burnableFromTier");

		m_isEdible = m_def.GetAsBool("isEdible");
		m_edibleFromTier = (DragonTier)m_def.GetAsInt("edibleFromTier");

		// m_canBeHolded = m_def.GetAsBool("canBeHolded", false);
		// m_holdFromTier = (DragonTier)m_def.GetAsInt("holdFromTier");

		m_canBeGrabbed = m_def.GetAsBool("canBeGrabed", false);
		m_grabFromTier = (DragonTier)m_def.GetAsInt("grabFromTier");

		m_canBeLatchedOn = m_def.GetAsBool("canBeLatchedOn", false);
		m_latchFromTier = (DragonTier)m_def.GetAsInt("latchOnFromTier");

		m_maxHealth = m_def.GetAsFloat("maxHealth", 1);

		// Feedback data
		m_feedbackData.InitFromDef(m_def);
	}

	override public void Spawn(ISpawner _spawner) {        
        base.Spawn(_spawner);

		m_spawner = _spawner;

		if (InstanceManager.player != null) {
			DragonTier tier = InstanceManager.player.data.tier;
			m_isGolden = ((edibleFromTier <= tier) && (Random.Range(0f, 1f) <= goldenChance));
			m_isPC = ((edibleFromTier <= tier) && (Random.Range(0f, 1f) <= pcChance));
		} else {
			m_isGolden = false;
			m_isPC = false;
		}

		m_isOnScreen = false;
		m_checkOnScreenTimer = 0;

		m_health = m_maxHealth;

		m_newCamera = InstanceManager.gameCamera;

        m_spawned = true;		
    }

    public override void Disable(bool _destroyed) {		
		if (m_viewControl != null)
			m_viewControl.PreDisable();
		
		base.Disable(_destroyed);

		if (m_spawner != null) {
			m_spawner.RemoveEntity(gameObject, _destroyed);
		}

        m_spawned = false;		
    }


    /// <summary>
    /// Get a Reward struct initialized with the reward to be given when killing this
    /// prey, taking in account its golden/pc chances and status.
    /// </summary>
    /// <returns>The reward to be given to the player when killing this unit.</returns>
    /// <param name="_burnt">Set to <c>true</c> if the cause of the death was fire - affects the reward.</param>
    public Reward GetOnKillReward(bool _burnt) {
		// Create a copy of the base rewards and tune them
		Reward newReward = reward;	// Since it's a struct, this creates a new copy rather than being a reference

		// Give coins? True if the entity was golden or has been burnt
		if(!m_isGolden && !_burnt) {
			newReward.coins = 0;
		}

		if (_burnt) {
			newReward.alcohol = 0;
		}

		// Give PC?
		if (!m_isPC) {
			newReward.pc = 0;
		}

		return newReward;
	}

	public bool IsBurnable() {
		return allowBurnable && m_isBurnable;
	}

	public bool IsBurnable(DragonTier _tier) {
		return IsBurnable() && (m_burnableFromTier <= _tier);
	}

	public bool IsEdible() {
		return allowEdible && m_isEdible;
	}

	public bool IsEdible(DragonTier _tier) {
		return IsEdible() && (m_edibleFromTier <= _tier);
	}

	public bool CanBeHolded(DragonTier _tier) {
		return allowEdible && (CanBeGrabbed(_tier) || CanBeLatchedOn(_tier));
	}

	public bool CanBeGrabbed( DragonTier _tier ){
		return allowEdible && m_canBeGrabbed && m_grabFromTier <= _tier;
	}

	public bool CanBeLatchedOn( DragonTier _tier){
		return allowEdible && m_canBeLatchedOn && m_latchFromTier <= _tier;
	}

	override public bool CanBeSmashed()
	{
		return true;
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
		m_reward.coins += Mathf.FloorToInt((m_reward.coins * m_powerUpSCMultiplier) / 100.0f);

		m_reward.xp = m_def.GetAsFloat("rewardXp");
		m_reward.xp += (m_reward.xp * m_powerUpXpMultiplier) / 100.0f;
	}


	public static void ResetSCMuliplier()
	{
		m_powerUpSCMultiplier = 0;
	}

	public static void AddSCMultiplier( float value )
	{
		m_powerUpSCMultiplier += value;
	}

	public static void ResetScoreMultiplier()
	{
		m_powerUpScoreMultiplier = 0;
	}

	public static void AddScoreMultiplier( float value )
	{
		m_powerUpScoreMultiplier += value;
	}

	public static void ResetXpMultiplier()
	{
		m_powerUpXpMultiplier = 0;
	}

	public static void AddXpMultiplier( float value )
	{
		m_powerUpXpMultiplier += value;
	}
}

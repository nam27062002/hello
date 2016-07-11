using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Entity : MonoBehaviour, ISpawnable {
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	// Exposed to inspector
	[FormerlySerializedAs("m_typeID")]
	[EntitySkuList]
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; } }


	/************/
	private DefinitionNode m_def;
	public DefinitionNode def { get { return m_def; } }

	private CircleArea2D m_bounds;
	public CircleArea2D circleArea { get{ return m_bounds; } }

	private Reward m_reward;
	public Reward reward { get { return m_reward; }}

	private float m_goldenChance = 0f;
	public float goldenChance { get { return m_goldenChance; }}

	private float m_pcChance = 0f;
	public float pcChance { get { return m_pcChance; }}

	private bool m_isEdible;
	private DragonTier m_edibleFromTier = 0;
	public DragonTier edibleFromTier { get { return m_edibleFromTier; } }

	private bool m_canBeHolded;
	private DragonTier m_holdFromTier = 0;
	public DragonTier holdFromTier { get { return m_holdFromTier; } }

	private FeedbackData m_feedbackData = new FeedbackData();
	public FeedbackData feedbackData { get { return m_feedbackData; }}

	// Health
	private float m_maxHealth;
	private float m_health;
	public float health { get { return m_health; } set { m_health = value; } }

	private bool m_isGolden = false;
	public bool isGolden { get { return m_isGolden; } }

	private bool m_isOnScreen = false;
	public bool isOnScreen { get { return m_isOnScreen; } }


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private Spawner m_spawner;

	private float m_checkOnScreenTimer = 0;

	private bool m_givePC = false;

	private GameCameraController m_camera;



	/************/
	void Awake() {
		// [AOC] Obtain the definition and initialize important data
		InitFromDef();

	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ENTITIES, sku);

		// Cache some frequently accessed values from the definition for faster access
		// Reward
		m_reward.score = m_def.GetAsInt("rewardScore");
		m_reward.coins = m_def.GetAsInt("rewardCoins");
		m_reward.pc = m_def.GetAsInt("rewardPC");
		m_reward.health = m_def.GetAsFloat("rewardHealth");
		m_reward.energy = m_def.GetAsFloat("rewardEnergy");
		// m_reward.fury = m_def.GetAsFloat("rewardFury");
		m_reward.xp = m_def.GetAsFloat("rewardXp");
		m_reward.origin = m_def.Get("sku");

		// Simple data
		m_goldenChance = m_def.GetAsFloat("goldenChance");
		m_pcChance = m_def.GetAsFloat("pcChance");

		m_isEdible = m_def.GetAsBool("isEdible");
		m_edibleFromTier = (DragonTier)m_def.GetAsInt("edibleFromTier");

		m_canBeHolded = m_def.GetAsBool("canBeHolded", false);
		m_holdFromTier = (DragonTier)m_def.GetAsInt("holdFromTier");

		m_maxHealth = m_def.GetAsFloat("maxHealth", 1);

		// Feedback data
		m_feedbackData.InitFromDef(m_def);
	}

	// Use this for initialization
	void Start () {
		m_camera = GameObject.Find("PF_GameCamera").GetComponent<GameCameraController>();
		m_bounds = GetComponentInChildren<CircleArea2D>();
	}

	void OnEnable() {
		EntityManager.instance.Register(this);
	}

	void OnDisable() {
		if (EntityManager.instance != null)
			EntityManager.instance.Unregister(this);
	}

	public void Spawn(Spawner _spawner) {
		m_spawner = _spawner;

		DragonTier tier = InstanceManager.player.data.tier;
		m_isGolden = ((edibleFromTier <= tier) && (Random.Range(0f, 1f) <= goldenChance));

		// [AOC] TODO!! Implement PC shader, implement PC reward feedback
		m_givePC = (Random.Range(0f, 1f) <= pcChance);

		m_isOnScreen = false;
		m_checkOnScreenTimer = 0;

		m_health = m_maxHealth;
	}

	public void Disable(bool _destroyed) {
		m_spawner.RemoveEntity(gameObject, _destroyed);
		gameObject.SetActive(false);
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

		// Give PC?
		if(!m_givePC) {
			newReward.pc = 0;
		}

		return newReward;
	}

	public void Damage(float damage)  {
		m_health -= damage;
	}

	public bool IsEdible(DragonTier _tier) {
		return m_isEdible && (m_edibleFromTier <= _tier);
	}

	public bool CanBeHolded(DragonTier _tier) {
		return m_canBeHolded && (m_holdFromTier <= _tier);
	}

	public bool IntersectsWith(Vector2 _center, float _radius) {
		Vector2 source = transform.position;

		if (m_bounds != null) {
			source = m_bounds.center;
		}

		float sqrMagnitude = (_center - source).sqrMagnitude;
		return (sqrMagnitude <= _radius * _radius);
	}


	/*****************/
	// Private stuff //
	/*****************/

	// Update is called once per frame
	void Update () {
		m_checkOnScreenTimer -= Time.deltaTime;
		if (m_checkOnScreenTimer <= 0) {	
			m_isOnScreen = m_camera.IsInsideActivationMinArea(transform.position);
			m_checkOnScreenTimer = 0.5f;
		}
	}

	void LateUpdate() {
		// check camera to destroy this entity if it is outside view area
		if (m_camera.IsInsideDeactivationArea(transform.position)) {
			if (m_spawner) {
				Disable(false);
			}
		}
	}
}

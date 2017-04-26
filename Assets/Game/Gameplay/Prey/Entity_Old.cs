using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class Entity_Old : Initializable {


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	// Exposed to inspector
	[FormerlySerializedAs("m_typeID")]
	[EntitySkuList]
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; } }

	// Reading frequently from the definition is expensive, try to use the below cached values rather
	private DefinitionNode m_def = null;
	public DefinitionNode def { 
		get { 
			if(m_def == null) InitFromDef();
			return m_def;
		}
	}

	// Cache some frequently accessed values from the definition for faster access
	private Reward m_reward;
	public Reward reward { get { return m_reward; }}

	private float m_goldenChance = 0f;
	public float goldenChance { get { return m_goldenChance; }}

	private float m_pcChance = 0f;
	public float pcChance { get { return m_pcChance; }}

	private bool m_isEdible = true;
	public bool isEdible { get { return m_isEdible; }}

	private DragonTier m_edibleFromTier = 0;
	public DragonTier edibleFromTier { get { return m_edibleFromTier; } }

	private DragonTier m_holdFromTier = 0;
	public DragonTier holdFromTier { get { return m_holdFromTier; } }

	private bool m_canBeHolded;
	public bool canBeHolded { get { return m_canBeHolded; } }

	private float m_biteResistance = 1f;
	public float biteResistance { get { return m_biteResistance; }}

	// Health
	private float m_maxHealth;
	private float m_health;
	public float health
	{
		get { return m_health; }
		set { m_health = value; }
	}

	private FeedbackData m_feedbackData = new FeedbackData();
	public FeedbackData feedbackData { get { return m_feedbackData; }}

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private bool m_isGolden = false;
	private bool m_givePC = false;

	private CircleArea2D m_bounds;
	public CircleArea2D circleArea
	{
		get{ return m_bounds; }
	}

	private Dictionary<int, Material[]> m_materials;

	[Range(0f, 100f)]
	public float m_onAppearSoundProbability = 40.0f;
	public List<string> m_onApprearSounds = new List<string>();
	private bool m_isOnScreen = false;
	private float m_checkOnScreenTimer = 0;


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake() {
		// [AOC] Obtain the definition and initialize important data
		InitFromDef();

		m_materials = new Dictionary<int, Material[]>();

		m_bounds = GetComponentInChildren<CircleArea2D>();

		// keep the original materials, sometimes it will become Gold!
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			m_materials[renderers[i].GetInstanceID()] = renderers[i].materials;
		}
	}

	/// <summary>
	/// Initialize important values from the definition.
	/// </summary>
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
		m_reward.category = m_def.Get("category");

		// Simple data
		m_goldenChance = m_def.GetAsFloat("goldenChance");
		m_pcChance = m_def.GetAsFloat("pcChance");
		m_isEdible = m_def.GetAsBool("isEdible");
		m_edibleFromTier = (DragonTier)m_def.GetAsInt("edibleFromTier");

		m_holdFromTier = (DragonTier)m_def.GetAsInt("holdFromTier");
		m_canBeHolded = m_def.GetAsBool("canBeHolded", false);
		m_maxHealth = m_def.GetAsFloat("maxHealth", 1);

		m_biteResistance = m_def.GetAsFloat("biteResistance");

		// Feedback data
		m_feedbackData.InitFromDef(m_def);
	}

	void Start()
	{
	}

	public override void Initialize() {
		//	m_health = m_maxHealth;		
		DragonTier tier = InstanceManager.player.data.tier;
		SetGolden((edibleFromTier <= tier) && (Random.Range(0f, 1f) <= goldenChance));

		// [AOC] TODO!! Implement PC shader, implement PC reward feedback
		m_givePC = (Random.Range(0f, 1f) <= pcChance);

		m_isOnScreen = false;
		m_checkOnScreenTimer = 0;
		m_health = m_maxHealth;
	}

/*	public void AddLife(float _offset) {
		m_health = Mathf.Min(m_maxHealth, Mathf.Max(0, m_health + _offset)); 
	}*/


	void Update()
	{
		if ( m_onApprearSounds.Count > 0 )
		{
			m_checkOnScreenTimer -= Time.deltaTime;
			if ( m_checkOnScreenTimer <= 0 )
			{	
				// bool test = m_camera.IsInsideActivationMinArea( transform.position );
				bool test = false;
				if ( test && !m_isOnScreen )	// If I wast on screen and new I am
				{
					if (Random.Range( 0, 100.0f ) < m_onAppearSoundProbability)
					{
						string soundName = m_onApprearSounds[ Random.Range( 0, m_onApprearSounds.Count ) ];
						// AudioManager.instance.PlayClip( soundName );
					}
				}
				m_isOnScreen = test;
				m_checkOnScreenTimer = 0.5f;
			}
		}
	}

	private void SetGolden(bool _value) {
		Renderer[] renderers = GetComponentsInChildren<Renderer>();

		for (int i = 0; i < renderers.Length; i++) {
			if (_value) {
				Material goldMat = Resources.Load ("Game/Assets/Materials/Gold") as Material;
				Material[] materials = renderers[i].materials;
				for (int m = 0; m < materials.Length; m++) {
					if ( !materials[m].shader.name.EndsWith("Additive") )
						materials[m] = goldMat;
				}
				renderers[i].materials = materials;
			} else {
				if (m_materials.ContainsKey(renderers[i].GetInstanceID()))
					renderers[i].materials = m_materials[renderers[i].GetInstanceID()];
			}
		}

		m_isGolden = _value;
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

	public float DistanceSqr(Vector3 _point) {
		if (m_bounds != null) {
			return m_bounds.bounds.bounds.SqrDistance(_point);
		} else {
			return Vector2.SqrMagnitude(_point - transform.position);
		}
	}

	//
	public bool IntersectsWith(Rect _r) {
		if (m_bounds != null) {
			return m_bounds.Overlaps(_r);
		} else {
			return _r.Contains(transform.position);
		}		
	}

	public bool IntersectsWith( Vector2 _center, float _radius)
	{
		if (m_bounds != null) {
			return m_bounds.Overlaps(_center, _radius);
		} else {
			// return _r.Contains(transform.position);
			float sqrMagnitude = (_center - (Vector2)transform.position).sqrMagnitude;
			return ( sqrMagnitude <= _radius * _radius );
		}		
	}

	public Vector3 RandomInsideBounds() {
		return m_bounds.bounds.RandomInside();
	}

	public void Damage( float damage ) 
	{
		m_health -= damage;
	}
}

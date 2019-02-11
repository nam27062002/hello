using UnityEngine;
using System;

abstract public class IEntity :  MonoBehaviour, ISpawnable {
    [Flags]
    public enum Tag {
        Animal      = (1 << 1),
        Flying      = (1 << 2),
        Ghost       = (1 << 3),
        Goblin      = (1 << 4),
        Human       = (1 << 5),
        Machine     = (1 << 6),
        Witch       = (1 << 7),
        Mine        = (1 << 8),
        Dragon      = (1 << 9),
        Collectible = (1 << 10),
        Magical     = (1 << 11),
        Troll       = (1 << 12),
        Monster     = (1 << 13),
        Fish        = (1 << 14),
        CarnivourusPlant = (1 << 15),
        Spider      = (1 << 16)
    }


	// Used externally to differientiate between types of entities
	public enum Type {
		PLAYER,
		PET,
		OTHER
	}

	public enum DyingReason{
		EATEN,
		BURNED,
		DESTROYED,
		OTHER
	}

	public const string ENTITY_PREFABS_PATH = "Game/Entities/NewEntites/";
	public const string ENTITY_PREFABS_LOW_PATH = "Game/Entities/NewEntitesLow/";
    
    /// <summary>
    /// Returns the path where the prefabs for entities are stored. It depends on the quality settings
    /// </summary>
    public static string EntityPrefabsPath {
        get {
            // Entities LOD flag has been disabled because it's not really worth it
            //return (FeatureSettingsManager.instance.EntitiesLOD == FeatureSettings.ELevel2Values.low) ? ENTITY_PREFABS_LOW_PATH : ENTITY_PREFABS_PATH;
            return ENTITY_PREFABS_PATH;
        }
    }

    //----------------------------------------------//
    [SerializeField][EnumMask] private Tag m_tags = 0;
    public bool HasTag(Tag _tag) {
        return (m_tags & _tag) != 0;
    }
    //----------------------------------------------//

	private int m_allowEdible;
	public bool allowEdible { get { return m_allowEdible == 0; } set { if (value) { m_allowEdible = Mathf.Max(0, m_allowEdible - 1); } else { m_allowEdible++; } } }

	private int m_allowBurnable;
	public bool allowBurnable { get { return m_allowBurnable == 0; } set { if (value) { m_allowBurnable = Mathf.Max(0, m_allowBurnable - 1); } else { m_allowBurnable++; } } }

	public virtual DragonTier edibleFromTier { get { return DragonTier.COUNT; } set { } }

	protected DefinitionNode m_def;
	public 	  DefinitionNode def { get { return m_def; } }
	public virtual string sku { get { return string.Empty; }}

	// Health
	protected float m_maxHealth;
	protected float m_health;
	public float health { get { return m_health; } set { m_health = value; } }

	public virtual int score { get { return 0; } }

	public ISpawnable[] m_otherSpawnables;
	protected int m_otherSpawnablesCount;
	protected AI.AIPilot m_pilot;
	public AI.AIPilot pilot { get { return m_pilot; } }

	protected AI.IMachine m_machine;
	public AI.IMachine machine { get { return m_machine; } }

	protected IViewControl m_viewControl;

    protected EntityEquip m_equip;
    public EntityEquip equip { get { return m_equip; } }

    public OnDieStatus onDieStatus;


	protected virtual void Awake() {
		ISpawnable[] spawners = GetComponents<ISpawnable>();
		m_otherSpawnables = new ISpawnable[ spawners.Length - 1 ];
		m_otherSpawnablesCount = 0;
		ISpawnable thisSpawn = this as ISpawnable;
		for (int i = 0; i < spawners.Length; i++) {
			if (spawners[i] != thisSpawn) {
				m_otherSpawnables[m_otherSpawnablesCount] = spawners[i];
				m_otherSpawnablesCount++;
			}
		}

		m_pilot = GetComponent<AI.AIPilot>();
		m_machine = GetComponent<AI.IMachine>();
		m_viewControl = GetComponent<IViewControl>();
        m_equip = GetComponent<EntityEquip>();

        onDieStatus = new OnDieStatus();
	}

	public virtual void Spawn(ISpawner _spawner) {
		m_health = m_maxHealth;

		m_allowEdible = 0;
		m_allowBurnable = 0;

		onDieStatus.isInFreeFall = false;
		onDieStatus.isPressed_ActionA = false;
		onDieStatus.isPressed_ActionB = false;
		onDieStatus.isPressed_ActionC = false;
	}

	protected bool m_isGolden = false;
	public bool isGolden { get { return m_isGolden; } }

	public virtual void SetGolden(Spawner.EntityGoldMode _mode) {
		m_isGolden = false;
	}

	protected bool m_isOnScreen = false;
	public bool isOnScreen { get { return m_isOnScreen; } }

	public int GetVertexCount() {
		if (m_viewControl != null && m_isOnScreen) {
			return m_viewControl.vertexCount;
		}
		return 0;
	}

	public int GetRendererCount() {
		if (m_viewControl != null && m_isOnScreen) {
			return m_viewControl.rendererCount;
		}
		return 0;
	}

	public void Damage(float damage) {
		m_health -= damage;
	}

	public virtual void Disable(bool _destroyed) {
		m_health = 0f;
		gameObject.SetActive(false);
	}

	public virtual Reward GetOnKillReward( DyingReason reason ) {
		return new Reward();
	}

    public virtual void CustomUpdate() {
		for (int i = 0; i < m_otherSpawnablesCount; i++) {
			m_otherSpawnables[i].CustomUpdate();
    	}
    }

	public virtual void CustomFixedUpdate() {
		if (m_machine != null) m_machine.CustomFixedUpdate();
	}

    public virtual bool CanDieOutsideFrustrum() { return true; }
	public virtual CircleArea2D circleArea 		{ get { return null; } }
}

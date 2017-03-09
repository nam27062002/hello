using UnityEngine;
using System.Collections;

public class SpawnerBg : AbstractSpawner {
	[System.Serializable]
	public class SpawnCondition {
		public enum Type {
			XP,
			TIME
		}

		public Type type = Type.XP;

		[NumericRange(0f)]	// Force positive value
		public float value = 0f;
	}

	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Separator("Entity")]	
	[CommentAttribute("The entities will spawn on the coordinates of the Spawner, and will move inside the defined area.")]
	[EntityPrefabListAttribute]
	[SerializeField] public string m_entityPrefabStr = "";
	private string m_entityPrefabPath;

	[SerializeField] public RangeInt m_quantity = new RangeInt(1, 1);
	[SerializeField] public Range	 m_scale = new Range(1f, 1f);
	[SerializeField] private uint	m_rails = 1;
	[CommentAttribute("Amount of points obtained after killing the whole flock. Points are multiplied by the amount of entities spawned.")]
	[SerializeField] private int m_flockBonus = 0;

	[Separator("Activation")]
	[Tooltip("Start spawning when any of the activation conditions is triggered.\nIf empty, the spawner will be activated at the start of the game.")]
	[SerializeField] private SpawnCondition[] m_activationTriggers;
	public SpawnCondition[] activationTriggers { get { return m_activationTriggers; }}

	[Tooltip("Stop spawning when any of the deactivation conditions is triggered.\nLeave empty for infinite spawning.")]
	[SerializeField] private SpawnCondition[] m_deactivationTriggers;
	public SpawnCondition[] deactivationTriggers { get { return m_deactivationTriggers; }}

	[Separator("Respawn")]	
	[SerializeField] private int m_maxSpawns;
	

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	protected AreaBounds m_area;
	public override AreaBounds area { 
		get { 
			if (m_guideFunction != null) {
				return m_guideFunction.GetBounds();
			} else {
				return m_area;
			}
		} 
	}
	
	protected EntityGroupController m_groupController;		
	
	private uint m_respawnCount;

	private bool m_readyToBeDisabled;

	private GameCamera m_newCamera;

	// Level editing stuff
	private bool m_showSpawnerInEditor = true;
	public bool showSpawnerInEditor {
		get { return m_showSpawnerInEditor; }
		set { m_showSpawnerInEditor = value; }
	}

    private int m_rail = 0;


    //-----------------------------------------------    

    //-----------------------------------------------
    // AbstractSpawner implementation
    //-----------------------------------------------    
    protected virtual AreaBounds GetArea() {
        Area area = GetComponent<Area>();
        if (area != null) {
            return area.bounds;
        }
        else {
            // spawner for static objects with a fixed position
            return new CircleAreaBounds(transform.position, 1f);
        }
    }

    protected override void RegisterInEntityManager(IEntity e) {        
        EntityManager.instance.RegisterEntityBg(e as EntityBg);
    }

    protected override void UnregisterFromEntityManager(IEntity e)
    {
        EntityManager.instance.UnregisterEntityBg(e as EntityBg);
    }

    protected override void OnStart() {
        if (m_quantity.max < m_quantity.min) {
            m_quantity.min = m_quantity.max;
        }
        
        RegisterInSpawnerManager();

        gameObject.SetActive(false);
	}

    protected override uint GetMaxEntities() {
        return (uint)m_quantity.max;
    }

    protected override void OnInitialize() {        
        m_respawnCount = 0;
        m_readyToBeDisabled = false;

        if (m_rails == 0) m_rails = 1;

        m_entityPrefabPath = IEntity.EntityPrefabsPath + m_entityPrefabStr;
        PoolManager.CreatePool(m_entityPrefabStr, m_entityPrefabPath, Mathf.Max(15, m_entities.Length), true);

        m_newCamera = Camera.main.GetComponent<GameCamera>();

        m_area = GetArea();

        m_groupController = GetComponent<EntityGroupController>();
        if (m_groupController)
        {
            m_groupController.Init(m_quantity.max);
        }

        m_guideFunction = GetComponent<IGuideFunction>();
    }

    protected override bool CanRespawnExtended() {
        // If we don't have any entity alive, proceed
        if (EntitiesAlive == 0 && FeatureSettingsManager.instance.IsDecoSpawnersEnabled) {
            // Respawn on cooldown?

            // Check activation area
            if (m_newCamera != null && m_newCamera.IsInsideBackgroundActivationArea(transform.position)) {
                return true;
            }
        }

        return false;
    }     

    protected override uint GetEntitiesAmountToRespawn() {
        // If player didn't killed all the spawned entities we'll re spawn only the remaining alive.
        // Also, this respawn will be instant.
        return (EntitiesKilled == EntitiesToSpawn) ? (uint)m_quantity.GetRandom() : EntitiesToSpawn - EntitiesKilled;        
    }   

    protected override string GetPrefabNameToSpawn(uint index) {
        return m_entityPrefabStr;
    }    
    
    protected override void OnEntitySpawned(GameObject spawning, uint index, Vector3 originPos) {
        if (index > 0) {
            originPos += RandomStartDisplacement(); // don't let multiple entities spawn on the same point
        }

        spawning.transform.position = originPos;
        spawning.transform.localScale = Vector3.one * m_scale.GetRandom();
    }

	protected override void OnMachineSpawned(AI.IMachine machine) {
        if (m_groupController)
            machine.EnterGroup(ref m_groupController.flock);
    }

    protected override void OnPilotSpawned(AI.Pilot pilot) {
        pilot.SetRail(m_rail, (int)m_rails);
        m_rail = (m_rail + 1) % (int)m_rails;
        pilot.guideFunction = m_guideFunction;
    }   
   
    protected override void OnAllEntitiesRemoved(GameObject _lastEntity, bool _allKilledByPlayer) {
        if (_allKilledByPlayer) {
            // check if player has destroyed all the flock
            if (m_flockBonus > 0) {
                Reward reward = new Reward();
                reward.score = (int)(m_flockBonus * EntitiesKilled);
                Messenger.Broadcast<Transform, Reward>(GameEvents.FLOCK_EATEN, _lastEntity.transform, reward);
            }

            m_respawnCount++;
            if (m_maxSpawns > 0 && m_respawnCount == m_maxSpawns) {
                m_readyToBeDisabled = true;
            }            
        } 

        if (m_readyToBeDisabled) {
            UnregisterFromSpawnerManager();
        }
    }    
    //-------------------------------------------------------------------    

	void OnDrawGizmos() {
		// Only if editor allows it
		if(showSpawnerInEditor) {
			// Draw spawn area
			GetArea().DrawGizmo();

			// Draw icon! - only in editor!
			#if UNITY_EDITOR
				// Icons are stored in the Gizmos folder in the project root (Unity rules), and have the same name as the entities
				Gizmos.DrawIcon(transform.position, IEntity.ENTITY_PREFABS_PATH + this.m_entityPrefabStr, true);
			#endif
		}
	}

	protected Vector3 RandomStartDisplacement() {
		return Random.onUnitSphere * 2f;
	}    
}

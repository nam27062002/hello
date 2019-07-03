using UnityEngine;
using System.Collections.Generic;
using System;

public class PrisonerSpawner : AbstractSpawner, IBroadcastListener {

	[Serializable]
	public class Group {
        [EntityPrefabListAttribute]
        public string[] m_entityPrefabsStr;
	}

	[SeparatorAttribute("Spawn")]
	[SerializeField] private Group[] m_groups;
	[SerializeField] private Range m_scale = new Range(1f, 1f);
	[SerializeField] private Transform[] m_spawnAtTransform;
		
	private PoolHandler[,] m_poolHandlers;

	private Transform[] m_parents;

    private uint m_maxEntities;	

	private bool m_allKilledByPlayer;


    //---------------------------------------------------------------------------------------------------------    
    // AbstractSpawner implementation
    //-------------------------------------------------------------------	

    private AreaBounds m_areaBounds = new RectAreaBounds(Vector3.zero, Vector3.one);
    public override AreaBounds area { get { return m_areaBounds; } set { m_areaBounds = value; } }

	void Awake() {
		UseProgressiveRespawn = false;
		UseSpawnManagerTree = false;

		int maxHandlers = 0;
		m_maxEntities = 0;
		for (int g = 0; g < m_groups.Length; g++) {
			m_maxEntities = (uint)Mathf.Max(m_maxEntities, m_groups[g].m_entityPrefabsStr.Length);
			maxHandlers = Mathf.Max(maxHandlers, m_groups[g].m_entityPrefabsStr.Length);
		}
		m_poolHandlers = new PoolHandler[m_groups.Length, maxHandlers];

		Initialize();

		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}

	protected override void OnDestroy() {
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
		base.OnDestroy();
	}

    public override List<string> GetPrefabList() {
        return null;
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                CreatePools();
            }break;
        }
    }
    

    protected override uint GetMaxEntities() {
        return m_maxEntities;
    }

    protected override void OnInitialize() {     		
		CreatePools();

		if (m_maxEntities > 0f) {            			
			m_parents = new Transform[m_maxEntities];
		}
	}    

	private void CreatePools() {
		string prefabName;
		for (int g = 0; g < m_groups.Length; g++) {			
			for (int e = 0; e < m_groups[g].m_entityPrefabsStr.Length; e++) {
				prefabName = m_groups[g].m_entityPrefabsStr[e];
				m_poolHandlers[g, e] = PoolManager.RequestPool(prefabName, 1);                
			}
		}
	}

    protected override void OnPrepareRespawning() {
        GroupIndexToSpawn = (uint)UnityEngine.Random.Range(0, m_groups.Length);        
        int count = m_entities.Length;
        for (int i = 0; i < count; i++)
        {
            m_entities[i] = null;
            m_parents[i] = null;
        }
    }

    protected override uint GetEntitiesAmountToRespawn() {
        return (uint)m_groups[GroupIndexToSpawn].m_entityPrefabsStr.Length;
    }                            

	protected override PoolHandler GetPoolHandler(uint index) {
		return m_poolHandlers[GroupIndexToSpawn, index];
	}

    protected override string GetPrefabNameToSpawn(uint index) {
        return m_groups[GroupIndexToSpawn].m_entityPrefabsStr[index];
    }

    protected override void OnCreateInstance(uint index, GameObject go) {        
        m_parents[index] = go.transform.parent;        
    }

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {
        Transform t = spawning.transform;
		Transform parent = m_spawnAtTransform[index % m_spawnAtTransform.Length];
		t.parent = parent;
		t.localPosition = Vector3.zero;
		t.localScale = Vector3.one * m_scale.GetRandom();

		m_allKilledByPlayer = false;
	}

	protected override void OnMachineSpawned(AI.IMachine machine) {
        machine.EnterDevice(true);
    }

	protected override void OnRemoveEntity(IEntity _entity, int index, bool _killedByPlayer) {        
        m_parents[index] = null;
    }
    
	protected override void OnAllEntitiesRemoved(IEntity _lastEntity, bool _allKilledByPlayer) {
		m_allKilledByPlayer = _allKilledByPlayer;
	}

	//---------------------------------------------------------------------------------------------------------   
    public void SetEntitiesFree() {
        for (int i = 0; i < m_entities.Length; i++) {
            if (m_entities[i] != null) {
                m_entities[i].transform.parent = m_parents[i];
                // change state in machine				
				m_entities[i].machine.LeaveDevice(true);				
				//m_entities[i] = null;
            }
        }
    }

	public bool AreAllDead() {
		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] != null) {
				return false;
			}
		}

		return true;
	}

	public bool AreAllKilledByPlayer() {
		return m_allKilledByPlayer;
	}

    private uint GroupIndexToSpawn { get; set; }

    private Vector3 RandomStartDisplacement() {
        return Vector3.right * UnityEngine.Random.Range(-1f, 1f) * 0.5f;
    }

    //
    void OnDrawGizmosSelected() {
		Gizmos.color = Colors.coral;
		for (int i = 0; i < m_spawnAtTransform.Length; i++) {
			if (m_spawnAtTransform[i] != null) {		        
				Gizmos.DrawSphere(m_spawnAtTransform[i].position, 0.5f);
			}
		}
    }

    //-------------------------------------------------------------------
}

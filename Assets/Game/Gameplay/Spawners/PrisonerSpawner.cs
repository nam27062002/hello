using UnityEngine;
using System.Collections;
using System;

public class PrisonerSpawner : AbstractSpawner {

	[Serializable]
	public class Group {
        [EntityPrefabListAttribute]
        public string[] m_entityPrefabsStr;
	}

	[SeparatorAttribute("Spawn")]
	[SerializeField] private Group[]	m_groups;
	[SerializeField] private Range		m_scale = new Range(1f, 1f);
	[SerializeField] private Vector3	m_spawnPosition = Vector3.zero;
	[SerializeField] private Transform	m_parent;
	
	private Transform[] m_parents;

    private uint m_maxEntities;	

    //---------------------------------------------------------------------------------------------------------    
    // AbstractSpawner implementation
    //-------------------------------------------------------------------	

    private AreaBounds m_areaBounds = new RectAreaBounds(Vector3.zero, Vector3.one);
    public override AreaBounds area { get { return m_areaBounds; } set { m_areaBounds = value; } }

	private void Awake() {
		// Progressive respawn disabled because it respawns only one instance and it's triggered by CageBehaviour which is not prepared to loop until Respawn returns true
		UseProgressiveRespawn = false;
		UseSpawnManagerTree = false;

        m_maxEntities = 0;
        for (int g = 0; g < m_groups.Length; g++) {
            m_maxEntities = (uint)Mathf.Max(m_maxEntities, m_groups[g].m_entityPrefabsStr.Length);            
        }        

		Initialize();
    }

    protected override uint GetMaxEntities() {
        return m_maxEntities;
    }

    protected override void OnInitialize() {                
        string prefabName;
        for (int g = 0; g < m_groups.Length; g++) {			
			for (int e = 0; e < m_groups[g].m_entityPrefabsStr.Length; e++) {
                prefabName = m_groups[g].m_entityPrefabsStr[e];
                
				PoolManager.RequestPool(prefabName, IEntity.EntityPrefabsPath, 1);                
			}
		}

		if (m_maxEntities > 0f) {            			
			m_parents = new Transform[m_maxEntities];
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

    protected override string GetPrefabNameToSpawn(uint index) {
        return m_groups[GroupIndexToSpawn].m_entityPrefabsStr[index];
    }

    protected override void OnCreateInstance(uint index, GameObject go) {        
        m_parents[index] = go.transform.parent;        
    }

    protected override void OnEntitySpawned(GameObject spawning, uint index, Vector3 originPos) {
        Vector3 pos = originPos + m_spawnPosition;
        if (index > 0) {
            pos += RandomStartDisplacement(); // don't let multiple entities spawn on the same point
        }

        Transform t = spawning.transform;
        t.position = pos;
        t.localScale = Vector3.one * m_scale.GetRandom();

		if (m_parent != null) {
			t.parent = m_parent;
		}
    }

	protected override void OnMachineSpawned(AI.IMachine machine) {
        machine.LockInCage();
    }

    protected override void OnRemoveEntity(GameObject _entity, int index) {        
        m_parents[index] = null;
    }
    //---------------------------------------------------------------------------------------------------------   

	public void UpdatePrisonerTransform(Transform _cage) {
		Vector3 pos = transform.position + m_spawnPosition;

		pos = _cage.InverseTransformVector(pos);
		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] != null) {
				m_entities[i].transform.position = pos;
				m_entities[i].transform.rotation = _cage.rotation;
			}
		}
	}

	//---------------------------------------------------------------------------------------------------------   

    public void SetEntitiesFree() {
        for (int i = 0; i < m_entities.Length; i++) {
            if (m_entities[i] != null) {
                m_entities[i].transform.parent = m_parents[i];

                // change state in machine
				m_entities[i].machine.UnlockFromCage();
				m_entities[i] = null;
            }
        }
    }

    private uint GroupIndexToSpawn { get; set; }

    private Vector3 RandomStartDisplacement() {
        return Vector3.right * UnityEngine.Random.Range(-1f, 1f) * 0.5f;
    }

    //
    void OnDrawGizmosSelected() {
        Gizmos.color = Colors.coral;
        Gizmos.DrawSphere(transform.position + m_spawnPosition, 0.5f);
    }

    //-------------------------------------------------------------------
}

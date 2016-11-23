using UnityEngine;
using System.Collections;
using System;

public class CageSpawner : AbstractSpawner {

	[Serializable]
	public class Group {
		public GameObject[] entityPrefabs;
	}

	[SeparatorAttribute("Spawn")]
	[SerializeField] private Group[]	m_groups;
	[SerializeField] private Range		m_scale = new Range(1f, 1f);
	[SerializeField] private Vector3	m_spawnPosition = Vector3.zero;

	private GameObject[] m_entities; // list of alive entities
	private Transform[] m_parents;

	private AreaBounds m_areaBounds = new RectAreaBounds(Vector3.zero, Vector3.one);

	//---------------------------------------------------------------------------------------------------------	
	public override AreaBounds area 				{ get { return m_areaBounds; } set { m_areaBounds = value; } }

    //---------------------------------------------------------------------------------------------------------
    protected override void StartExtended() { 
        m_rect = new Rect((Vector2)transform.position, Vector2.zero);
	}

	public override void Initialize() {
		int maxEntities = 0;

		for (int g = 0; g < m_groups.Length; g++) {
			maxEntities = Mathf.Max(maxEntities, m_groups[g].entityPrefabs.Length);

			for (int e = 0; e < m_groups[g].entityPrefabs.Length; e++) {
				PoolManager.CreatePool(m_groups[g].entityPrefabs[e], 15, true);
			}
		}

		if (maxEntities > 0f) {
			m_entities = new GameObject[maxEntities];
			m_parents = new Transform[maxEntities];
		}
	}

	public void SetEntitiesFree() {
		for (int i = 0; i < m_entities.Length; i++) {			
			if (m_entities[i] != null) {
				m_entities[i].transform.parent = m_parents[i];

				// change state in machine
				m_entities[i].GetComponent<AI.IMachine>().UnlockFromCage();
			}
		}
	}    

    public override void ForceRemoveEntities() {
        for (int i = 0; i < m_entities.Length; i++) {
            if (m_entities[i] != null) {
                RemoveEntity(m_entities[i], false);
            }
        }
    }

    protected override bool RemoveEntityExtended(GameObject _entity, bool _killedPlayer) {
        bool returnValue = false;
        for (int i = 0; i < m_entities.Length && !returnValue; i++) {
            if (m_entities[i] == _entity) {
                m_entities[i].transform.parent = m_parents[i];
                returnValue = true;
                m_entities[i] = null;
                m_parents[i] = null;
            }
        }

        return returnValue;
    }
        
    public override bool CanRespawn() 	{ return true; }
	public override bool Respawn()		{ Spawn(); return true; }

	//---------------------------------------------------------------------------------------------------------

	private void Spawn() {
		for (int i = 0; i < m_entities.Length; i++) {
			m_entities[i] = null;
			m_parents[i] = null;
		}

		// choose one group
		int groupIndex = UnityEngine.Random.Range(0, m_groups.Length);
		int entitiesSpawned = m_groups[groupIndex].entityPrefabs.Length;

		for (int i = 0; i < entitiesSpawned; i++) {			
			m_entities[i] = PoolManager.GetInstance(m_groups[groupIndex].entityPrefabs[i].name);
			m_parents[i] = m_entities[i].transform.parent;
		}

		for (int i = 0; i < entitiesSpawned; i++) {			
			GameObject spawning = m_entities[i];

			Vector3 pos = transform.position + m_spawnPosition;
			if (i > 0) {
				pos += RandomStartDisplacement(); // don't let multiple entities spawn on the same point
			}

			spawning.transform.position = pos;
			spawning.transform.localScale = Vector3.one * m_scale.GetRandom();

			Entity entity = spawning.GetComponent<Entity>();
			if (entity != null) {
				entity.Spawn(this); // lets spawn Entity component first
			}

			AI.AIPilot pilot = spawning.GetComponent<AI.AIPilot>();
			if (pilot != null) {
				pilot.Spawn(this);
			}

			ISpawnable[] components = spawning.GetComponents<ISpawnable>();
			foreach (ISpawnable component in components) {
				if (component != entity && component != pilot) {
					component.Spawn(this);
				}
			}

			spawning.transform.parent = transform;
			spawning.GetComponent<AI.IMachine>().LockInCage();
		}
	}

	private Vector3 RandomStartDisplacement() {
		return Vector3.right * UnityEngine.Random.Range(-1f, 1f) * 0.5f;
	}


	//
	void OnDrawGizmosSelected() {
		Gizmos.color = Colors.coral;
		Gizmos.DrawSphere(transform.position + m_spawnPosition, 0.5f);
	}

	public override void DrawStateGizmos() {}
}

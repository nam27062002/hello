using UnityEngine;
using System.Collections;
using System;

public class CageSpawner : MonoBehaviour, ISpawner {

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

	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }
	public string name 					{ get { return name; } }
	public AreaBounds area 				{ get { return m_areaBounds; } set { m_areaBounds = value; } }
	public IGuideFunction guideFunction	{ get { return null; } }

	//---------------------------------------------------------------------------------------------------------
	void Start() {
		m_rect = new Rect((Vector2)transform.position, Vector2.zero);
	}

	public void Initialize() {
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

	public void ForceRemoveEntities() {
		for (int i = 0; i < m_entities.Length; i++) {			
			if (m_entities[i] != null) {
				m_entities[i].transform.parent = m_parents[i];

				PoolManager.ReturnInstance(m_entities[i]);

				m_entities[i] = null;
				m_parents[i] = null;
			}
		}
	}

	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {
		for (int i = 0; i < m_entities.Length; i++) {			
			if (m_entities[i] == _entity) {
				m_entities[i].transform.parent = m_parents[i];

				PoolManager.ReturnInstance(m_entities[i]);

				m_entities[i] = null;
				m_parents[i] = null;
			}
		}
	}

	public void CheckRespawn() 	{}
	public void Respawn()		{ Spawn(); }

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
}

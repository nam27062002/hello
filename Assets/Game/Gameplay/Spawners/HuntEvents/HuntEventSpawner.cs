using UnityEngine;
using System.Collections.Generic;

public class HuntEventSpawner : MonoBehaviour {

	[SerializeField] private float m_spawnTimeSecs;
	private float m_spawnTimer;

	[SerializeField] private float m_huntTimeSecs;
	public float huntTimeSecs { get { return m_huntTimeSecs; } }

	private float m_huntTimer; 
	public float huntTimer { get { return m_huntTimer; } }

	private List<Transform> m_spawners;
	private List<float> m_spawnChance;

	private int m_currentSpawnIndex;
	private GameObject m_currentTarget;

	// Use this for initialization
	void Start() {

		PoolManager.CreatePool(Resources.Load<GameObject>("PROTO/HuntFalcon"), 2, true);

		m_spawners = new List<Transform>();
		m_spawnChance = new List<float>();

		foreach (Transform child in transform) {
			m_spawners.Add(child);
			m_spawnChance.Add(0);
		}

		m_huntTimer = 0;
		m_spawnTimer = m_spawnTimeSecs;

		m_currentSpawnIndex = -1;
	}
	
	// Update is called once per frame
	void Update() {

		if (m_spawnTimer > 0) {
		
			m_spawnTimer -= Time.deltaTime;

			if (m_spawnTimer <= 0) {

				m_spawnTimer = 0;
				Spawn();
			}
		} else if (m_huntTimer > 0) {

			m_huntTimer -= Time.deltaTime;

			if (m_huntTimer <= 0 || !m_currentTarget.activeInHierarchy) {

				m_huntTimer = 0;
				m_spawnTimer = m_spawnTimeSecs;

				m_currentTarget.SetActive(false);
				m_currentTarget = null;

				Messenger.Broadcast<Transform, bool>(GameEvents.HUNT_EVENT_TOGGLED, m_spawners[m_currentSpawnIndex], false);

				m_currentSpawnIndex = -1;
			}
		}
	}

	private void Spawn() {

		/* Should we spawn based on distance?
		 * maybe we can have more than one spawner
		 */

		m_currentSpawnIndex = Random.Range(0, m_spawners.Count);

		CircleCollider2D col = m_spawners[m_currentSpawnIndex].GetComponent<CircleCollider2D>();
		Bounds bounds = new Bounds();
		bounds.center = m_spawners[m_currentSpawnIndex].position;
		bounds.extents = new Vector3(col.radius, col.radius, 0);

		Vector3 pos = bounds.center;

		m_currentTarget = PoolManager.GetInstance("HuntFalcon");
		m_currentTarget.transform.position = pos;
		m_currentTarget.GetComponent<SpawnableBehaviour>().Spawn(bounds);

		m_huntTimer = m_huntTimeSecs;
		
		Messenger.Broadcast<Transform, bool>(GameEvents.HUNT_EVENT_TOGGLED, m_currentTarget.transform, true);
	}
}

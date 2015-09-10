﻿using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Header("Entity")]
	[SerializeField] private GameObject m_entityPrefab;
	[SerializeField] protected RangeInt m_quantity;

	[Header("Activation")]
	[SerializeField] private float m_enableTime;
	[SerializeField] private float m_disableTime;
	[SerializeField] private float m_playerDistance;

	[Header("Respawn")]
	[SerializeField] private Range m_spawnTime;
	[SerializeField] private float m_TimeInc;
	[SerializeField] private float m_TimeIncTime;
	[SerializeField] private int m_maxSpawns;
	

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	protected GameObject[] m_entities;
	protected AreaBounds m_area;

	private float m_enableTimer;
	private float m_disableTimer;
	private float m_spawnTimer;
	private uint m_spawnCount;	

	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	protected virtual void Start () {
		
		InstanceManager.pools.CreatePool(m_entityPrefab);
		m_entities = new GameObject[m_quantity.max];
		m_area = GetComponent<Area>().bounds;
	}

	protected virtual void OnEnable() {

		m_enableTimer = m_enableTime;
		m_disableTimer = m_disableTime;

		m_spawnTimer = 0;
		m_spawnCount = 0;
	}

	// entities can remove themselves when are destroyed or auto-disabled
	public void RemoveEntity(GameObject _entity) {

		for (int i = 0; i < m_entities.Length; i++) {			
			if (m_entities[i] == _entity) {
				m_entities[i] = null;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
		bool playerNear = CheckPlayerDistance();

		// A spawner can have a delay time before it can spawn things
		if (m_enableTimer > 0) {
			m_enableTimer -= Time.deltaTime;
			if (m_enableTimer <= 0) {
				m_enableTimer = 0;
			}
		} else {
			// Modify spawn time over time
			// TODO: spawners can have more or less respawn time while we keep playing

			// Spawn logic
			bool allDisabled = CheckEntities();
			if (allDisabled) {
				if (m_spawnTimer > 0) {
					m_spawnTimer -= Time.deltaTime;
					if (m_spawnTimer <= 0) {
						m_spawnTimer = 0;
					}
				} else {
					if (playerNear) {
						Spawn();
						m_spawnTimer = m_spawnTime.GetRandom();
					}
				}
			}
		}

		// Check if we have to disable this spawner after few seconds
		if (m_disableTimer > 0) {
			m_disableTimer -= Time.deltaTime;
			if (m_disableTimer <= 0) {
				enabled = false;
			}
		}
	}

	bool CheckEntities() {
		bool allDisabled = true;

		for (int i = 0; i < m_entities.Length; i++) {

			if (m_entities[i] != null) {
				allDisabled = false;
			}
		}

		return allDisabled;
	}

	bool CheckPlayerDistance() {
		bool playerNear = true; //TODO: disable spawners when player is far away

		if (!playerNear) {
			// disable all entities
			for (int i = 0; i < m_entities.Length; i++) {
				
				if (m_entities[i] != null) {
					m_entities[i].SetActive(false);
					m_entities[i] = null;
				}
			}
		}

		return playerNear;
	}

	void Spawn() {

		int count = m_quantity.GetRandom();
		for (int i = 0; i < count; i++) {			
			m_entities[i] = InstanceManager.pools.GetInstance(m_entityPrefab.name);
			SpawnBehaviour spawn = m_entities[i].GetComponent<SpawnBehaviour>();
			spawn.Spawn(m_area, this);
		}

		ExtendedSpawn();

		// Disable this spawner after a number of spawns
		if (m_maxSpawns > 0) {
			m_spawnCount++;			
			if (m_spawnCount == m_maxSpawns) {
				enabled = false;
			}
		}
	}

	protected virtual void ExtendedSpawn() {}
}

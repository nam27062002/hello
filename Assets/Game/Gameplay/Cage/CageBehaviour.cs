﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class CageBehaviour : MonoBehaviour, ISpawnable {
	
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SeparatorAttribute]
	[SerializeField] private HitsPerDragonTier m_hitsPerTier;
	[SeparatorAttribute]
	[SerializeField] private GameObject m_colliderHolder;
	[SerializeField] private GameObject m_view;
	[SerializeField] private string m_onBreakParticle;


	private float m_waitTimer = 0;
	private Hit m_currentHits;
	private DragonTier m_tier;

	private PrisonerSpawner m_cageSpawner;



	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Awake() {
		m_cageSpawner = GetComponent<PrisonerSpawner>();
		m_cageSpawner.Initialize();

		m_currentHits = new Hit();
	}

	public void Spawn(ISpawner _spawner) {
		DragonPlayer player = InstanceManager.player;
		m_tier = player.GetTierWhenBreaking();

		m_waitTimer = 0;
		Hit originalHits = m_hitsPerTier.Get(m_tier);
		m_currentHits.count = originalHits.count;
		m_currentHits.needBoost = originalHits.needBoost;

		m_view.SetActive(true);
		SetCollisionsEnabled(true);

		m_cageSpawner.area = _spawner.area; // cage spawner will share the area defined to spawn the cage.
		m_cageSpawner.Respawn();
	}

	void OnDisable() {
		m_cageSpawner.ForceRemoveEntities();
	}

	// Update is called once per frame
	private void Update() {
		m_waitTimer -= Time.deltaTime;
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.transform.CompareTag("Player")) {
			if (m_currentHits.needBoost) {				
				if (m_waitTimer <= 0) {
					GameObject go = collision.transform.gameObject;
					DragonBoostBehaviour boost = go.GetComponent<DragonBoostBehaviour>();	
					if (boost.IsBoostActive()) 	{
						DragonMotion dragonMotion = go.GetComponent<DragonMotion>();	// Check speed is enough
						if (dragonMotion.lastSpeed >= (dragonMotion.absoluteMaxSpeed * 0.85f)) {
							m_waitTimer = 0.5f;
							// Check Min Speed
							m_currentHits.count--;
							if (m_currentHits.count <= 0)
								Break();
						}
					}
				}
			} else {
				Break();
			}
		}
	}

	private void Break() {
		// Spawn particle
		GameObject prefab = Resources.Load("Particles/" + m_onBreakParticle) as GameObject;
		if (prefab != null) {
			GameObject go = Instantiate(prefab) as GameObject;
			if (go != null) {
				go.transform.position = transform.position;
				go.transform.rotation = transform.rotation;
			}
		}

		m_view.SetActive(false);
		SetCollisionsEnabled(false);

		m_cageSpawner.SetEntitiesFree();
	}

	private void SetCollisionsEnabled(bool _value) {
		Collider[] colliders = m_colliderHolder.GetComponents<Collider>();
		for (int c = 0; c < colliders.Length; c++) {
			colliders[c].enabled = _value;
		}
	}
}
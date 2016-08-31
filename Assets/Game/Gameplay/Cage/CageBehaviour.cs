using UnityEngine;
using System.Collections.Generic;
using System;

public class CageBehaviour : MonoBehaviour, ISpawnable {

	//-----------------------------------------------
	// Classes
	//-----------------------------------------------
	[Serializable]
	public class ContainerHit {
		public int m_numHits;
		public bool m_breaksWithoutTurbo;
	}

	[Serializable]
	public class SerializableInstance : SerializableDictionary<DragonTier, ContainerHit>
	{}


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] public  SerializableInstance m_hits = new SerializableInstance();
	[SerializeField] private string m_onBreakParticle;

	private float m_waitTimer = 0;
	private ContainerHit m_currentHits;
	private DragonTier m_tier;

	private ISpawner m_spawner;
	private CageSpawner m_cageSpawner;

	private GameObject m_view;
	private GameCamera m_newCamera;


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Awake() {
		m_cageSpawner = GetComponent<CageSpawner>();
		m_cageSpawner.Initialize();

		m_currentHits = new ContainerHit();

		m_view = transform.FindChild("view").gameObject;
		m_newCamera = Camera.main.GetComponent<GameCamera>();
	}

	public void Spawn(ISpawner _spawner) {
		m_spawner = _spawner;

		DragonPlayer player = InstanceManager.player.GetComponent<DragonPlayer>();
		m_tier = player.GetTierWhenBreaking();

		m_waitTimer = 0;
		ContainerHit originalHits = m_hits.Get(m_tier);
		m_currentHits.m_numHits = originalHits.m_numHits;
		m_currentHits.m_breaksWithoutTurbo = originalHits.m_breaksWithoutTurbo;

		m_view.SetActive(true);
		SetCollisionsEnabled(true);

		m_cageSpawner.area = m_spawner.area; // cage spawner will share the area defined to spawn the cage.
		m_cageSpawner.Respawn();
	}

	private void Disable(bool _destroyed) {		
		m_spawner.RemoveEntity(gameObject, _destroyed);
		m_cageSpawner.ForceRemoveEntities();
	}

	// Update is called once per frame
	private void Update() {
		m_waitTimer -= Time.deltaTime;
	}

	private void LateUpdate() {
		// check camera to destroy this entity if it is outside view area
		if (m_newCamera != null && m_newCamera.IsInsideDeactivationArea(transform.position)) {
			if (m_spawner != null) {
				Disable(false);
			}
		}
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.transform.tag == "Player") {
			if (m_currentHits.m_breaksWithoutTurbo) {
				Break();
			} else if (m_waitTimer <= 0) {
				GameObject go = collision.transform.gameObject;
				DragonBoostBehaviour boost = go.GetComponent<DragonBoostBehaviour>();	
				if (boost.IsBoostActive()) 	{
					DragonMotion dragonMotion = go.GetComponent<DragonMotion>();	// Check speed is enough
					if (dragonMotion.lastSpeed >= (dragonMotion.absoluteMaxSpeed * 0.85f)) {
						m_waitTimer = 0.5f;
						// Check Min Speed
						m_currentHits.m_numHits--;
						if (m_currentHits.m_numHits <= 0)
							Break();
					}
				}
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
		Collider[] colliders = GetComponents<Collider>();
		for (int c = 0; c < colliders.Length; c++) {
			colliders[c].enabled = _value;
		}
	}
}
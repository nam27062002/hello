using UnityEngine;
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
	[SerializeField] private GameObject[] m_viewDestroyed;
	[SerializeField] private ParticleData m_onBreakParticle;
	[SerializeField] private string m_onBreakSound;
	[SerializeField] private string m_onCollideSound;


	private float m_waitTimer = 0;
	private Hit m_currentHits;
	private DragonTier m_tier;

	private bool m_broken;

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
		for (int i = 0; i < m_viewDestroyed.Length; i++) {
			m_viewDestroyed[i].SetActive(false);
		}
		SetCollisionsEnabled(true);

		m_cageSpawner.area = _spawner.area; // cage spawner will share the area defined to spawn the cage.
		m_cageSpawner.Respawn();

		m_broken = false;
	}

	void OnDisable() {
		m_cageSpawner.ForceRemoveEntities();
	}

	// Update is called once per frame
	public void CustomUpdate() {
		m_waitTimer -= Time.deltaTime;
	}

	private void LateUpdate() {
	//	m_cageSpawner.UpdatePrisonerTransform(m_view.transform);
	}

	private void OnCollisionEnter(Collision collision) {
		if (!m_broken) {
			if (collision.transform.CompareTag("Player")) {				
				if (m_currentHits.needBoost) {
					if (m_waitTimer <= 0) {						
						GameObject go = collision.transform.gameObject;
						DragonBoostBehaviour boost = go.GetComponent<DragonBoostBehaviour>();	
						bool playCollideSound = true;
						if (boost.IsBoostActive()) 	{
							DragonMotion dragonMotion = go.GetComponent<DragonMotion>();	// Check speed is enough
							if (dragonMotion.howFast >= 0.85f) {
								m_waitTimer = 0.5f;
								// Check Min Speed

								if (m_currentHits > 0) {
									m_currentHits.count--;
									if (m_currentHits.count <= 0) {
										Break();
										playCollideSound = false;
									}
								} else {
									Messenger.Broadcast<DragonTier, string>(GameEvents.BIGGER_DRAGON_NEEDED, DragonTier.COUNT, "");
								}
							}
						}
						if ( playCollideSound && !string.IsNullOrEmpty( m_onCollideSound))
							AudioController.Play(m_onCollideSound);
					
					}
				} else {
					Break();
				}
			}
		}
	}

	private void Break() {
		// Spawn particle
		if (m_onBreakParticle.IsValid()) {
			GameObject ps = ParticleManager.Spawn(m_onBreakParticle);
			if (ps != null) {
				ps.transform.position = m_view.transform.TransformPoint(m_onBreakParticle.offset);
			}
		}

		m_view.SetActive(false);
		for (int i = 0; i < m_viewDestroyed.Length; i++) {
			m_viewDestroyed[i].SetActive(true);
		}
		SetCollisionsEnabled(false);

		m_cageSpawner.SetEntitiesFree();

		if (!string.IsNullOrEmpty(m_onBreakSound)) {
			AudioController.Play(m_onBreakSound);
		}

		m_broken = true;
	}

	private void SetCollisionsEnabled(bool _value) {
		Collider[] colliders = m_colliderHolder.GetComponents<Collider>();
		for (int c = 0; c < colliders.Length; c++) {
			colliders[c].enabled = _value;
		}
	}
}
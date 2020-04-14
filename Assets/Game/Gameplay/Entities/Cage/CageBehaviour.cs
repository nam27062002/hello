﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class CageBehaviour : ISpawnable {

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
    [SerializeField] private ParticleData m_onHitParticle;
    [SerializeField] private bool m_hitParticleFaceDragonDirection = false;
    [SerializeField] private string m_onBreakSound;
    [SerializeField] private string m_onCollideSound;
    [SerializeField] public Transform m_centerTarget;
    public Transform centerTarget {
        get { return m_centerTarget; }
    }

    private Cage m_entity;

    private Vector3 m_initialViewPos;

    private float m_waitTimer = 0;
    private Hit m_currentHits;
    private DragonTier m_tier;
    private DragonTier m_minTierToBreak;

    private bool m_broken;
    public bool broken {
        get { return m_broken; }
    }

    private PrisonerSpawner m_prisonerSpawner;
    private Wobbler m_wobbler;


    //-----------------------------------------------
    // Methods
    //-----------------------------------------------
    void Awake() {
        m_entity = GetComponent<Cage>();
        m_prisonerSpawner = GetComponent<PrisonerSpawner>();
        m_wobbler = GetComponent<Wobbler>();
        m_minTierToBreak = m_hitsPerTier.GetMinTier();

        m_initialViewPos = m_view.transform.localPosition;

        m_onBreakParticle.CreatePool();
        m_onHitParticle.CreatePool();

        m_currentHits = new Hit();
    }

    override public void Spawn(ISpawner _spawner) {
		// Only if spawner is valid!
		if(_spawner == null) {
			Debug.LogError("Attempting to Spawn a CageBehaviour with a NULL spawner.");
			return;
		}

		// Check player's destruction tier
		// Protect possible null player - shouldn't happen, but just in case
        DragonPlayer player = InstanceManager.player;
		if(player == null) {
			m_tier = DragonTier.COUNT - 1;	// Max Tier, if this ever happens, favor the player
		} else {
			m_tier = player.GetTierWhenBreaking();
		}

        m_waitTimer = 0;

		if(m_currentHits != null) {
			Hit originalHits = m_hitsPerTier.Get(m_tier);
			m_currentHits.count = originalHits.count;
			m_currentHits.needBoost = originalHits.needBoost;
		}

        if(m_view != null) m_view.SetActive(true);
        for (int i = 0; i < m_viewDestroyed.Length; i++) {
            if(m_viewDestroyed != null) m_viewDestroyed[i].SetActive(false);
        }

        SetCollisionsEnabled(true);

		if(m_prisonerSpawner != null) {
			m_prisonerSpawner.area = _spawner.area; // cage spawner will share the area defined to spawn the cage.
			m_prisonerSpawner.Respawn();
		}

        m_broken = false;
    }

    void OnDisable() {
        m_prisonerSpawner.ForceRemoveEntities();
    }

    // Update is called once per frame
    override public void CustomUpdate() {
        m_waitTimer -= Time.deltaTime;

        if (m_prisonerSpawner.AreAllDead()) {
            if (m_prisonerSpawner.AreAllKilledByPlayer()) {
                m_entity.SetDestroyedByDragon();
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (!m_broken) {
            if (collision.transform.CompareTag("Player")) {
                if (m_currentHits.count > 0) {
                    bool isValidHit = true;

                    GameObject go = collision.transform.gameObject;
                    DragonPlayer player = go.GetComponent<DragonPlayer>();
                    DragonMotion dragonMotion = player.dragonMotion;    // Check speed is enough

                    if (m_currentHits.needBoost) {
                        if (!player.IsBreakingMovement() || dragonMotion.howFast < 0.85f) {
                            Messenger.Broadcast(MessengerEvents.BREAK_OBJECT_NEED_TURBO);
                            isValidHit = false;
                        }
                    }

                    if (m_waitTimer <= 0f && isValidHit) {
                        bool playCollideSound = true;

                        m_waitTimer = 1f;

                        // Check Min Speed
                        m_currentHits.count--;
                        if (m_currentHits.count <= 0) {
                            Break();
                            playCollideSound = false;
                        } else {
                            GameObject ps = m_onHitParticle.Spawn();
                            if (ps != null) {
                                ps.transform.position = collision.contacts[0].point;// m_view.transform.TransformPoint(m_onHitParticle.offset);
                                if (m_hitParticleFaceDragonDirection) {
                                    FaceDragon(ps, dragonMotion);
                                }
                            }
                            if (m_wobbler != null) {
                                m_wobbler.enabled = true;
                                m_wobbler.StartWobbling(m_view.transform, m_initialViewPos);
                            }
                        }

                        if (playCollideSound && !string.IsNullOrEmpty(m_onCollideSound))
                            AudioController.Play(m_onCollideSound);
                    }
                } else {
                    Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, m_minTierToBreak, "");
                }
            } else if (collision.transform.CompareTag("Pet")) {
                // Check if pet is trying to break this cage!
                Pet pet = collision.transform.GetComponent<Pet>();
                if (pet != null && pet.CanBreakCages && pet.Charging) {
                    Break();
                }
            }
        }
    }

    public void Break() {
        if (!m_broken) {
            // Spawn particle
            GameObject ps = m_onBreakParticle.Spawn();
            if (ps != null) {
                ps.transform.position = m_view.transform.TransformPoint(m_onBreakParticle.offset);
            }

            m_view.SetActive(false);
            for (int i = 0; i < m_viewDestroyed.Length; i++) {
                m_viewDestroyed[i].SetActive(true);
            }
            SetCollisionsEnabled(false);

            m_prisonerSpawner.SetEntitiesFree();

            if (!string.IsNullOrEmpty(m_onBreakSound)) {
                AudioController.Play(m_onBreakSound);
            }

            m_entity.SetDestroyedByDragon();

            m_broken = true;
        }
    }

    private void SetCollisionsEnabled(bool _value) {
        Collider[] colliders = m_colliderHolder.GetComponents<Collider>();
        for (int c = 0; c < colliders.Length; c++) {
            if(colliders[c] != null) colliders[c].isTrigger = !_value;
        }
    }

    private void FaceDragon(GameObject _ps, DragonMotion _dragonMotion) {
        Vector3 dir = _dragonMotion.direction;
        dir.z = 0;
        dir.Normalize();

        float angle = Vector3.Angle(Vector3.up, dir);
        angle = Mathf.Min(angle, 60f);
        angle *= Mathf.Sign(Vector3.Cross(Vector3.up, dir).z);
        _ps.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
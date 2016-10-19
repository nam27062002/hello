﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Entity))]
public class DestructibleDecoration : Initializable {

	private enum InteractionType {
		Collision = 0,
		GoThrough
	}

	[SerializeField] private InteractionType m_zone1Interaction = InteractionType.Collision;
	[SerializeField] private float m_knockBackStrength = 5f;
	[CommentAttribute("Add a feedback effect when this object is touched by Dragon.")]
	[SerializeField] private string m_feddbackParticle = "";
	[CommentAttribute("Add a destroy effect when this object is trampled by Dragon.")]
	[SerializeField] private string m_destroyParticle = "";

	private ZoneManager m_zoneManager;
	private ZoneManager.ZoneEffect m_effect;
	private ZoneManager.Zone m_zone;

	private GameObject m_view;

	private AutoSpawnBehaviour m_autoSpawner;
	private BoxCollider m_collider;
	private Entity m_entity;

	private Vector3 m_colliderCenter;

	private bool m_spawned = false;

	private DragonBreathBehaviour m_breath;


	//-------------------------------------------------------------------------------------------//
	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		m_entity = GetComponent<Entity>();
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_collider = GetComponent<BoxCollider>();

		m_zoneManager = GameObjectExt.FindComponent<ZoneManager>(true);
		m_zone = m_zoneManager.GetZone(transform.position.z);
		m_effect = m_zoneManager.GetDestructionEffectCode(transform.position, m_entity.sku);

		if (m_effect == ZoneManager.ZoneEffect.None) {
			if (m_collider) Destroy(m_collider);
			Destroy(m_autoSpawner);
			Destroy(m_entity);
			Destroy(this);
		} else {
			m_view = transform.FindChild("view").gameObject;
			m_colliderCenter = m_collider.center;

			if (m_zone == ZoneManager.Zone.Zone1) {
				m_collider.enabled = true;
				m_collider.isTrigger = true;

				if (m_effect == ZoneManager.ZoneEffect.S
				&&	m_zone1Interaction == InteractionType.Collision) {
					m_collider.isTrigger = false;
				}
			} else if (m_zone == ZoneManager.Zone.Zone2) {
				m_collider.enabled = true;
				m_collider.isTrigger = true;
			}

			Vector3 colliderCenterTransform = Vector3.zero;
			colliderCenterTransform.x = transform.position.x;
			colliderCenterTransform.y = transform.position.y + m_collider.center.y;
			colliderCenterTransform.z = 0;
			colliderCenterTransform = transform.InverseTransformPoint(colliderCenterTransform);
			m_collider.center = colliderCenterTransform;
		}

		m_spawned = true;
		m_breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();
	}

	public override void Initialize() {
		m_view.SetActive(true);
		m_spawned = true;

		if (m_zone == ZoneManager.Zone.Zone1) {
			switch (m_zone1Interaction) {
				case InteractionType.Collision:	m_collider.isTrigger = false;	break;
				case InteractionType.GoThrough: m_collider.isTrigger = true; 	break;
			}
		}

		m_collider.enabled = true;
	}

	void OnCollisionEnter(Collision _other) {
		if (m_spawned) {
			if (!m_breath.IsFuryOn()) {
				if (_other.gameObject.CompareTag("Player")) {
					if (_other.contacts.Length > 0) {
						ContactPoint contact = _other.contacts[0];
						if (m_feddbackParticle != "") {
							ParticleManager.Spawn(m_feddbackParticle, contact.point - (m_collider.center - m_colliderCenter));
						}
					}
				}
			}
		}
	}

	void OnTriggerEnter(Collider _other) {
		if (m_spawned) {
			if (!m_breath.IsFuryOn()) {
				if (_other.gameObject.CompareTag("Player")) {
					if (m_effect == ZoneManager.ZoneEffect.S) {
						if (m_feddbackParticle != "") {
							Vector3 particlePosition = transform.position + m_colliderCenter;
							particlePosition.y = _other.transform.position.y;

							if (particlePosition.x < _other.transform.position.x) {
								particlePosition.x += m_collider.size.x * 0.5f;
							} else {
								particlePosition.x -= m_collider.size.x * 0.5f;
							}
							ParticleManager.Spawn(m_feddbackParticle, particlePosition);
						}
					} else {
						if (m_destroyParticle != "") {
							ParticleManager.Spawn(m_destroyParticle, transform.position);
						}

						if (m_zone == ZoneManager.Zone.Zone1 && m_knockBackStrength > 0f) {
							DragonMotion dragonMotion = m_breath.GetComponent<DragonMotion>();

							Vector3 knockBack = dragonMotion.transform.position - (transform.position + m_collider.center);
							knockBack.z = 0f;
							knockBack.Normalize();

							knockBack *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude * m_knockBackStrength, 1f));

							dragonMotion.AddForce(knockBack);
						}

						m_autoSpawner.StartRespawn();
						m_view.SetActive(false);
						m_spawned = false;
					}
				}
			}
		}
	}

	void OnTriggerExit(Collider _other) {
		if (m_spawned) {
			if (!m_breath.IsFuryOn()) {
				if (_other.gameObject.CompareTag("Player")) {
					Vector3 particlePosition = transform.position + m_colliderCenter;
					particlePosition.y = _other.transform.position.y;

					if (particlePosition.x < _other.transform.position.x) {
						particlePosition.x += m_collider.size.x * 0.5f;
					} else {
						particlePosition.x -= m_collider.size.x * 0.5f;
					}

					if (m_effect == ZoneManager.ZoneEffect.S) {
						if (m_feddbackParticle != "") {
							ParticleManager.Spawn(m_feddbackParticle, particlePosition);
						}
					}
				}
			}
		}
	}
}

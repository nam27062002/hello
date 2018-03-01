﻿using UnityEngine;
using System.Collections;

public class DestructibleDecoration : MonoBehaviour, ISpawnable {

	private enum InteractionType {
		Collision = 0,
		GoThrough
	}

	[SerializeField] private InteractionType m_zone1Interaction = InteractionType.Collision;
	[SerializeField] private float m_knockBackStrength = 5f;

	[SerializeField] private bool m_particleFaceDragonDirection = false;

	[CommentAttribute("Add a feedback effect when this object is touched by Dragon.")]
	//[SerializeField] private string m_feddbackParticle = "";
	[SerializeField] private ParticleData  m_feedbackParticle;
	[CommentAttribute("Add a destroy effect when this object is trampled by Dragon.")]
	//[SerializeField] private string m_destroyParticle = "";
	[SerializeField] private ParticleData m_destroyParticle;

	[CommentAttribute("Audio When Dragon completely destroys the object.")]
	[SerializeField] private string m_onDestroyAudio = "";
	[CommentAttribute("Audio When Dragon interacts with object but does not destroy it.")]
	[SerializeField] private string m_onFeedbackAudio = "";


	private ZoneManager.ZoneEffect m_effect;
	private ZoneManager.Zone m_zone;

	private GameObject m_view;
	private GameObject m_viewDestroyed;
	private Corpse     m_corpse;

	private AutoSpawnBehaviour m_autoSpawner;
	private BoxCollider m_collider;
	private Decoration m_entity;

	private Vector3 m_colliderCenter;

	private bool m_spawned = false;

	private DragonBreathBehaviour m_breath;



	//-------------------------------------------------------------------------------------------//
	// Use this for initialization
	void Start() {		
		m_feedbackParticle.CreatePool();
		m_destroyParticle.CreatePool();
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.AddListener(MessengerEvents.GAME_AREA_ENTER, OnLevelLoaded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.RemoveListener(MessengerEvents.GAME_AREA_ENTER, OnLevelLoaded);
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		m_entity = GetComponent<Decoration>();
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_collider = GetComponent<BoxCollider>();
	
		m_zone = InstanceManager.zoneManager.GetZone(transform.position.z);
		m_effect = InstanceManager.zoneManager.GetDestructionEffectCode(m_entity, InstanceManager.player.GetTierWhenBreaking());

		if (m_zone == ZoneManager.Zone.None || m_effect == ZoneManager.ZoneEffect.None) {
			if (m_collider) Destroy(m_collider);
			//TODO: find a better way to clean prefabs
			//if (m_viewDestroyed) Destroy(m_viewDestroyed);
			//Destroy(m_autoSpawner);
			Destroy(this);
			//Destroy(m_entity);
		} else {
			m_view = transform.Find("view").gameObject;
			Transform viewDestroyed = transform.Find("view_destroyed");
			if (viewDestroyed != null) {
				m_viewDestroyed = viewDestroyed.gameObject;
			} else {
				m_viewDestroyed = transform.Find("view_burned").gameObject; // maybe, we'll need another game object, for now we use the burned one
			}
			m_corpse = m_viewDestroyed.GetComponent<Corpse>();
			m_colliderCenter = m_collider.center;

			if (m_zone == ZoneManager.Zone.Zone1) {
				m_collider.enabled = true;

				if (m_zone1Interaction == InteractionType.Collision) {
					m_collider.isTrigger = false;
				} else {
					m_collider.isTrigger = true;
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

	public void Spawn(ISpawner _spawner) {
		enabled = true;

		m_view.SetActive(true);
		m_viewDestroyed.SetActive(false);
		m_spawned = true;

		if (m_zone == ZoneManager.Zone.Zone1) {
			switch (m_zone1Interaction) {
				case InteractionType.Collision:	m_collider.isTrigger = false;	break;
				case InteractionType.GoThrough: m_collider.isTrigger = true; 	break;
			}
		}

		m_collider.enabled = true;
	}

	public void CustomUpdate() {}

	void OnCollisionEnter(Collision _other) {
		if (enabled && m_spawned) {
			if (!m_breath.IsFuryOn()) {
				if (_other.gameObject.CompareTag("Player")) {
					if (_other.contacts.Length > 0) {
						if (m_effect == ZoneManager.ZoneEffect.S) {
							ContactPoint contact = _other.contacts[0];
							GameObject ps = m_feedbackParticle.Spawn(contact.point - (m_collider.center - m_colliderCenter) + m_feedbackParticle.offset);
							if (ps != null) {
								if (m_particleFaceDragonDirection) {
									FaceDragon(ps);
								}
							}
						} else {
							Break();
						}
					}
				}
			}
		}
	}

	void OnTriggerEnter(Collider _other) {
		if (enabled && m_spawned) {
			if (!m_breath.IsFuryOn()) {
				if (_other.gameObject.CompareTag("Player")) {
					if (m_effect == ZoneManager.ZoneEffect.S) {
						GameObject ps = m_feedbackParticle.Spawn();
						if (ps != null) {
							Vector3 particlePosition = transform.position + m_colliderCenter;
							particlePosition.y = _other.transform.position.y;

							if (particlePosition.x < _other.transform.position.x) {
								particlePosition.x += m_collider.size.x * 0.5f;
							} else {
								particlePosition.x -= m_collider.size.x * 0.5f;
							}

							ps.transform.position = particlePosition + m_feedbackParticle.offset;

							if (m_particleFaceDragonDirection) {
								FaceDragon(ps);
							}
						}
					
						if (!string.IsNullOrEmpty(m_onFeedbackAudio))
							AudioController.Play(m_onFeedbackAudio, transform.position + m_colliderCenter);
					} else {
						Break();
					}
				}
			}
		}
	}

	void OnTriggerExit(Collider _other) {
		if (enabled && m_spawned) {
			if (!m_breath.IsFuryOn()) {
				if (_other.gameObject.CompareTag("Player")) {
					if (m_effect == ZoneManager.ZoneEffect.S) {
						Vector3 particlePosition = transform.position + m_colliderCenter;
						particlePosition.y = _other.transform.position.y;

						if (particlePosition.x < _other.transform.position.x) {
							particlePosition.x += m_collider.size.x * 0.5f;
						} else {
							particlePosition.x -= m_collider.size.x * 0.5f;
						}

						GameObject ps = m_feedbackParticle.Spawn(particlePosition + m_feedbackParticle.offset);
						if (ps != null) {
							if (m_particleFaceDragonDirection) {
								FaceDragon(ps);
							}
						}

						if (!string.IsNullOrEmpty(m_onFeedbackAudio))
							AudioController.Play(m_onFeedbackAudio, transform.position + m_colliderCenter);
					}
				}
			}
		}
	}

	void Break() {
		GameObject ps = m_destroyParticle.Spawn(transform.position + m_destroyParticle.offset);
		if (ps != null) {
			if (m_particleFaceDragonDirection) {
				FaceDragon(ps);
			}
		}

		if (m_zone == ZoneManager.Zone.Zone1 && m_knockBackStrength > 0f) {
			DragonMotion dragonMotion = m_breath.GetComponent<DragonMotion>();

			Vector3 knockBack = dragonMotion.transform.position - (transform.position + m_collider.center);
			knockBack.z = 0f;
			knockBack.Normalize();

			knockBack *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude * m_knockBackStrength, 1f));

			dragonMotion.AddForce(knockBack);
		}

		if (!string.IsNullOrEmpty(m_onDestroyAudio))
			AudioController.Play(m_onDestroyAudio, transform.position + m_collider.center);

		m_view.SetActive(false);
		m_viewDestroyed.SetActive(true);
		if (m_autoSpawner) m_autoSpawner.StartRespawn();
		if (m_corpse != null) m_corpse.Spawn(false, false);

		m_spawned = false;

		m_collider.isTrigger = true;

		// Update some die status data
		m_entity.onDieStatus.source = IEntity.Type.PLAYER;

		// [AOC] Notify game!
		Messenger.Broadcast<Transform, Reward>(MessengerEvents.ENTITY_DESTROYED, transform, m_entity.reward);
	}

	void FaceDragon(GameObject _ps) {
		DragonMotion dragonMotion = m_breath.GetComponent<DragonMotion>();
		Vector3 dir = dragonMotion.direction;
		dir.z = 0;
		dir.Normalize();

		float angle = Vector3.Angle(Vector3.up, dir);
		angle = Mathf.Min(angle, 60f);
		angle *= Mathf.Sign(Vector3.Cross(Vector3.up, dir).z);
		_ps.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}
}

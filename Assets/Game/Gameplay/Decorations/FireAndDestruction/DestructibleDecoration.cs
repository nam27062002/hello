using UnityEngine;
using System.Collections;

public class DestructibleDecoration : ISpawnable, IBroadcastListener {

	private enum InteractionType {
		Collision = 0,
		GoThrough
	}

	[SerializeField] private InteractionType m_zone1Interaction = InteractionType.Collision;
	[SerializeField] private float m_knockBackStrength = 5f;
	[SerializeField] private float m_damageOnDestruction = 0f;

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

	[SeparatorAttribute]
	[SerializeField] private float m_cameraShake = 0;

    //------
    public delegate void OnDestroyDelegate();
    public OnDestroyDelegate onDestroy;
    //------

    private Transform m_transform;

    private ZoneManager.ZoneEffect m_effect;
	private ZoneManager.Zone m_zone;

	private GameObject m_view;
	private GameObject m_viewDestroyed;
	private Corpse     m_corpse;

	private AutoSpawnBehaviour m_autoSpawner;
	private InflammableDecoration m_inflammableBehaviour;
	private BoxCollider m_collider;
	private Decoration m_entity;

	private Vector3 m_colliderCenter;

	private bool m_spawned = false;

	private DragonMotion m_dragonMotion;
	private DragonHealthBehaviour m_dragonHealth;
	private DragonBreathBehaviour m_dragonBreath;
    
    private Renderer[] m_viewBurnedRenderes = null;



	//-------------------------------------------------------------------------------------------//
    
    void Awake()
    {
        m_transform = transform;

        // Subscribe to external events
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
    }
    
	// Use this for initialization
	void Start() {		
		m_feedbackParticle.CreatePool();
		m_destroyParticle.CreatePool();
	}

	private void OnDestroy() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                if ( gameObject.name.Contains("PF_Catapult") )
                {
                        Debug.Log("Hola!");
                }
                OnLevelLoaded();
            }break;
        }
    }
    
	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		m_entity = GetComponent<Decoration>();
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_inflammableBehaviour = GetComponent<InflammableDecoration>();
		m_collider = GetComponent<BoxCollider>();
	
		m_zone = InstanceManager.zoneManager.GetZone(m_transform.position.z);
		m_effect = InstanceManager.zoneManager.GetDestructionEffectCode(m_entity, InstanceManager.player.GetTierWhenBreaking());

		if (m_zone == ZoneManager.Zone.None || m_effect == ZoneManager.ZoneEffect.None) {
			if (m_collider) Destroy(m_collider);
			Destroy(this);			
		} else {
			m_view = m_transform.Find("view").gameObject;
			Transform viewDestroyed = m_transform.Find("view_destroyed");
			if (viewDestroyed != null) {
				m_viewDestroyed = viewDestroyed.gameObject;
			} else {
				m_viewDestroyed = m_transform.Find("view_burned").gameObject; // maybe, we'll need another game object, for now we use the burned one
                // Change material to red one
                m_viewBurnedRenderes = m_viewDestroyed.GetComponentsInChildren<Renderer>();   
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

			Vector3 colliderCenterTransform = m_transform.position + (m_transform.up * m_collider.center.y * m_transform.localScale.y);
			colliderCenterTransform.z = 0;
			colliderCenterTransform = m_transform.InverseTransformPoint(colliderCenterTransform);
			m_collider.center = colliderCenterTransform;
		}

		m_spawned = true;

		m_dragonMotion = InstanceManager.player.dragonMotion;
		m_dragonBreath = InstanceManager.player.breathBehaviour;
		m_dragonHealth = InstanceManager.player.dragonHealthBehaviour;
	}

	override public void Spawn(ISpawner _spawner) {
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

	override public void CustomUpdate() {}

	void OnCollisionEnter(Collision _other) {
		if (enabled && m_spawned) {
			if (!m_dragonBreath.IsFuryOn() || m_dragonBreath.isFuryPaused) {
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
			if (!m_dragonBreath.IsFuryOn() || m_dragonBreath.isFuryPaused) {
				if (_other.gameObject.CompareTag("Player")) {
					if (m_effect == ZoneManager.ZoneEffect.S) {
						GameObject ps = m_feedbackParticle.Spawn();
						if (ps != null) {
							Vector3 particlePosition = m_transform.position + m_colliderCenter;
							particlePosition.y = _other.transform.position.y;

							if (particlePosition.x < _other.transform.position.x) {
								particlePosition.x += m_collider.size.x * 0.5f;
							} else {
								particlePosition.x -= m_collider.size.x * 0.5f;
							}

							ps.transform.localRotation = m_transform.rotation;
							ps.transform.position = particlePosition + m_feedbackParticle.offset;

							if (m_particleFaceDragonDirection) {
								FaceDragon(ps);
							}
						}
					
						if (!string.IsNullOrEmpty(m_onFeedbackAudio))
							AudioController.Play(m_onFeedbackAudio, m_transform.position + m_colliderCenter);
					} else {
						Break();
					}
				}
			}
		}
	}

	void OnTriggerExit(Collider _other) {
		if (enabled && m_spawned) {
			if (!m_dragonBreath.IsFuryOn() || m_dragonBreath.isFuryPaused) {
				if (_other.gameObject.CompareTag("Player")) {
					if (m_effect == ZoneManager.ZoneEffect.S) {
						Vector3 particlePosition = m_transform.position + m_colliderCenter;
						particlePosition.y = _other.transform.position.y;

						if (particlePosition.x < _other.transform.position.x) {
							particlePosition.x += m_collider.size.x * 0.5f;
						} else {
							particlePosition.x -= m_collider.size.x * 0.5f;
						}

						GameObject ps = m_feedbackParticle.Spawn(particlePosition + (m_transform.rotation * m_feedbackParticle.offset));
						if (ps != null) {
							ps.transform.localRotation = m_transform.rotation;
							if (m_particleFaceDragonDirection) {
								FaceDragon(ps);
							}
						}

						if (!string.IsNullOrEmpty(m_onFeedbackAudio))
							AudioController.Play(m_onFeedbackAudio, m_transform.position + m_colliderCenter);
					}
				}
			}
		}
	}
    
    public bool CanBreakByShooting()
    {
        return m_effect != ZoneManager.ZoneEffect.S && enabled && m_spawned;
    }

	public void Break(bool _player = true) {
    
        if (m_viewBurnedRenderes != null)
        {
            BurnedView();
        }

        GameObject ps = m_destroyParticle.Spawn(m_transform.position + (m_transform.rotation * m_destroyParticle.offset), m_transform.rotation);
		if (ps != null) {
			if (m_particleFaceDragonDirection) {
				FaceDragon(ps);
			}
		}

		if (m_zone == ZoneManager.Zone.Zone1 && m_knockBackStrength > 0f) {
			Vector3 knockBack = m_dragonMotion.transform.position - (m_transform.position + m_collider.center);
			knockBack.z = 0f;
			knockBack.Normalize();

			knockBack *= Mathf.Log(Mathf.Max(m_dragonMotion.velocity.magnitude * m_knockBackStrength, 1f));

			m_dragonMotion.AddForce(knockBack);
		}

		if (m_damageOnDestruction > 0) {
			m_dragonHealth.ReceiveDamage(m_damageOnDestruction, DamageType.NORMAL);
		}

		if (!string.IsNullOrEmpty(m_onDestroyAudio))
			AudioController.Play(m_onDestroyAudio, m_transform.position + m_collider.center);

        if (onDestroy != null)
            onDestroy();

        m_view.SetActive(false);
		m_viewDestroyed.SetActive(true);
		if (m_autoSpawner) m_autoSpawner.StartRespawn();
		if (m_inflammableBehaviour != null) m_inflammableBehaviour.enabled = false;
		if (m_corpse != null) m_corpse.Spawn(false, false);

		m_spawned = false;

		m_collider.isTrigger = true;

		// Update some die status data
		m_entity.onDieStatus.source = IEntity.Type.PLAYER;

		// [AOC] Notify game!
        Messenger.Broadcast<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, m_transform, m_entity, m_entity.reward, KillType.HIT);

        if (m_cameraShake > 0) {
			Messenger.Broadcast<float, float>(MessengerEvents.CAMERA_SHAKE, m_cameraShake, 1f);
		}
        
        if ( _player )
        {
            InstanceManager.timeScaleController.HitStop();
        }
	}
    
    private void BurnedView()
    {
        Material burnedMaterial = FireColorSetupManager.instance.GetDecorationBurnedMaterial( FireColorSetupManager.FireColorType.RED );
        int max = m_viewBurnedRenderes.Length;
        for (int i = 0; i < max; i++) {
            Material[] materials = m_viewBurnedRenderes[i].materials;
            for (int m = 0; m < materials.Length; m++) {
                materials[m] = burnedMaterial;
            }
            m_viewBurnedRenderes[i].materials = materials;
        }
    }

	void FaceDragon(GameObject _ps) {
		DragonMotion dragonMotion = m_dragonBreath.GetComponent<DragonMotion>();
		Vector3 dir = dragonMotion.direction;
		dir.z = 0;
		dir.Normalize();

		float angle = Vector3.Angle(Vector3.up, dir);
		angle = Mathf.Min(angle, 60f);
		angle *= Mathf.Sign(Vector3.Cross(Vector3.up, dir).z);
		_ps.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}
}

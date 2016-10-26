using UnityEngine;
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

	[CommentAttribute("Audio When Dragon completely destroys the object.")]
	[SerializeField] private string m_onDestroyAudio = "";
	[CommentAttribute("Audio When Dragon interacts with object but does not destroy it.")]
	[SerializeField] private string m_onFeedbackAudio = "";


	private ZoneManager m_zoneManager;
	private ZoneManager.ZoneEffect m_effect;
	private ZoneManager.Zone m_zone;

	private GameObject m_view;
	private GameObject m_viewDestroyed;

	private AutoSpawnBehaviour m_autoSpawner;
	private BoxCollider m_collider;
	private Entity m_entity;

	private Vector3 m_colliderCenter;

	private bool m_spawned = false;

	private DragonBreathBehaviour m_breath;




	//-------------------------------------------------------------------------------------------//
	// Use this for initialization
	void Start() {		
		ParticleManager.CreatePool(m_feddbackParticle);
		ParticleManager.CreatePool(m_destroyParticle);
	}

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
			if (m_viewDestroyed) Destroy(m_viewDestroyed);
			Destroy(m_autoSpawner);
			Destroy(this);
			Destroy(m_entity);
		} else {
			m_view = transform.FindChild("view").gameObject;
			m_viewDestroyed = transform.FindChild("view_burned").gameObject; // maybe, we'll need another game object, for now we use the burned one
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

	void OnCollisionEnter(Collision _other) {
		if (enabled && m_spawned) {
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
		if (enabled && m_spawned) {
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

						if ( !string.IsNullOrEmpty(m_onFeedbackAudio) )
							AudioController.Play(m_onFeedbackAudio, transform.position + m_colliderCenter);
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

						if ( !string.IsNullOrEmpty(m_onDestroyAudio) )
							AudioController.Play(m_onDestroyAudio, transform.position + m_collider.center);

						m_autoSpawner.StartRespawn();
						m_view.SetActive(false);
						m_viewDestroyed.SetActive(true);
						m_spawned = false;


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

						if ( m_feddbackParticle != "") {
							ParticleManager.Spawn(m_feddbackParticle, particlePosition);
						}

						if ( !string.IsNullOrEmpty(m_onFeedbackAudio) )
							AudioController.Play(m_onFeedbackAudio, transform.position + m_colliderCenter);
					}
				}
			}
		}
	}
}

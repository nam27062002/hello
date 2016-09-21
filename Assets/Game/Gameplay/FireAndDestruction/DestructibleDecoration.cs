using UnityEngine;
using System.Collections;

public class DestructibleDecoration : Initializable {

	private enum DestructionType {
		Collision = 0,
		GoThrough,
		DestroyAndKB
	}

	[SerializeField] private DestructionType m_zone1Effect = DestructionType.Collision;
	[CommentAttribute("Add a destroy effect when this object is trampled by Dragon.")]
	[SerializeField] private string m_destroyParticle = "";

	private ZoneManager m_zoneManager;
	private ZoneManager.ZoneEffect m_effect;
	private ZoneManager.Zone m_zone;

	private GameObject m_view;

	private AutoSpawnBehaviour m_autoSpawner;
	private BoxCollider m_collider;
	private Entity m_entity;

	private bool m_spawned = false;
	private bool m_addKnockBack = false;

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
		m_effect = m_zoneManager.GetFireEffectCode(transform.position, m_entity.sku);

		if (m_effect == ZoneManager.ZoneEffect.None) {
			if (m_collider) Destroy(m_collider);
			Destroy(m_autoSpawner);
			Destroy(m_entity);
			Destroy(this);
		} else {
			m_view = transform.FindChild("view").gameObject;

			Vector3 center = m_collider.center;
			if (m_zone == ZoneManager.Zone.Zone1) {
				switch (m_zone1Effect) {
					case DestructionType.Collision:
						m_collider.enabled = true;
						m_collider.isTrigger = false;
						center.z = 0f;
						break;

					case DestructionType.GoThrough:
						m_collider.enabled = false;
						break;

					case DestructionType.DestroyAndKB:
						m_addKnockBack = true;
						m_collider.enabled = true;
						m_collider.isTrigger = true;
						center.z = 0f;
						break;
				}
			} else if (m_zone == ZoneManager.Zone.Zone2) {
				m_collider.enabled = true;
				m_collider.isTrigger = true;
				center.z = -transform.position.z;
			}
			m_collider.center = center;
		}

		m_spawned = true;
		m_breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();
	}

	public override void Initialize() {
		m_view.SetActive(true);
		m_spawned = true;

		if (m_zone == ZoneManager.Zone.Zone1) {
			switch (m_zone1Effect) {
				case DestructionType.Collision:		m_collider.enabled = true;	break;
				case DestructionType.GoThrough: 	m_collider.enabled = false; break;
				case DestructionType.DestroyAndKB:	m_collider.enabled = true;	break;
			}
		} else if (m_zone == ZoneManager.Zone.Zone2) {
			m_collider.enabled = true;
		}
	}

	void OnTriggerEnter(Collider _other) {
		if (m_spawned) {
			if (!m_breath.IsFuryOn()) {
				if (_other.gameObject.CompareTag("Player")) {
					if (m_destroyParticle != "") {
						ParticleManager.Spawn(m_destroyParticle, transform.position);
					}

					if (m_addKnockBack) {
						DragonMotion dragonMotion = m_breath.GetComponent<DragonMotion>();

						Vector3 knockBack = dragonMotion.transform.position - transform.position;
						knockBack.z = 0f;
						knockBack.Normalize();

						knockBack *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude * 5f, 2f));

						dragonMotion.AddForce(knockBack);
					}

					m_autoSpawner.Respawn();
					m_view.SetActive(false);
					m_spawned = false;
				}
			}
		}
	}
}

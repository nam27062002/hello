using UnityEngine;

public abstract class IMeleeWeapon : MonoBehaviour {

	[SerializeField] private float m_timeBetweenHits = 0.5f;
	[SerializeField] protected DamageType m_damageType = DamageType.NORMAL;

	protected Transform m_transform;
	private Collider m_weapon;

	protected float m_damage;
	public float damage { set { m_damage = value; } }

	protected Entity m_entity;
	public Entity entity { set { m_entity = value; } }

	private float m_timer;
	private float m_timerPosition;
	protected Vector3 m_lastPosition;


	void Awake() {
		m_transform = transform;
		m_weapon = GetComponent<Collider>();
	}

	void OnEnable() {
		m_weapon.enabled = true;
		m_lastPosition = m_transform.position;

		m_timer = 0;
		m_timerPosition = 0.25f;

		OnEnabled();
	}

	void OnDisable() {
		m_weapon.enabled = false;

		OnDisabled();
	}

	void Update() {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
		}

		m_timerPosition -= Time.deltaTime;
		if (m_timerPosition <= 0f) {
			m_lastPosition = m_transform.position;
			m_timerPosition = 0.25f;
		}
	}

	void OnTriggerEnter(Collider _other) {
		if (m_timer <= 0f && _other.CompareTag("Player")) {
			OnDealDamage();
			m_timer = m_timeBetweenHits;
		}
	}


	//----------------------------------------------------
	protected abstract void OnEnabled();
	protected abstract void OnDisabled();
	protected abstract void OnDealDamage();

}
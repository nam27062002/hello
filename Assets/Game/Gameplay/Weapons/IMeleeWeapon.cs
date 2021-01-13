using UnityEngine;

public abstract class IMeleeWeapon : MonoBehaviour {
    public enum Activation {
        Manual = 0,
        OnEnable
    }

    [SerializeField] private Activation m_activation = Activation.Manual;

    [SerializeField] protected float m_damage = 1f;
    public float damage { set { m_damage = value; } }

    [SerializeField] private float m_timeBetweenHits = 0.5f;
	[SerializeField] protected DamageType m_damageType = DamageType.NORMAL;

    protected Transform m_transform;
	private Collider[] m_weapon;

    protected IEntity m_entity;
    public IEntity entity { set { m_entity = value; } }

	private float m_timer;
	private float m_timerPosition;
	protected Vector3 m_lastPosition;


	void Awake() {
		m_transform = transform;
		m_weapon = GetComponents<Collider>();

        if (!enabled) {
            for (int i = 0; i < m_weapon.Length; ++i) {
                m_weapon[i].enabled = false;
            }
        }

        OnAwake();
	}

    private void OnEnable() {
        if (m_activation == Activation.OnEnable) {
            EnableWeapon();
        }
    }

    private void OnDisable() {
        if (m_activation == Activation.OnEnable) {
            DisableWeapon();
        }
    }

    public void EnableWeapon() {
		for (int i = 0; i < m_weapon.Length; ++i) {
			m_weapon[i].enabled = true;
		}
		m_lastPosition = m_transform.position;

		m_timer = 0;
		m_timerPosition = 0.25f;

		OnEnabled();
	}

    public void DisableWeapon() {
		for (int i = 0; i < m_weapon.Length; ++i) {
			m_weapon[i].enabled = false;
		}
		OnDisabled();
	}

    protected virtual void Update() {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
		}

		m_timerPosition -= Time.deltaTime;
		if (m_timerPosition <= 0f) {
			m_lastPosition = m_transform.position;
			m_timerPosition = 0.25f;
		}
	}

    protected virtual void OnTriggerEnter(Collider _other) {
		if (m_timer <= 0f && _other.CompareTag("Player")) {
			OnDealDamage();
			m_timer = m_timeBetweenHits;
		}
	}


    //----------------------------------------------------
    protected abstract void OnAwake();
	protected abstract void OnEnabled();
	protected abstract void OnDisabled();
	protected abstract void OnDealDamage();

}
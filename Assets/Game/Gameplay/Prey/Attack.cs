using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Attack")]
public class Attack : Steering {

	[SerializeField] private float m_range;
	[SerializeField] private float m_delay;
	[SerializeField] private float m_damage;


	private float m_timer;
	private float m_rangeSqr;
	private bool m_inRange;

	private int m_hitCount;
	public int hitCount { get { return m_hitCount; } }

	private DragonHealthBehaviour m_dragon;
	protected Animator m_animator;


	void Awake() {

		m_dragon = null;
		enabled = false;
		m_inRange = false;
		
		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}

	void OnEnable() {

		m_timer = 0;
		m_hitCount = 0;
		m_rangeSqr = m_range * m_range;
		m_dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
	}

	void OnDisable() {

		m_dragon = null;
	}

	public bool InRange() {
		return enabled && m_inRange;
	}

	void Update() {

		Vector2 v = transform.position - m_dragon.transform.position;
		m_inRange = v.sqrMagnitude <= m_rangeSqr;
		if (m_inRange) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_hitCount++;
				m_timer = m_delay;
				m_dragon.ReceiveDamage(m_damage);
				m_animator.SetTrigger("attack");
				if (!m_dragon.IsAlive()) {
					enabled = false;
				}
			}
		}
	}
}

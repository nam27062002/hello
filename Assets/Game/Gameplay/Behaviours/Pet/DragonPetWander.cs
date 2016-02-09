using UnityEngine;
using System.Collections;

public class DragonPetWander : MonoBehaviour {
	private DragonPlayer m_player;

	private PreyMotion m_motion;
	private Animator m_animator;

	private Vector2 m_target;
	private float m_timer;
	private float m_t;

	private bool m_idle;

	// Use this for initialization
	void Awake() {
		m_player = InstanceManager.player;
		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}

	void OnEnable() {
		m_idle = true;
		m_timer = 2f;

		m_animator.SetBool("fly", true);
		m_motion.SetSpeedMultiplier(0.75f);
	}

	void OnDisable() {
		m_motion.Stop();
		m_animator.SetBool("fly", false);
		m_motion.SetSpeedMultiplier(1f);
	}

	// Update is called once per frame
	void Update() {
		m_timer -= Time.deltaTime;
		if (m_idle) {			
			if (m_timer <= 0) {
				m_idle = false;
				m_timer = Random.Range(5, 8);
			}
		} else {
			if (m_timer <= 0) {
				m_idle = true;
				m_motion.Stop();
				m_timer = Random.Range(2, 4);
			}

			m_t += Time.smoothDeltaTime * 2f;

			m_target = m_player.transform.position;
			m_target.x += (Mathf.Sin(m_t * 0.75f) * 0.5f + Mathf.Cos(m_t * 0.25f) * 0.5f) * 4f;
			m_target.y += (Mathf.Sin(m_t * 0.35f) * 0.5f + Mathf.Cos(m_t * 0.65f) * 0.5f) * 4f;
		}
	}

	void FixedUpdate() {
		if (!m_idle) {
			m_motion.Seek(m_target);
		}
	}

	void OnDrawGizmos() {
		Gizmos.DrawSphere(m_target, 0.5f);
	}
}

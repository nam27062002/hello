using UnityEngine;
using System.Collections;

public class DragonPetTactics : MonoBehaviour {

	enum State {
		None = 0,
		Wander,
		Follow,
		Hunt
	};

	[SerializeField] private float m_sensePreyRadius = 10f;
	[SerializeField] private float m_sensePreyTime = 2f;

	// Behaviours
	private SensePlayer m_playerSensor;
	private FollowTarget m_follow;
	private DragonPetWander m_wander;
	private DragonPetEatBehaviour m_eat;

	private DragonMotion m_player;
	private PreyMotion m_target;

	private float m_timer;

	private State m_state;
	private State m_nextState;


	// Use this for initialization
	void Start () {
		m_player = InstanceManager.player.GetComponent<DragonMotion>();
		m_target = null;

		m_playerSensor = GetComponent<SensePlayer>();

		m_eat = GetComponent<DragonPetEatBehaviour>();

		m_follow = GetComponent<FollowTarget>();
		m_follow.enabled = false;

		m_wander = GetComponent<DragonPetWander>();
		m_wander.enabled = false;

		m_state = State.None;
		m_nextState = State.Wander;
	}
		
	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();		
		}

		if (m_state == State.Hunt) {
			if (m_target.isActiveAndEnabled == false) {
				SearchTarget();
				if (m_target == null) {
					m_nextState = State.Follow;
				} else {
					m_follow.SetTarget(m_target);
				}
			}
		} else {
			if (m_state == State.Wander) {
				if (!m_playerSensor.isInsideMaxArea) {
					m_nextState = State.Follow;
				}
			} else if (m_state == State.Follow) {
				if (m_playerSensor.isInsideMinArea) {
					m_nextState = State.Wander;
				}
			}

			// search for a new target
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_timer = m_sensePreyTime;

				SearchTarget();
				if (m_target != null) {
					m_nextState = State.Hunt;
				}
			}
		}
	}

	private void SearchTarget() {
		Entity target = EntityManager.instance.GetEntityInRangeNearest2D(transform.position, m_sensePreyRadius, m_eat.tier);
		if (target != null) {
			m_target = target.GetComponent<PreyMotion>();
		} else {
			m_target = null;
		}
	}

	private void ChangeState() {
		switch (m_state) {
			case State.Wander:
				m_wander.enabled = false;
				m_follow.enabled = true;
				break;

			case State.Follow:				
				break;

			case State.Hunt:
				break;
		}

		switch (m_nextState) {
			case State.Wander:
				m_wander.enabled = true;
				m_follow.enabled = false;
				break;

			case State.Follow:
				m_follow.SetTarget(m_player);
				break;

			case State.Hunt:
				m_follow.SetTarget(m_target);
				break;
		}

		m_state = m_nextState;
	}
}

using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Dragon Tactics")]
[RequireComponent(typeof(WanderBehaviour))]
[RequireComponent(typeof(AttackBehaviour))]
[RequireComponent(typeof(EvadeBehaviour))]
public class DragonTactics : Initializable {

	private enum State {
		None = 0,
		Wander,
		Attack,
		Retreat
	};

	private State m_state;
	private State m_nextState;
	// private SensePlayer m_sensor;

	private WanderBehaviour m_wander;
	private AttackBehaviour m_attack;
	private EvadeBehaviour m_evade;

	// Use this for initialization
	void Start () {
		// m_sensor = GetComponent<SensePlayer>();
	}

	public override void Initialize() {
		//start at random anim position
		m_state = State.None;
		m_nextState = State.Wander;
		
		m_wander = GetComponent<WanderBehaviour>();
		m_attack = GetComponent<AttackBehaviour>();
		m_evade = GetComponent<EvadeBehaviour>();
		m_wander.enabled = false;
		m_evade.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_attack != null) {
			switch (m_attack.state) {
				case AttackBehaviour.State.Pursuit:
				case AttackBehaviour.State.Attack:
					m_nextState = State.Attack;
					break;

				case AttackBehaviour.State.AttackRetreat:
					m_nextState = State.Retreat;
					break;

				case AttackBehaviour.State.Idle:
				default:
					m_nextState = State.Wander;
					break;
			}	
		}
	}

	private void ChangeState() {
		// exit State
		switch (m_state) {
			case State.Wander:
				m_wander.enabled = false;
				break;

			case State.Retreat:
				m_wander.enabled = false;
				m_evade.enabled = false;
				break;
		}
		
		// enter State
		switch (m_nextState) {
			case State.Wander:
				m_wander.enabled = true;
				break;

			case State.Retreat:
				m_wander.enabled = true;
				m_evade.enabled = true;
				break;
		}
		
		m_state = m_nextState;
	}
}

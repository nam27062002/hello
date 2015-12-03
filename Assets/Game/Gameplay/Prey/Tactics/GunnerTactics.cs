using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Gunner Tactics")]
[RequireComponent(typeof(WanderBehaviour))]
[RequireComponent(typeof(AttackBehaviour))]
public class GunnerTactics : Initializable {

	private enum State {
		None = 0,
		Wander,
		Attack
	};

	private State m_state;
	private State m_nextState;
	private SensePlayer m_sensor;

	private WanderBehaviour m_wander;
	private AttackBehaviour m_attack;
	private EvadeBehaviour  m_evade; // optional!


	// Use this for initialization
	void Start () {
		m_sensor = GetComponent<SensePlayer>();
	}

	public override void Initialize() {
		//start at random anim position
		m_state = State.None;
		m_nextState = State.Wander;
		
		m_wander = GetComponent<WanderBehaviour>();
		m_attack = GetComponent<AttackBehaviour>();
		m_evade  = GetComponent<EvadeBehaviour>();

		m_wander.enabled = false;
		m_attack.enabled = false;
		if (m_evade) m_evade.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_sensor.alert) {
			m_nextState = State.Attack;
		} else {
			m_nextState = State.Wander;
		}
	}

	private void ChangeState() {
		// exit State
		switch (m_state) {
			case State.Wander:
				m_wander.enabled = false;
				if (m_evade) m_evade.enabled = false;
				break;
				
			case State.Attack:
				m_attack.enabled = false;
				break;
		}
		
		// enter State
		switch (m_nextState) {
			case State.Wander:
				m_wander.enabled = true;
				if (m_evade) m_evade.enabled = true;
				break;
				
			case State.Attack:
				m_attack.enabled = true;
				break;
		}
		
		m_state = m_nextState;
	}
}

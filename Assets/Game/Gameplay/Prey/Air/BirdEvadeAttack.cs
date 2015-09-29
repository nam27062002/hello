using UnityEngine;
using System.Collections;

[AddComponentMenu("Behaviour/Prey/BirdEvadeAndAttack")]
[RequireComponent(typeof(Pursuit))]
[RequireComponent(typeof(Attack))]
[RequireComponent(typeof(Evade))]
[RequireComponent(typeof(Wander))]
[RequireComponent(typeof(SensePlayer))]
public class BirdEvadeAttack : PreyBehaviour {

	private float m_evadeTime = 2f;


	private enum State {
		Wander = 0,
		Attack,
		Evade,
	};
	
	private State m_state;
	private float m_timer;
	private DragonMotion m_player;


	void Start() {
		m_player = InstanceManager.player.GetComponent<DragonMotion>();
	}

	public override void Initialize() {
		
		base.Initialize();
		
		// Start at random anim position
		ChangeState(State.Wander);
	}
	
	// Update is called once per frame
	void Update () {
		switch (m_state) {
			
			case State.Wander:
				if (playerDetected) {
					ChangeState(State.Attack);
				}
				break;

			case State.Attack:
				if (m_attack.hitCount > 2) {
					ChangeState(State.Evade);
				}
				break;

			case State.Evade:
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					ChangeState(State.Attack);
				}
				break;
		}

		if (m_state != State.Wander) {
			if (!playerDetected) {
				ChangeState(State.Wander);
			}
		}
	}

	protected override void FixedUpdate() {
		
		// Calculate steering
		Vector2 steering = Vector2.zero;

		switch (m_state) {
			
			case State.Wander:
				steering += m_seek.GetForce(m_wander.GetTarget());
				break;
				
			case State.Attack:
				steering += m_pursuit.GetForce(m_player.transform.position, m_player.GetVelocity(), m_player.GetMaxSpeed());
				break;
				
			case State.Evade:
				steering += m_evade.GetForce(m_player.transform.position, m_player.GetVelocity(), m_player.GetMaxSpeed());
				break;
		}

		UpdateVelocity(steering);
		UpdatePosition();		
		UpdateOrientation();
		
		ApplyPosition();
	}

	private void ChangeState(State _newSate) {

		if (m_state != _newSate) {
			switch (m_state) {

				case State.Attack:
					m_attack.enabled = false;
					break;
			}

			switch (_newSate) {
				
				case State.Attack:
					m_attack.enabled = true;
					break;

				case State.Evade:
					m_timer = m_evadeTime;
					break;
			}

			m_state = _newSate;
		}
	}
}

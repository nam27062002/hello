using UnityEngine;
using System.Collections;

public class HumanBehaviour : PreyBehaviour {

	private enum State {
		Wander = 0,
		Flee,
		Afraid
	};

	private State m_state;

	private int m_groundMask;

	// Use this for initialization
	void Start () {
	
		m_groundMask = 1 << LayerMask.NameToLayer("Ground");
	}
	
	public override void Initialize() {

		base.Initialize();

		//start at random anim position
		ChangeState(State.Wander);

		UpdateCollisions();
	}

	void Update() {


		switch (m_state) {

			case State.Wander:
				if (playerDetected) {
					ChangeState(State.Flee);
				} else {
					if (m_velocity.sqrMagnitude < 0.125f * 0.125f) {
						m_animator.SetBool("run", false);
					} else {
						m_animator.SetBool("run", true);
					}
				}
				break;

			case State.Flee:
				if (playerDetected) {
					if (m_velocity.sqrMagnitude < (0.125f * 0.125f) || !m_area.Contains(m_position)) {
						ChangeState(State.Afraid);
					} 
				} else {
					ChangeState(State.Wander);
				}
				break;

			case State.Afraid:
				if (playerDetected) {
					Vector3 player = InstanceManager.player.transform.position;
					
					if (player.x < m_position.x) {
						m_direction = Vector2.left;
					} else {
						m_direction = Vector2.right;
					}
				} else {
					ChangeState(State.Wander);
				}
				break;
		}
	}

	protected override void FixedUpdate() {

		// Calculate steering
		Vector2 steering = Vector2.zero;

		if (m_state == State.Wander) {
			if (m_seek && m_wander) {
				steering += m_seek.GetForce(m_wander.GetTarget());
			}
		}

		if (m_state == State.Flee) {
			if (m_flee) {
				DragonPlayer player = InstanceManager.player;
				steering += m_flee.GetForce(player.transform.position);
			}
		}

		UpdateVelocity(steering);
		UpdateCollisions();
		UpdatePosition();		
		UpdateOrientation();

		ApplyPosition();
	}

	private void UpdateCollisions() {

		// teleport to ground
		RaycastHit ground;
		Vector3 testPosition = m_positionLast + Vector2.up * 50f;
		
		if (Physics.Linecast(testPosition, testPosition + Vector3.down * m_area.bounds.size.y * 2, out ground, m_groundMask)) {
			m_position.y = ground.point.y;
			m_velocity.y = 0;
		}
	}

	private void ChangeState(State _newSate) {

		if (m_state != _newSate) {
			// exit State
			switch (m_state) {
				case State.Wander:										
				case State.Flee:
					m_animator.SetBool("run", false);
					break;
					
				case State.Afraid:
					m_animator.SetBool("scared", false);
					break;
			}

			// enter State
			switch (_newSate) {
				case State.Wander:
				case State.Flee:
					m_animator.SetBool("run", true);
					break;

				case State.Afraid:
					m_animator.SetBool("scared", true);
					m_velocity = Vector2.zero;
					break;
			}

			m_state = _newSate;
		}
	}
}
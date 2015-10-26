using UnityEngine;
using System.Collections;

[AddComponentMenu("Behaviour/Prey/Humans")]
public class HumanBehaviour : PreyBehaviour {

	private enum State {
		Wander = 0,
		Flee,
		Afraid
	};

	private State m_state;
	private float m_timer;

	private Transform m_dragon;

	

	public override void Initialize() {

		base.Initialize();

		m_dragon = InstanceManager.player.transform;

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
					if (m_velocity.sqrMagnitude < 0.25f * 0.25f) {
						m_animator.SetBool("run", false);
					} else {
						m_animator.SetBool("run", true);
					}
				}
				break;

			case State.Flee:
				if (!playerDetected || !m_area.Contains(m_position)) {
					ChangeState(State.Afraid);
				}
				break;

			case State.Afraid:
				Vector3 player = m_dragon.position;
				if (player.x < m_position.x) {
					m_direction = Vector2.left;
				} else {
					m_direction = Vector2.right;
				}

				if (!playerDetected) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0) {
						ChangeState(State.Wander);
					}
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

	private void ChangeState(State _newSate) {

		if (m_state != _newSate) {
			// exit State
			switch (m_state) {
				case State.Wander:
					m_animator.SetBool("run", false);
					m_animator.speed = 1f;
					break;

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
					m_animator.SetBool("run", true);
					m_animator.speed = 0.5f;
					break;

				case State.Flee:
					m_animator.SetBool("run", true);
					break;

				case State.Afraid:
					m_animator.SetBool("scared", true);
					m_velocity = Vector2.zero;
					m_timer = 5f; // 5 sconds before wandering around
					break;
			}

			m_state = _newSate;
		}
	}
}
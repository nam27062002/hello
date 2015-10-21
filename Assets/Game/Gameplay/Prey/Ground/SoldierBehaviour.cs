using UnityEngine;
using System.Collections;

[AddComponentMenu("Behaviour/Prey/Soldiers")]
[RequireComponent(typeof(Pursuit))]
[RequireComponent(typeof(Attack))]
[RequireComponent(typeof(Evade))]
[RequireComponent(typeof(Wander))]
[RequireComponent(typeof(SensePlayer))]
public class SoldierBehaviour : PreyBehaviour {

	private float m_evadeTime = 10f;


	private enum State {
		Wander = 0,
		Pursuit,
		Evade,
	};
	
	private State m_state;
	private float m_timer;
	private DragonMotion m_player;
	private Transform m_gun;
	private Transform m_gunShot;

	void Start() {
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/PF_Spark"), 4, true);

		m_player = InstanceManager.player.GetComponent<DragonMotion>();
		m_gun	  = transform.FindSubObjectTransform("gun");
		m_gunShot = transform.FindSubObjectTransform("gun_shot");
	}

	public override void Initialize() {
		
		base.Initialize();
		
		// Start at random anim position
		ChangeState(State.Wander);

		UpdateCollisions();
	}
	
	// Update is called once per frame
	void Update () {
		switch (m_state) {
			
			case State.Wander:
				if (playerDetected) {
					ChangeState(State.Pursuit);
				} else if (m_velocity.sqrMagnitude < 0.1f * 0.1f) {
					m_animator.SetBool("moving", false);
				} else if (m_velocity.sqrMagnitude < 0.25f * 0.25f) {
					m_animator.SetBool("moving", true);
					m_animator.SetBool("running", false);
				} else {
					m_animator.SetBool("moving", true);
					m_animator.SetBool("running", true);
				}
				break;

			case State.Pursuit:
				if (m_attack.hitCount > 20) {
					ChangeState(State.Evade);
				} else if (m_velocity.sqrMagnitude < 0.1f * 0.1f) {
					m_animator.SetBool("moving", false);
				} else if (m_velocity.sqrMagnitude < 0.25f * 0.25f) {
					m_animator.SetBool("moving", true);
					m_animator.SetBool("running", false);
				} else {
					m_animator.SetBool("moving", true);
					m_animator.SetBool("running", true);
				}
				break;

			case State.Evade:
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					ChangeState(State.Pursuit);
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
				
			case State.Pursuit:
				if (m_attack.InRange()) {
					steering += m_seek.GetForce(transform.position);
				} else {
					steering += m_pursuit.GetForce(m_player.transform.position, m_player.GetVelocity(), m_player.GetMaxSpeed());
				}
				break;
				
			case State.Evade:
				steering += m_evade.GetForce(m_player.transform.position, m_player.GetVelocity(), m_player.GetMaxSpeed());
				break;
		}

		if (m_flock && m_flock.HasController()) {
			steering += m_flock.GetForce();
		}

		UpdateVelocity(steering);
		UpdateCollisions();
		UpdatePosition();		
		UpdateOrientation();
		
		ApplyPosition();
	}

	private void ChangeState(State _newSate) {

		if (m_state != _newSate) {
			switch (m_state) {
			case State.Wander:
					m_animator.SetBool("moving", false);
					break;

				case State.Pursuit:
					m_attack.enabled = false;
					m_animator.SetBool("moving", false);
					break;

				case State.Evade:
					m_animator.SetBool("moving", false);
					break;
			}

			switch (_newSate) {
				case State.Wander:
					m_animator.SetBool("moving", true);
					m_animator.SetBool("running", false);
					break;

				case State.Pursuit:
					m_attack.enabled = true;
					m_animator.SetBool("moving", true);
					break;

				case State.Evade:
					m_timer = m_evadeTime;
					m_animator.SetBool("moving", true);
					m_animator.SetBool("running", false);
					break;
			}

			m_state = _newSate;
		}
	}

	override public void OnAttack() {

		Vector3 dir = m_gunShot.position - m_gun.position;
		dir.Normalize();

		GameObject spark = PoolManager.GetInstance("PF_Spark");
		spark.transform.position = m_gunShot.position + (dir * 0.1f);
		spark.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector3.Angle(dir, Vector3.up)));
	}
}

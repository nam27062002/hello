using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Drone Tactics")]
[RequireComponent(typeof(FollowPathBehaviour))]
[RequireComponent(typeof(AttackBehaviour))]
public class DroneTactics : Initializable {

	private enum State {
		None = 0,
		FollowPath,
		Attack,
		Retreat
	};

	private State m_state;
	private State m_nextState;
	private SensePlayer m_sensor;

	private AttackBehaviour m_attack;
	private FollowPathBehaviour m_follow;
	private EdibleBehaviour m_edible;

	private float m_timer;
	public bool m_canBeEatenWhileAttacking = true;

	// Use this for initialization
	void Start () {

	}

	public override void Initialize() {
		//start at random anim position
		m_state = State.None;
		m_nextState = State.FollowPath;
		
		m_follow = GetComponent<FollowPathBehaviour>();
		m_follow.enabled = false;
		m_attack = GetComponent<AttackBehaviour>();

		m_sensor = GetComponent<SensePlayer>();
		m_sensor.enabled = true;

		m_timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		switch( m_attack.state )
		{
			case AttackBehaviour.State.Pursuit:
			case AttackBehaviour.State.Attack:
			{
				m_nextState = State.Attack;
			}break;
			case AttackBehaviour.State.AttackRetreat:
			{
				m_nextState = State.Retreat;
			}break;
			case AttackBehaviour.State.Idle:
			default:
			{
				m_nextState = State.FollowPath;
			}break;
		}
	}

	private void ChangeState() {
		// exit State
		switch (m_state) {
			case State.FollowPath:
				m_follow.enabled = false;
				break;
				
			case State.Attack:
			case State.Retreat:
			{
				if (m_edible && !m_canBeEatenWhileAttacking)
				{
					m_edible.enabled = true;
				}
			}break;
		}
		
		// enter State
		switch (m_nextState) {
			case State.FollowPath:
				m_follow.enabled = true;
				break;
				
			case State.Attack:
			{
				if (m_edible && !m_canBeEatenWhileAttacking)
				{
					m_edible.enabled = false;
				}
			}
			break;
			case State.Retreat:
			{
				m_follow.enabled = true;
				if (m_edible && !m_canBeEatenWhileAttacking)
				{
					m_edible.enabled = false;
				}
			}break;
		}
		
		m_state = m_nextState;
	}
}

using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class WanderBehaviour : MonoBehaviour {

	protected enum State {
		None = 0,
		Idle,
		Move
	};

	[SerializeField] private float m_idleTime = 5f;
	[SerializeField][Range(0f, 1f)] private float m_idleProbability = 1f;
	
	protected PreyMotion m_motion;
	private Animator m_animator;

	private Vector2 m_target;
	private float m_timer;

	protected State m_state;
	protected State m_nextState;


	// --------------------------------------------------------------------------- //

	virtual protected void Awake() {
		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () {
		m_state = State.None;
		m_nextState = State.Idle;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_state == State.Idle) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_nextState = State.Move;
			}
		}
	}

	virtual protected void FixedUpdate() {
		if (m_state == State.Move) {
			if (m_motion.HasFlockController()) {
				ChooseTarget();
			} else {
				if ((m_target - m_motion.position).sqrMagnitude < 1f) {
					if (Random.Range(0f, 1f) < 0.5f) {
						ChooseTarget();
					} else {
						m_nextState = State.Idle;
					}
				}
			}
			
			m_motion.Seek(m_target);
			m_motion.ApplySteering();
		}
	}

	private void ChangeState() {
		if (m_nextState == State.Move) {
			m_animator.SetBool("move", true);
			ChooseTarget();
		} else {
			m_timer = m_idleTime;
			m_animator.SetBool("move", false);
		}

		m_state = m_nextState;
	}

	private void ChooseTarget() {
		if (m_motion.HasFlockController()) {
			m_target = m_motion.GetFlockTarget();
		} else {
			m_target = m_motion.area.RandomInside();
		}

		if (m_motion.HasGroundSensor()) {
			m_target = m_motion.ProjectToGround(m_target);
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(m_target, 0.5f);
	}
}

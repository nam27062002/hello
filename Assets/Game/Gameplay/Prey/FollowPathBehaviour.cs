using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyMotion))]
public class FollowPathBehaviour : Initializable {

	protected enum State {
		None = 0,
		Idle,
		Move
	};

	[SerializeField] private bool m_randomMovement = false;

	[SeparatorAttribute]
	[SerializeField][Range(0f, 1f)] private float m_idleProbability = 1f;
	[SerializeField] private Range m_moveTime = new Range(6f, 12f);
	[SerializeField] private Range m_idleTime = new Range(3f, 6f);

	private PathController m_path;
	public PathController path { set { m_path = value; } }

	private PreyMotion m_motion;
	private Animator m_animator;

	private int m_changeDirectionCountDown;

	private Vector3 m_target;

	private float m_timer;

	private State m_state;
	private State m_nextState;



	// Use this for initialization
	void Awake () {
		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}

	public override void Initialize() {			
		if (m_path != null) {
			m_target = m_path.GetNext();			
		}
	
		m_state = State.None;
		if (m_idleProbability > 0f) {
			m_nextState = State.Idle;
		} else {
			m_nextState = State.Move;
		}
	}

	void OnEnable() {
		if (m_path != null) {
			m_target = m_path.GetNearestTo(m_motion.position);
		}				

		m_state = State.None;
		if (m_idleProbability > 0f) {
			m_nextState = State.Idle;
		} else {
			m_nextState = State.Move;
		}
	}

	void OnDisable() {
		if (m_path != null) {
			m_target = m_path.GetNearestTo(m_motion.position);
		}
	}

	public void SetPath(PathController _path) {
		m_path = _path;
		if (m_path != null) {
			m_target = m_path.GetNext();
		}
	}

	void Update() {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_idleProbability > 0f && m_idleProbability < 1f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				switch (m_state) {
					case State.Idle:
						if (m_path != null) {
							m_nextState = State.Move; 
						}
						break;

					case State.Move:
						if (Random.Range(0f, 1f) < m_idleProbability) {
							m_nextState = State.Idle;
						} else {
							m_timer = m_moveTime.GetRandom();
						}
						break;
				}
			}
		}
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		if (m_path != null && m_state == State.Move) {
			if (Vector2.Distance(m_motion.position, m_target) <= m_path.radius) {
				if (m_randomMovement) {
					m_changeDirectionCountDown--;
					if (m_changeDirectionCountDown <= 0) {
						m_path.ChangeDirection();
						m_changeDirectionCountDown = Random.Range(1, m_path.count);
					}
				}
				m_target = m_path.GetNext();
			}
			m_motion.Seek(m_target);
		} else {
			m_motion.Stop();
		}
	}

	private void ChangeState() {
		if (m_nextState == State.Move) {
			m_timer = m_moveTime.GetRandom();
			m_animator.SetBool("move", true);
		} else {
			m_timer = m_idleTime.GetRandom();
			m_animator.SetBool("move", false);

			m_motion.Stop();
		}
		
		m_state = m_nextState;
	}

}

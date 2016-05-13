using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyMotion))]
public class FleePathBehaviour : Initializable {

	private enum State {
		None = 0,
		Run,		// we'll start always running from the dragon, if it gets too close
		Scared,		// then we'll start the scared animations until we have the dragon
		Panic   // over us and we'll stop moving.
	};

	private enum Direction {
		Any = 0,
		Left,
		Right
	};

	[CommentAttribute("This prey has special animations when it changes directions running.")]
	[SerializeField] private bool m_hasTurnAnimations = false;
	[CommentAttribute("This prey will stop running if the Dragon is too close.")]
	[SerializeField] private bool m_canPanic = true; 
	[CommentAttribute("When this prey is fleeing from the dragon it can move to random nodes or the node further from dragon.")]
	[SerializeField] private bool m_randomNode = false;

	private PathController m_path;
	public PathController path { set { m_path = value; } }

	private PreyMotion m_motion;
	private Animator m_animator;
	private SensePlayer m_sensor;

	private Vector3 m_target;
	private Direction m_direction;

	private State m_state;
	private State m_nextState;

	//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	// Use this for initialization
	void Awake () {
		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_sensor = GetComponent<SensePlayer>();
	}

	public override void Initialize() {			
		if (m_path != null) {
			m_target = m_path.GetNext();
		}	

		m_state = State.None;
		m_nextState = State.Run;
	}

	void OnEnable() {
		if (m_path != null) {
			m_target = m_path.GetNearestTo(m_motion.position);
		}

		m_target = Vector3.zero;
		m_state = State.None;
		m_nextState = State.Run; 
	}

	void OnDisable() {
		if (m_path != null) {
			m_target = m_path.GetNearestTo(m_motion.position);
		}

		if (m_animator.isInitialized)
			m_animator.SetBool("scared", false);
	}

	public void SetPath(PathController _path) {
		m_path = _path;
		if (m_path != null) {
			m_target = m_path.GetNext();
		}
	}

	//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	void Update() {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_state != State.Panic) {
			if (Vector2.Distance(m_motion.position, m_target) <= m_path.radius) {
				ChooseTarget();
			} 
		}

		switch (m_state) {
			case State.Run:
				if (m_sensor.isInsideMinArea) {
					m_nextState = State.Scared;
				}
				break;

			case State.Scared:
				if (m_canPanic) {
					if (m_sensor.distanceSqr < 4f){
						m_nextState = State.Panic;
					}
				}
				break;

			case State.Panic:
				if (!m_sensor.isInsideMinArea) {
					m_nextState = State.Scared;
				}
				break;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		if (m_state == State.Panic) {
			m_motion.Stop();
		} else {
			m_motion.RunTo(m_target);
		}
	}

	private void ChangeState() {
		m_animator.SetBool("move", m_nextState != State.Panic);
		m_animator.SetBool("scared", m_nextState != State.Run);

		if (m_nextState == State.Panic) {
			m_motion.Stop();
		}

		if (m_state != State.Panic) {
			ChooseTarget();
		}

		m_state = m_nextState;
	}

	private void ChooseTarget() {
		if (m_randomNode) {
			m_target = m_path.GetRandom();
		} else {
			if (transform.position.x <= m_sensor.targetPosition.x) {
				m_direction = Direction.Left;
			} else {
				m_direction = Direction.Right;
			}

			Vector3 newTarget;
			if (m_direction == Direction.Left) {
				newTarget = m_path.GetLeftmostPoint();
			} else {
				newTarget = m_path.GetRightmostPoint();
			}

			if (m_target == newTarget) {
				if (m_direction == Direction.Left) {
					m_direction = Direction.Right;
					newTarget = m_path.GetRightmostPoint();
				} else {
					m_direction = Direction.Left;
					newTarget = m_path.GetLeftmostPoint();
				}
			}

			m_target = newTarget;

			if (m_hasTurnAnimations) {
				if (m_direction == Direction.Left)	m_animator.SetTrigger("turn left");
				else 								m_animator.SetTrigger("turn right");
			}
		}
	}
}

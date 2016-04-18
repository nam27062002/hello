using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyMotion))]
public class FleePathBehaviour : Initializable {

	protected enum State {
		None = 0,
		Run,		// we'll start always running from the dragon, if it gets too close
		Scared,		// then we'll start the scared animations until we have the dragon
		Panic   // over us and we'll stop moving.
	};

	[CommentAttribute("This prey will stop running if the Dragon is too close.")]
	[SerializeField] private bool m_canPanic = true; 

	private PathController m_path;
	public PathController path { set { m_path = value; } }

	private PreyMotion m_motion;
	private Animator m_animator;
	private SensePlayer m_sensor;

	private Vector3 m_target;

	private State m_state;
	private State m_nextState;

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

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

		m_state = State.None;
		m_nextState = State.Run; 
	}

	void OnDisable() {
		if (m_path != null) {
			m_target = m_path.GetNearestTo(m_motion.position);
		}

		m_animator.SetBool("scared", false);
	}

	public void SetPath(PathController _path) {
		m_path = _path;
		if (m_path != null) {
			m_target = m_path.GetNext();
		}
	}

	//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

	void Update() {
		if (m_state != m_nextState) {
			ChangeState();
		}

		switch (m_state) {
			case State.Run:
				if (Vector2.Distance(m_motion.position, m_target) <= m_path.radius) {
					ChooseTarget();
				}

				if (m_sensor.isInsideMinArea) {
					m_nextState = State.Scared;
				}
				break;

			case State.Scared:
				if (Vector2.Distance(m_motion.position, m_target) <= m_path.radius) {
					ChooseTarget();
				}

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

		ChooseTarget();

		m_state = m_nextState;
	}

	private void ChooseTarget() {
		m_target = m_path.GetFurtherFrom(transform.position, m_sensor.targetPosition);
	}
}

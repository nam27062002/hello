using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyMotion))]
public class WanderBehaviour : Initializable {

	protected enum State {
		None = 0,
		Idle,
		Move
	};

	[SerializeField] private float m_idleTime = 5f;
	[SerializeField] private bool m_chaoticMovement = true;

	[Header("Realistic Wander")] // Experimental
	[SerializeField] private float m_displacementDistance = 1f;
	[SerializeField] private float m_displacementRadius = 1f;
	[SerializeField] private float m_angleIncrement = 10f;

	[SeparatorAttribute]
	[SerializeField][Range(0f, 1f)] private float m_idleProbability = 1f;
	
	protected PreyMotion m_motion;
	private Animator m_animator;

	protected Vector2 m_target;
	private float m_timer;

	protected State m_state;
	protected State m_nextState;

	private float m_displacementAngle;


	// --------------------------------------------------------------------------- //

	virtual protected void Awake() {
		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}
		
	public override void Initialize() {			
		OnEnable();
	}

	void OnEnable() {
		m_state = State.None;
		m_nextState = State.Idle;
		m_displacementAngle = 0;
	}

	void OnDisable() {		
		if (m_animator && m_animator.isInitialized) {
			m_animator.SetBool("move", false);
		}
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
				if (m_chaoticMovement) {
					if ((m_target - m_motion.position).sqrMagnitude < 1f) {
						if (Random.Range(0f, 1f) < m_idleProbability) {
							m_nextState = State.Idle;
						} else {
							ChooseTarget();
						}
					}
				} else {
					m_target = IncrementalMovement(); // Experimental!!!
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

			if (m_motion.direction.x < 0) {
				m_motion.direction = Vector3.left;
			} else {
				m_motion.direction = Vector3.right;
			}
		}

		m_state = m_nextState;
	}

	virtual protected void ChooseTarget() {
		if (m_motion.HasFlockController()) {
			m_target = m_motion.GetFlockTarget();
		} else {
			m_target = m_motion.area.RandomInside();
		}

		if (m_motion.HasGroundSensor()) {
			m_target = m_motion.ProjectToGround(m_target);
		}
	}

	private Vector2 IncrementalMovement() {
		Vector2 displacementCenter = m_motion.velocity;
		displacementCenter.Normalize();
		displacementCenter *= m_displacementDistance;

		Vector2 displacementForce = Vector2.right;
		displacementForce.x = Mathf.Cos(m_displacementAngle) * m_displacementRadius;
		displacementForce.y = Mathf.Sin(m_displacementAngle) * m_displacementRadius;

		m_displacementAngle += Random.Range(-m_angleIncrement, m_angleIncrement);

		return m_motion.position + displacementCenter + displacementForce;
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(m_target, 0.5f);
	}
}

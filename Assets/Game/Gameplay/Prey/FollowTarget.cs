using UnityEngine;
using System.Collections;

public class FollowTarget : Initializable {

	[SerializeField] private float m_uptadeTargetTime = 0.5f;

	private PreyMotion m_motion;
	private Animator m_animator;

	private MotionInterface m_target = null;

	private Vector3 m_targetPosition;
	private Vector3 m_targetVelocity;

	private float m_timer;

	void Awake() {
		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () {
	}

	public override void Initialize() {		

	}

	void OnEnable() {
		m_animator.SetBool("fly", true);
	}

	void OnDisable() {
		m_target = null;
		m_motion.Stop();
		m_animator.SetBool("fly", false);
	}

	public void SetTarget(MotionInterface _target) {
		m_target = _target;
		m_targetPosition = m_target.position;
		m_targetVelocity = m_target.velocity;
		m_timer = m_uptadeTargetTime;
	}

	// Update is called once per frame
	void Update () {
		if (m_target != null) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				SetTarget(m_target);
			}
		}
	}

	void FixedUpdate() {
		if (m_target != null) {
			//m_motion.Pursuit(m_targetPosition, m_targetVelocity, m_target.maxSpeed);
			m_motion.Seek(m_targetPosition);
		}
	}
}

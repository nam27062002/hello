using UnityEngine;

[DisallowMultipleComponent]
public class PreyOrientation : Orientation {

	[SerializeField] private bool m_hasTurnAnimations = false;
	[SerializeField] private bool m_faceDirection;
	public bool faceDirection { get { return m_faceDirection; } }

	private Vector3 m_direction;

	private Quaternion m_targetRotation;
	private Quaternion m_rotation;

	private Animator m_animator;

	private float m_angle;

	private bool m_turningRight;
	private bool m_turningLeft;


	// Use this for initialization
	void Awake() {
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_targetRotation = transform.rotation;
		m_rotation = transform.rotation;
		m_direction = Vector3.right;
	}

	// Update is called once per frame
	void Update() {		
		m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, Time.deltaTime * 2f);
		transform.rotation = m_rotation;
	}

	public override void SetRotation(Quaternion _rotation) {
		m_targetRotation = _rotation;
	}

	public override void SetDirection(Vector3 _direction) {
		if (m_faceDirection) {
			float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;

			m_targetRotation = Quaternion.AngleAxis(angle, Vector3.forward)*Quaternion.AngleAxis(-angle, Vector3.left);
			Vector3 eulerRot = m_targetRotation.eulerAngles;		
			if (_direction.y > 0) {
				eulerRot.z = Mathf.Min(40f, eulerRot.z);
			} else if (_direction.y < 0) {
				eulerRot.z = Mathf.Max(320f, eulerRot.z);
			}
			m_targetRotation = Quaternion.Euler(eulerRot);		
		} else {
			// Rotate so it faces the right direction (replaces 2D sprite flip)
			float angleY = 0f;

			if (m_direction.x < 0f) {
				angleY = 180f;
			}

			m_targetRotation = Quaternion.Euler(0, angleY, 0);
		}

		if (m_hasTurnAnimations && m_animator) {
			if (m_direction.x >= 0f && _direction.x < 0f) {
				m_animator.SetTrigger("turn right");
			} else if (m_direction.x < 0f && _direction.x >= 0f) {
				m_animator.SetTrigger("turn left");
			}
		}

		m_direction = _direction;
	}
}

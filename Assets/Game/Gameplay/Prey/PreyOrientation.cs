using UnityEngine;

[DisallowMultipleComponent]
public class PreyOrientation : Orientation {

	[SeparatorAttribute]
	[SerializeField] private bool m_hasTurnAnimations;
	[SerializeField] private bool m_faceDirection;
	public bool faceDirection { get { return m_faceDirection; } }

	[SeparatorAttribute]
	[SerializeField] private float m_faceLeftAngleY = 180f;
	[SerializeField] private float m_faceRightAngleY = 0f;

	private Animator m_animator;

	private Vector3 m_direction;

	private Quaternion m_targetRotation;
	private Quaternion m_rotation;

	private float m_targetAngle;
	private float m_angle;

	private bool m_turningRight;
	private bool m_turningLeft;

	// Use this for initialization
	void Awake() {
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		m_targetRotation = transform.rotation;
		m_rotation = transform.rotation;

		m_targetAngle = m_rotation.eulerAngles.y;
		m_angle = m_targetAngle;

		if (Random.Range(0f, 1f) < 0.5f) {
			m_faceLeftAngleY *= -1f;
			m_faceRightAngleY *= -1f;
		}

		m_direction = Vector3.right;
	}

	// Update is called once per frame
	void Update() {		

		float angle = 0; 

		if (m_faceDirection) {
			m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, Time.deltaTime * 2f);
			angle = Quaternion.Angle(m_rotation, m_targetRotation);
		} else {
			m_angle = Mathf.Lerp(m_angle, m_targetAngle, Time.deltaTime * 2f);
			m_rotation = Quaternion.Euler(0, m_angle, 0);

			angle = m_angle;
		}

		if (m_hasTurnAnimations) {
			if (m_turningRight) {
				// change direction
				m_turningRight = angle > 60f;
			} else if (m_turningLeft) {
				// change direction
				m_turningLeft = angle > 60f;
			}

			m_animator.SetBool("turn right", m_turningRight);
			m_animator.SetBool("turn left", m_turningLeft);
		}

		transform.rotation = m_rotation;
	}

	public override void SetRotation(Quaternion _rotation) {
		m_targetRotation = _rotation;
	}

	public void SetAngle(float _angle) {
		m_targetAngle = _angle;
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
			float angleY = m_faceRightAngleY;

			if (m_direction.x < 0f) {
				angleY = m_faceLeftAngleY;
			}

			m_targetAngle = angleY;
		}

		if (m_hasTurnAnimations) {
			if (m_direction.x >= 0f && _direction.x < 0f) {
				m_turningRight = true;
				m_turningLeft = false;
			} else if (m_direction.x < 0f && _direction.x >= 0f) {
				m_turningLeft = true;
				m_turningRight = false;
			}
		}

		m_direction = _direction;
	}
}

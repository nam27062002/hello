using UnityEngine;
using System.Collections;

public class DragonOrientation : Orientation {

	enum State {
		PLAYING,
		DYING,
		DEAD
	};

	[SerializeField] private float m_turningSpeed = 8.0f;

	private Vector3 m_direction;
	private Animator m_animator;
	private Quaternion m_targetRotation;
	private Quaternion m_rotation;

	private float angle;
	private float timer;

	private bool m_turningRight;
	private bool m_turningLeft;

	private State m_state;

	// Use this for initialization
	void Start () {
		
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_targetRotation = transform.rotation;
		m_rotation = transform.rotation;
		m_direction = Vector3.right;

		m_state = State.PLAYING;
	}
	
	// Update is called once per frame
	void LateUpdate() {
	
		if(m_state == State.PLAYING) {
			m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, Time.deltaTime * m_turningSpeed);

			float angle = Quaternion.Angle(m_rotation, m_targetRotation);
			if (m_turningRight) {
				// change direction
				m_turningRight = angle > 60f;
			} else if (m_turningLeft) {
				// change direction
				m_turningLeft = angle > 60f;
			}
			
			m_animator.SetBool("turn right", m_turningRight);
			m_animator.SetBool("turn left", m_turningLeft);

		} else if(m_state == State.DYING) {
			m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, 0.1f);
			m_targetRotation *= Quaternion.AngleAxis(200f * Time.deltaTime, Vector3.down);

			timer += Time.deltaTime;
			if (timer > 3f)
				m_state = State.DEAD;
		} else {
			m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, 0.1f);
		}

		transform.rotation = m_rotation;
	}


	public override void SetDirection(Vector3 direction) {
	
		Vector3 dir = direction.normalized;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

		m_targetRotation = Quaternion.AngleAxis(angle, Vector3.forward)*Quaternion.AngleAxis(-angle, Vector3.left);
		Vector3 eulerRot = m_targetRotation.eulerAngles;		
		if (dir.y > 0.25f) {
			eulerRot.z = Mathf.Min(40f, eulerRot.z);
		} else if (dir.y < -0.25f) {
			eulerRot.z = Mathf.Max(300f, eulerRot.z);
		}
		m_targetRotation = Quaternion.Euler(eulerRot);

		if (m_direction.x >= 0f && dir.x < 0f) {
			m_turningRight = true;
			m_turningLeft = false;
		} else if (m_direction.x < 0f && dir.x >= 0f) {
			m_turningLeft = true;
			m_turningRight = false;
		}

		m_direction = dir;
	}

	public void OnDeath() {
		m_targetRotation = Quaternion.AngleAxis(0f, Vector3.forward)*Quaternion.AngleAxis(0f, Vector3.left);
		m_state = State.DYING;
		timer = 0f;
	}
}

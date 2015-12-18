using UnityEngine;
using System.Collections;

public class DragonOrientation : MonoBehaviour {

	Vector3 m_direction;
	Animator m_animator;
	Quaternion m_targetRotation;
	Quaternion m_rotation;

	float angle;
	float timer;


	bool m_turningRight;
	bool m_turningLeft;

	float m_turningSpeed;

	enum State{

		PLAYING,
		DYING,
		DEAD
	};


	State state = State.PLAYING;

	// Use this for initialization
	void Start () {
		
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_targetRotation = transform.rotation;
		m_rotation = transform.rotation;
		m_direction = Vector3.right;

		// TODO (miguel): This should come from dragon setup
		m_turningSpeed = 8.0f; 
	}
	
	// Update is called once per frame
	void LateUpdate() {
	
		if (state == State.PLAYING) {

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

		} else if (state == State.DYING) {

			m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, 0.1f);
			m_targetRotation *= Quaternion.AngleAxis(200f * Time.deltaTime, Vector3.down);

			timer += Time.deltaTime;
			if (timer > 3f)
				state = State.DEAD;
		} else {

			m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, 0.1f);
		}

		transform.rotation = m_rotation;
	}


	public void SetDirection(Vector3 direction) {
	
		Vector3 dir = direction.normalized;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

		m_targetRotation = Quaternion.AngleAxis(angle, Vector3.forward)*Quaternion.AngleAxis(-angle, Vector3.left);
		Vector3 eulerRot = m_targetRotation.eulerAngles;		
		if (dir.y > 0) {
			eulerRot.z = Mathf.Min(40f, eulerRot.z);
		} else if (dir.y < 0) {
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
		state = State.DYING;
		timer = 0f;
	}
}

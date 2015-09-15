using UnityEngine;
using System.Collections;

public class DragonOrientation : MonoBehaviour {

	
	Quaternion m_targetRotation;
	Quaternion m_rotation;

	float angle;
	float timer;

	enum State{

		PLAYING,
		DYING,
		DEAD
	};


	State state = State.PLAYING;

	// Use this for initialization
	void Start () {
	
		m_targetRotation = transform.rotation;
		m_rotation = transform.rotation;
	}
	
	// Update is called once per frame
	void LateUpdate() {
	
		if (state == State.PLAYING) {

			m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, 0.12f);

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

		//Camera.main.GetComponent<CameraController_OLD>().SetPlayerDirection(dir);
	}

	public void OnDeath() {
		m_targetRotation = Quaternion.AngleAxis(0f, Vector3.forward)*Quaternion.AngleAxis(0f, Vector3.left);
		state = State.DYING;
		timer = 0f;
	}
}

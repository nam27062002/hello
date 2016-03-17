using UnityEngine;
using System.Collections;

public class AimTest : MonoBehaviour {

	private Animator m_animator;

	public Transform m_eye;
	public Transform m_target;

	private Vector3 m_direction;

	// Use this for initialization
	void Start () {
		m_animator = GetComponent<Animator>();
		m_animator.SetBool("attack", true);
	}
	
	// Update is called once per frame
	void Update () {
		if (m_eye != null && m_target != null) {
			Vector3 targetDir = m_target.position - m_eye.position;

			targetDir.Normalize();
			Vector3 cross = Vector3.Cross(targetDir, Vector3.right);
			float aim = cross.z * -1;

			//between aim [0.9 - 1 - 0.9] we'll rotate the model
			//for testing purpose, it'll go from 90 to 270 degrees and back. Aim value 1 is 180 degrees of rotation
			if (aim >= 0.6f) {
				float angleSide = 90f;
				if (targetDir.x < 0) {
					angleSide = 270f;
				}
				float angle = (((aim - 0.6f) / (1f - 0.6f)) * (180f - angleSide)) + angleSide;
				transform.localRotation = Quaternion.Euler(new Vector3(0, angle, 0));
			}

			// blend between attack directions
			m_animator.SetFloat("Blend", aim);
		}
	}
}

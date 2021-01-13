using UnityEngine;
using System.Collections;

public class RotationMotion : Initializable {

	[SerializeField] private Vector3 m_rotationSpeed;
		
	private Transform m_transform;
	private Vector3 m_rotation;

	void Start() {
		m_transform = transform;
		m_rotation = m_transform.localRotation.eulerAngles;
	}
	
	public override void Initialize() {		
		m_rotation = m_transform.localRotation.eulerAngles;
	}

	void LateUpdate() {
		m_rotation += m_rotationSpeed * Time.deltaTime;
		m_transform.localRotation = Quaternion.Euler(m_rotation);
	}
}

using UnityEngine;
using System.Collections;

public class RotationMotion : Initializable {

	[SerializeField] private Vector3 m_rotationSpeed;
		
	private Vector3 m_rotation;

	void Start() {
		m_rotation = transform.localRotation.eulerAngles;
	}
	
	public override void Initialize() {		
		m_rotation = transform.localRotation.eulerAngles;
	}

	void Update() {
		m_rotation += m_rotationSpeed * Time.deltaTime;
		transform.localRotation = Quaternion.Euler(m_rotation);
	}
}

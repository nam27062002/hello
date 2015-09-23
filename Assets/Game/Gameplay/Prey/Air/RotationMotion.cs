using UnityEngine;
using System.Collections;

public class RotationMotion : Initializable {

	[SerializeField] private Vector3 m_rotationSpeed;
		
	private Vector3 m_rotation;

	void Start() {
		m_rotation = transform.rotation.eulerAngles;
	}
	
	public override void Initialize() {		
		m_rotation = transform.rotation.eulerAngles;
	}

	void Update() {
		m_rotation += m_rotationSpeed * Time.deltaTime;
		transform.rotation = Quaternion.Euler(m_rotation);
	}
}

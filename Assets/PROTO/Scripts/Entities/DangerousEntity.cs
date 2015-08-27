using UnityEngine;
using System.Collections;

public class DangerousEntity : MonoBehaviour {

	public float m_FocusDistance;

	private CameraController_OLD m_CameraController;
	private Transform m_PlayerTransform;

	private float m_FocusDistanceSQR;
	private bool m_InRange;
	private bool m_enabled;

	// Use this for initialization
	void Start () {

		m_CameraController = Camera.main.GetComponent<CameraController_OLD>();
		m_PlayerTransform = GameObject.Find ("Player").transform;

		m_FocusDistanceSQR = m_FocusDistance * m_FocusDistance;
		m_InRange = false;
		m_enabled = true;
	}

	void OnEnable() {
		m_enabled = true;
	}

	void OnDisable() {
		if (m_CameraController)
			m_CameraController.SetDangerousEntity(null);
		m_InRange = false;
		m_enabled = false;
	}
	
	// Update is called once per frame
	void Update () {

		if (m_enabled) {
			float distance = (transform.position - m_PlayerTransform.position).sqrMagnitude;

			if (distance < m_FocusDistanceSQR) {
				if (!m_InRange) {
					m_CameraController.SetDangerousEntity(transform);
					m_InRange = true;
				}
			} else if (m_InRange) {			
				m_CameraController.SetDangerousEntity(null);
				m_InRange = false;
			}
		}
	}
}

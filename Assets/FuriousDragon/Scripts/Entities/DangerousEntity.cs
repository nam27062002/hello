using UnityEngine;
using System.Collections;

public class DangerousEntity : MonoBehaviour {

	public float m_FocusDistance;

	private CameraController m_CameraController;
	private Transform m_PlayerTransform;

	private float m_FocusDistanceSQR;
	private bool m_InRange;

	// Use this for initialization
	void Start () {

		m_CameraController = Camera.main.GetComponent<CameraController>();
		m_PlayerTransform = GameObject.Find ("Player").transform;

		m_FocusDistanceSQR = m_FocusDistance * m_FocusDistance;
		m_InRange = false;
	}
	
	// Update is called once per frame
	void Update () {
	
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

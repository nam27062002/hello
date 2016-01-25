using UnityEngine;
using System.Collections;

public class EntityOfInterest : MonoBehaviour {

	[SerializeField] private float m_FocusDistance;

	private GameCameraController m_camera;
	private Transform m_dragon;

	private float m_FocusDistanceSQR;
	private bool m_InRange;

	// Use this for initialization
	void Start () {
		m_camera = Camera.main.GetComponent<GameCameraController>();
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>().head;

		m_FocusDistanceSQR = m_FocusDistance * m_FocusDistance;
		m_InRange = false;
	}

	void OnDisable() {
		if (m_camera) m_camera.SetEntityOfInterest(null);
		m_InRange = false;
	}
	
	// Update is called once per frame
	void Update () {
		float distance = (transform.position - m_dragon.position).sqrMagnitude;

		if (distance < m_FocusDistanceSQR) {
			if (!m_InRange) {
				m_camera.SetEntityOfInterest(transform);
				m_InRange = true;
			}
		} else if (m_InRange) {			
			m_camera.SetEntityOfInterest(null);
			m_InRange = false;
		}
	}
}

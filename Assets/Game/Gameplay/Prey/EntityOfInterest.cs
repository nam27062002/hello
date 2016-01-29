using UnityEngine;
using System.Collections;

public class EntityOfInterest : MonoBehaviour {

	[SerializeField] private float m_FocusDistance;

	private GameCameraController m_camera;
	private DragonPlayer m_dragon;
	private Transform m_dragonHead;

	private float m_FocusDistanceSQR;
	private bool m_InRange;

	// Use this for initialization
	void Start () {
		m_camera = Camera.main.GetComponent<GameCameraController>();
		m_dragon = InstanceManager.player;
		m_dragonHead = m_dragon.GetComponent<DragonMotion>().head;

		m_FocusDistanceSQR = m_FocusDistance * m_FocusDistance;
		m_InRange = false;
	}

	void OnDisable() {
		if (m_camera) m_camera.SetEntityOfInterest(null);
		m_InRange = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_dragon.IsAlive()) {
			float distance = (transform.position - m_dragonHead.position).sqrMagnitude;

			if (distance < m_FocusDistanceSQR) {
				if (!m_InRange) {
					m_camera.SetEntityOfInterest(transform);
					m_InRange = true;
				}
			} else if (m_InRange) {			
				m_camera.SetEntityOfInterest(null);
				m_InRange = false;
			}
		} else {
			if (m_InRange) {			
				m_camera.SetEntityOfInterest(null);
				m_InRange = false;
			}
		}
	}
}

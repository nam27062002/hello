using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewParticleSpawner : MonoBehaviour {
	[SerializeField] private Renderer m_view;
	[SerializeField] private ParticleData[] m_particleDatas;


	private GameCamera m_camera;

	private Transform m_parent;
	private GameObject[] m_particleSytems;

	private bool m_spawned;

	// Use this for initialization
	void Awake () {
		m_camera = Camera.main.GetComponent<GameCamera>();

		m_parent = transform;
		m_particleSytems = new GameObject[m_particleDatas.Length];

		for (int i = 0; i < m_particleDatas.Length; ++i) {
			m_particleDatas[i].CreatePool();
		}

		m_spawned = false;
	}

	void Update() {
		// Show / Hide fire effect if thfis node is inside Camera or not
		bool isInsideActivationMaxArea = false;

		if(m_camera != null) {
			if (m_view != null) {
				isInsideActivationMaxArea = m_camera.IsInsideFrustrum(m_view.bounds);
			} else {
				isInsideActivationMaxArea = m_camera.IsInsideFrustrum(m_parent.position);
			}
		}

		if (isInsideActivationMaxArea) {
			if (!m_spawned) {
				Spawn();
			}
		} else {
			if (m_spawned) {
				Return();
			}
		}
	}
	
	void Spawn() {
		for (int i = 0; i < m_particleDatas.Length; ++i) {
			m_particleSytems[i] = m_particleDatas[i].Spawn(m_parent);
		}

		m_spawned = true;
	}

	void Return() {
		for (int i = 0; i < m_particleSytems.Length; ++i) {
			if (m_particleSytems[i] != null) {
				m_particleDatas[i].ReturnInstance(m_particleSytems[i]);
			}
			m_particleSytems[i] = null;
		}

		m_spawned = false;
	}
}

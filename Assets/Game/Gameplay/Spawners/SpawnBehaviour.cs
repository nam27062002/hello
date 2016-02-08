﻿using UnityEngine;
using System.Collections;

public class SpawnBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField] private bool m_deactivate = true;

	private AreaBounds m_area;
	private Spawner m_spawner;
	private int m_index;
	public int index { get { return m_index; } }

	private GameCameraController m_camera;
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {
		m_camera = GameObject.Find("PF_GameCamera").GetComponent<GameCameraController>();
	}

	void OnEnable() {
		EntityManager.instance.Register(GetComponent<Entity>());
	}

	void OnDisable() {
		EntityManager.instance.Unregister(GetComponent<Entity>());

		if (m_spawner) {
			m_spawner.RemoveEntity(gameObject, true);
			m_spawner = null;
		}
	}

	void LateUpdate() {
		if (m_deactivate && m_camera.IsInsideDeactivationArea(transform.position)) {
			if (m_spawner) {
				m_spawner.RemoveEntity(gameObject, false);
				m_spawner = null;
				gameObject.SetActive(false);
			}
		}
	}

	public void Spawn(Spawner _spawner, int _index, Vector3 _position, AreaBounds _bounds) {		
		m_spawner = _spawner;
		m_index = _index;
		m_area = _bounds;
		
		transform.position = _position;

		Initializable[] components = GetComponents<Initializable>();

		foreach (Initializable component in components) {
			component.SetAreaBounds(m_area);
			component.Initialize();
		}
	}
}
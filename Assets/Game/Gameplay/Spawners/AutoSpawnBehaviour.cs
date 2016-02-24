﻿using UnityEngine;
using System.Collections;

public class AutoSpawnBehaviour : MonoBehaviour, ISpawner {
	//-----------------------------------------------
	// Constants
	//-----------------------------------------------
	public enum State {
		Idle = 0,
		Respawning
	};

	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField] private float m_spawnTime;

	private State m_state;
	public State state
	{
		get
		{
			return m_state;
		}
	}

	private float m_timer;

	private Bounds m_bounds; // view bounds

	private GameCameraController m_camera;
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {
		SpawnerManager.instance.Register(this);

		m_state = State.Idle;
		m_camera = GameObject.Find("PF_GameCamera").GetComponent<GameCameraController>();

		GameObject viewBurned = transform.FindChild("view_burned").gameObject;
		Collider collider = GetComponent<Collider>();
		if (collider != null) {
			m_bounds = collider.bounds;
		} else {
			m_bounds = viewBurned.GetComponent<Renderer>().bounds;
		}
	}

	void Update() {
		if (m_state == State.Respawning) {
			if (m_timer > 0) {
				m_timer -= Time.deltaTime;
				if (m_timer < 0) {
					m_timer = 0;
				}
			} else {
				if (m_camera.IsInsideActivationArea(m_bounds)) {
					Spawn();
				}
			}
		}
	}

	public void Respawn() {
		m_timer = m_spawnTime;
		m_state = State.Respawning;
	}
		
	private void Spawn() {		
		Initializable[] components = GetComponents<Initializable>();
		foreach (Initializable component in components) {
			component.Initialize();
		}
		m_state = State.Idle;
	}
}
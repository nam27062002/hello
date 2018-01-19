using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewParticleSpawner : MonoBehaviour {
	[Comment("Where the particles will be spawned.\nLeave empty to use this transform as parent.")]
	[SerializeField] private Transform m_parent;
	[Comment("View's bounds will be compared to the camera frustum to trigger the particles.\nLeave empty to use parent's position instead.")]
	[SerializeField] private Renderer m_view;
	[Space]
	[SerializeField] private ParticleData[] m_particleDatas;

	private enum State {
		Idle = 0,
		Spawned,
		Return
	}

	private GameCamera m_camera;

	private GameObject[] m_particleSytems;
	private ParticleControl[] m_particleControl;

	private State m_state;


	// Use this for initialization
	void Awake () {
		m_camera = null;
		if(Camera.main != null) {
			m_camera = Camera.main.GetComponent<GameCamera>();
		}

		if(m_parent == null) {
			m_parent = transform;
		}

		m_particleSytems = new GameObject[m_particleDatas.Length];
		m_particleControl = new ParticleControl[m_particleDatas.Length];

		for (int i = 0; i < m_particleDatas.Length; ++i) {
			m_particleDatas[i].CreatePool();
		}

		m_state = State.Idle;
	}

	void OnDisable() {
		ForceReturn();
	}

	void Update() {
		// Show / Hide effect if this node is inside Camera or not
		bool isInsideActivationMaxArea = false;

		if (m_camera != null) {
			if (m_view != null) {
				isInsideActivationMaxArea = m_camera.IsInsideCameraFrustrum(m_view.bounds);
			} else {			
				isInsideActivationMaxArea = m_camera.IsInsideCameraFrustrum(m_parent.position);
			}
		} else {
			// [AOC] Probably not in-game, always show the effect (i.e. Menu)
			isInsideActivationMaxArea = true;
		}

		switch (m_state) {
			case State.Idle:
				if (isInsideActivationMaxArea) {
					Spawn();
				}
				break;

			case State.Spawned:
				if (!isInsideActivationMaxArea) {
					m_state = State.Return;
				}
				break;

			case State.Return:
				if (isInsideActivationMaxArea) {
					CancelReturn();
				} else {
					StopAndReturn();
				}
				break;
		}
	}
	
	protected virtual void Spawn() {
		for (int i = 0; i < m_particleDatas.Length; ++i) {
			m_particleSytems[i] = m_particleDatas[i].Spawn(m_parent, m_particleDatas[i].offset, true);
			if (m_particleSytems[i] != null) {
				m_particleControl[i] = m_particleSytems[i].GetComponent<ParticleControl>();
			}
		}

		m_state = State.Spawned;
	}

	protected virtual void CancelReturn() {
		for (int i = 0; i < m_particleControl.Length; ++i) {
			if (m_particleControl[i] != null) {
				m_particleControl[i].Play(m_particleDatas[i]);
			}
		}
		m_state = State.Spawned;
	}

	protected virtual void StopAndReturn() {
		bool areStopped = true;
		for (int i = 0; i < m_particleControl.Length; ++i) {
			if (m_particleControl[i] != null) {
				areStopped = areStopped && m_particleControl[i].Stop();
			}
		}

		if (areStopped) {
			ForceReturn();
		}
	}

	protected virtual void ForceReturn() {
		for (int i = 0; i < m_particleSytems.Length; ++i) {
			if (m_particleSytems[i] != null) {
				m_particleDatas[i].ReturnInstance(m_particleSytems[i]);
			}
			m_particleSytems[i] = null;
			m_particleControl[i] = null;
		}
		m_state = State.Idle;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProcController : MonoBehaviour {
	private Renderer m_renderer;
	private Material m_material;

	private enum State {
		Idle = 0,
		Play
	}

	private float m_powerSource;
	private float m_powerTarget;
	private float m_timer;
	private float m_duration;

	private State m_state;

	private void Awake() {
		m_renderer = transform.GetFirstComponentInChildren<Renderer>();
		m_material = m_renderer.material;
		m_material.SetFloat("_Power", 0f);
		m_renderer.material = m_material;

		m_powerSource = 0f;
		m_powerTarget = 0f;
		m_timer = 0f;
		m_duration = 1f;

		m_state = State.Idle;
	}

	public void SetPower(float _value) {
		m_material.SetFloat("_Power", _value);
		m_renderer.material = m_material;
	}
/*
	// Use this for initialization
	public void Burn(float _initPower) {
		m_material = m_renderer.material;

		m_powerSource = _initPower;
		m_powerTarget = 6f;
		m_timer = 0f;

		m_material.SetFloat("_Power", m_powerSource);
		m_renderer.material = m_material;

		m_state = State.Play;
	}

	// Use this for initialization
	public void Extinguish() {
		m_material = m_renderer.material;

		m_powerSource = m_material.GetFloat("_Power");
		m_powerTarget = 0f;
		m_timer = 0f;

		m_material.SetFloat("_Power", m_powerSource);
		m_renderer.material = m_material;

		m_state = State.Play;
	}

	// Update is called once per frame
	private void Update () {
		if (m_state == State.Play) {
			m_timer += Time.deltaTime;

			float power = m_powerSource + (m_timer * ((m_powerTarget - m_powerSource) / m_duration));
			m_material.SetFloat("_Power", power);
			m_renderer.material = m_material;

			if (m_timer >= 1f) {
				m_state = State.Idle;
			}
		}
	}*/
}

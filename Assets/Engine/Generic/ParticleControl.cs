using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl : MonoBehaviour {

	private ParticleScaler m_scaler;
	private List<ParticleSystem> m_subsystems = null;


	private void Awake() {
		if (m_subsystems == null) {
			FindSystems();
		}
	}

	private void FindSystems() {
		m_scaler = GetComponent<ParticleScaler>();
		m_subsystems = transform.FindComponentsRecursive<ParticleSystem>();
	}

	public void Play(ParticleData _data) {
		if (m_subsystems == null) {
			FindSystems();
		}

		// lets iterate
		for (int i = 0; i < m_subsystems.Count; i++) {
			ParticleSystem system = m_subsystems[i];

			system.Clear();

			ParticleSystem.MainModule main = system.main;

			if (_data != null) {
				if (_data.changeStartColor) {					
					ParticleSystem.MinMaxGradient gradient = main.startColor;
					gradient.color = _data.startColor;
					main.startColor = gradient;
				}

				if (_data.changeColorOvertime) {
					ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
					ParticleSystem.MinMaxGradient gradient = colorOverLifetime.color;
					gradient.gradient = _data.colorOvertime;
					colorOverLifetime.color = gradient;
				}
			}

			ParticleSystem.EmissionModule em = system.emission;
			em.enabled = true;

			if (main.prewarm) {
				system.Simulate(1f);
			}

			system.Play();
		}

		if (_data != null) {
			if (m_scaler != null) {
				if (m_scaler.m_scale != _data.scale) {
					m_scaler.m_scale = _data.scale;
					m_scaler.DoScale();
				}
			}
		}
	}

	// returns if it is fully stopped or not
	public bool Stop() {
		if (m_subsystems == null) {
			FindSystems();
		}

		bool isAlive = false;
		for (int i = 0; i < m_subsystems.Count; i++) {
			ParticleSystem system = m_subsystems[i];
			ParticleSystem.EmissionModule em = system.emission;

			if (em.enabled && system.main.loop) {
				em.enabled = false;
				system.Stop();
			}

			if (system.IsAlive()) {
				isAlive = true;
			}
		}

		return !isAlive;
	}
}

using UnityEngine;
using System.Collections.Generic;


public class ParticlePoolHandler {

	private Pool m_pool;
	private ParticleData m_data;
	private bool m_isValid;


	//-----------------------------------------------------------------------------------------------------------------
	public ParticlePoolHandler(Pool _pool, ParticleData _data) {
		m_pool = _pool;
		m_data = _data;
		m_isValid = true;
	}


	//-----------------------------------------------------------------------------------------------------------------
	public void Invalidate() { m_isValid = false; }
	public bool isValid { get { return m_isValid; } }

	//-----------------------------------------------------------------------------------------------------------------
	public GameObject Spawn(Vector3 _at = default(Vector3)) {
		if (m_isValid) {
			GameObject system = m_pool.Get(true);
			StartSystem(system, _at);
			return system;
		}
		Debug.LogError("[Pool of " + m_data.name + "] invalid pool handler");
		return null;
	}

	public void ReturnInstance(GameObject _go) {
		if (m_isValid) {
			m_pool.Return(_go);
		} else {
			Debug.LogError("[Pool of " + m_data.name + "] invalid pool handler");
		}
	}


	//-----------------------------------------------------------------------------------------------------------------
	private void StartSystem(GameObject _system, Vector3 _at) {
		// Skip if system is not valid
		if (_system != null) {

			// Reset system's position
			_system.transform.localPosition = Vector3.zero;
			_system.transform.position = _at;

			// Restart all particle systems within the instance
			List<ParticleSystem> subsystems = _system.transform.FindComponentsRecursive<ParticleSystem>();
			for (int i = 0; i < subsystems.Count; i++) {
				subsystems[i].Clear();

				ParticleSystem.MainModule main = subsystems[i].main;

				if (m_data.changeStartColor) {					
					ParticleSystem.MinMaxGradient gradient = main.startColor;
					gradient.color = m_data.startColor;
					main.startColor = gradient;
				}

				if (m_data.changeColorOvertime) {
					ParticleSystem.ColorOverLifetimeModule colorOverLifetime = subsystems[i].colorOverLifetime;
					ParticleSystem.MinMaxGradient gradient = colorOverLifetime.color;
					gradient.gradient = m_data.colorOvertime;
					colorOverLifetime.color = gradient;
				}

				ParticleSystem.EmissionModule em = subsystems[i].emission;
				em.enabled = true;

				if (main.prewarm) {
					subsystems[i].Simulate(1f);
				}

				subsystems[i].Play();
			}

			ParticleScaler scaler = _system.GetComponent<ParticleScaler>();
			if (scaler != null) {
				if (scaler.m_scale != m_data.scale) {
					scaler.m_scale = m_data.scale;
					scaler.DoScale();
				}
			}
		}
	}
}

using UnityEngine;
using System.Collections.Generic;


public class ParticleHandler {
	private Pool m_pool;
	private bool m_isValid;


	//-------------------------------------------------//
	public ParticleHandler() {
		Invalidate();
	}

	public ParticleHandler(Pool _pool) {
		AssignPool(_pool);
	}


	//-------------------------------------------------//
	public void AssignPool(Pool _pool)	{ 
		if (_pool != null) {
			m_isValid = true;  
			m_pool = _pool;
		} else {
			Invalidate();
		}
	}

	public void Invalidate() {
		m_isValid = false;
		m_pool = null;  
	}


	//-------------------------------------------------//
	public GameObject Spawn(ParticleData _data, Vector3 _at = default(Vector3)) {
		if (m_isValid) {
			GameObject system = m_pool.Get(true);
			StartSystem(system, null, _data, _at);
			return system;
		}
		Debug.LogError("[Particle] invalid pool handler for " + _data.name);
		return null;
	}

	public GameObject Spawn(ParticleData _data, Transform _parent, Vector3 _offset = default(Vector3)) {
		if (m_isValid) {
			GameObject system = m_pool.Get(true);
			StartSystem(system, _parent, _data, _offset);
			return system;
		}
		Debug.LogError("[Particle] invalid pool handler for " + _data.name);
		return null;
	}

	public void ReturnInstance(GameObject _go) {
		if (m_isValid) {
			m_pool.Return(_go);
		} else {
			Debug.LogError("[Particle] invalid pool handler for " + _go.name);
		}
	}


	//-------------------------------------------------//
	private void StartSystem(GameObject _system, Transform _parent, ParticleData _data, Vector3 _at) {
		// Skip if system is not valid
		if (_system != null) {
			// Setup system's transform
			Transform t = _system.transform;
			if (_parent != null) {
				t.SetParent(_parent, false);
			}
			t.localScale = Vector3.one;
			t.localRotation = Quaternion.identity;
			t.localPosition = Vector3.zero;
			t.position = Vector3.zero;
			t.localPosition = _at;

			// Restart all particle systems within the instance
			List<ParticleSystem> subsystems = _system.transform.FindComponentsRecursive<ParticleSystem>();
			for (int i = 0; i < subsystems.Count; i++) {
				subsystems[i].Clear();

				ParticleSystem.MainModule main = subsystems[i].main;

				if (_data != null) {
					if (_data.changeStartColor) {					
						ParticleSystem.MinMaxGradient gradient = main.startColor;
						gradient.color = _data.startColor;
						main.startColor = gradient;
					}

					if (_data.changeColorOvertime) {
						ParticleSystem.ColorOverLifetimeModule colorOverLifetime = subsystems[i].colorOverLifetime;
						ParticleSystem.MinMaxGradient gradient = colorOverLifetime.color;
						gradient.gradient = _data.colorOvertime;
						colorOverLifetime.color = gradient;
					}
				}

				ParticleSystem.EmissionModule em = subsystems[i].emission;
				em.enabled = true;

				if (main.prewarm) {
					subsystems[i].Simulate(1f);
				}

				subsystems[i].Play();
			}

			if (_data != null) {
				ParticleScaler scaler = _system.GetComponent<ParticleScaler>();
				if (scaler != null) {
					if (scaler.m_scale != _data.scale) {
						scaler.m_scale = _data.scale;
						scaler.DoScale();
					}
				}
			}
		}
	}
}

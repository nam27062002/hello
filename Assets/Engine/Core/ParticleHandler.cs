using UnityEngine;
using System.Collections.Generic;


public class ParticleHandler {
	private Pool m_pool;
	private bool m_isValid;
	public bool isValid { get { return m_isValid; } }

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
	public GameObject Spawn(ParticleData _data, Vector3 _at = default(Vector3), bool _prewarm = true) {		
		if (m_isValid) {
			GameObject system = m_pool.Get(false);
			StartSystem(system, null, _data, _at, _prewarm);
			return system;
		}
		if (_data != null){
			Debug.LogWarning("[Particle] invalid pool handler for " + _data.name);
		}else{
			Debug.LogWarning("[Particle] invalid pool handler");
		}
		return null;
	}

	public GameObject Spawn(ParticleData _data, Transform _parent, Vector3 _offset = default(Vector3), bool _prewarm = true) {
		if (m_isValid) {
			GameObject system = m_pool.Get(true);
			StartSystem(system, _parent, _data, _offset, _prewarm);
			return system;
		}
		if (_data != null){
			Debug.LogWarning("[Particle] invalid pool handler for " + _data.name);
		}else{
			Debug.LogWarning("[Particle] invalid pool handler");
		}
		return null;
	}

	public void ReturnInstance(GameObject _go) {
		if (m_isValid) {
			m_pool.Return(_go);
		} else {
			Debug.LogWarning("[Particle] invalid pool handler for " + _go.name);
		}
	}


	//-------------------------------------------------//
	private void StartSystem(GameObject _system, Transform _parent, ParticleData _data, Vector3 _at, bool _prewarm) {
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

			_system.SetActive(true);

			// Restart all particle systems within the instance
			ParticleControl pc = _system.GetComponent<ParticleControl>();
			if (pc != null) {
				pc.Play(_data, _prewarm);
			}


		}
	}
}

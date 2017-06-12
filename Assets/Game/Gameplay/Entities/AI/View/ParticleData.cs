using System;
using UnityEngine;

[Serializable]
public class ParticleData {
	public string name = "";
	public string path = "";
	public Vector3 offset = Vector3.zero;

	public bool changeStartColor = false;
	public Color startColor = Color.white;

	public bool changeColorOvertime = false;
	public Gradient colorOvertime;

	public float scale = 1f;

	//--------------//
	private ParticleHandler m_handler;
	//--------------//

	public ParticleData() {
		name = "";
		path = "";
		offset = Vector3.zero;

		// Color
		changeStartColor = false;
     	startColor = Color.white;

		changeColorOvertime = false;
		colorOvertime = new Gradient();

		// Particle scale
		scale = 1f;

		//
		m_handler = null;
	}

	public ParticleData(string n, string p, Vector3 o) {
		name = n;
		path = p;
		offset = o;

		// Color
		changeStartColor = false;
		startColor = Color.white;

		changeColorOvertime = false;
		colorOvertime = new Gradient();

		// Particle scale
		scale = 1f;

		//
		m_handler = null;
	}

	public bool IsValid() {
		return !string.IsNullOrEmpty(name);
	}

	public GameObject CreateInstance() {
		if (!string.IsNullOrEmpty(path) && !path.EndsWith("/")) path = path + "/";
		GameObject go = Resources.Load<GameObject>("Particles/" + path + name);
		GameObject instance = GameObject.Instantiate(go);
		return instance;
	}

	public void CreatePool() {
		if (IsValid()) {
			m_handler = ParticleManager.CreatePool(this);
		}
	}

	public GameObject Spawn(Vector3 _at = default(Vector3)) {
		if (m_handler != null) {
			return m_handler.Spawn(this, _at);
		}
		return null;
	}

	public GameObject Spawn(Transform _parent, Vector3 _offset = default(Vector3)) {
		if (m_handler != null) {
			return m_handler.Spawn(this, _parent, _offset);
		}
		return null;
	}

	public void ReturnInstance(GameObject _go) {
		if (m_handler != null) {
			m_handler.ReturnInstance(_go);
		}
	}
}
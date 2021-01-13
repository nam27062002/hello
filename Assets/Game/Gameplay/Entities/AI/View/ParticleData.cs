using System;
using UnityEngine;

[Serializable]
public class ParticleData {
	public string name = "";
	public string path = "";

	[Space]
	public bool changeStartColor = false;
	public Color startColor = Color.white;
	public Color startColorTwo = Color.white;

	public bool changeColorOvertime = false;
	public Gradient colorOvertime;

	[Space]
	public Vector3 offset = Vector3.zero;
	public float scale = 1f;
	public bool orientate = false;

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
		startColorTwo = Color.white;

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
		startColorTwo = Color.white;

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
        GameObject go = HDAddressablesManager.Instance.LoadAsset<GameObject>(name, "Master");
        GameObject instance = null;
        if (go != null) {
            instance = UnityEngine.Object.Instantiate(go);
        }
		return instance;
	}

	public void CreatePool() {
		if (IsValid()) {
			if (m_handler == null || !m_handler.isValid) {
				m_handler = ParticleManager.CreatePool(this);
			}
		}
	}

	public GameObject Spawn(Vector3 _at = default(Vector3), Quaternion _orientation = default(Quaternion)) {
		if (m_handler != null) {
			if (!m_handler.isValid) {
				CreatePool();
			}
			GameObject go = m_handler.Spawn(this, _at);
            if (orientate && go != null)
				go.transform.rotation = _orientation;
			return go;
		}
		return null;
	}

	public GameObject Spawn(Transform _parent, Vector3 _offset = default(Vector3), bool _prewarm = true, Quaternion _orientation = default(Quaternion)) {
		if (m_handler != null) {
			if (!m_handler.isValid) {
				CreatePool();
			}
			GameObject go = m_handler.Spawn(this, _parent, _offset, _prewarm);
            if (orientate && go != null)
				go.transform.rotation = _orientation;
			return go;
		}
		return null;
	}

	public void ReturnInstance(GameObject _go) {
		if (m_handler != null) {
			m_handler.ReturnInstance(_go);
		}
	}
}
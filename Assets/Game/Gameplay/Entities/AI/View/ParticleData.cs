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
	}

	public bool IsValid() {
		return !string.IsNullOrEmpty(name);
	}

	public GameObject CreateInstance() {
		GameObject go = Resources.Load<GameObject>("Particles/" + path + name);
		GameObject instance = GameObject.Instantiate(go);
		return instance;
	}
}
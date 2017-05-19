using System;
using UnityEngine;

[Serializable]
public class ParticleData {
	public string name = "";
	public string path = "";
	public Vector3 offset = Vector3.zero;
	public bool changeColor = false;
	public Color startColor = Color.white;

	public ParticleData() {
		name = "";
		path = "";
		offset = Vector3.zero;
		changeColor = false;
     	startColor = Color.white;
	}

	public ParticleData(string n, string p, Vector3 o) {
		name = n;
		path = p;
		offset = o;
		changeColor = false;
		startColor = Color.white;
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
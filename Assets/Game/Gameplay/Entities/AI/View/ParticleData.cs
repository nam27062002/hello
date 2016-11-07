using System;
using UnityEngine;

[Serializable]
public class ParticleData {
	public string name = "";
	public string path = "";
	public Vector3 offset = Vector3.zero;

	public ParticleData( string n, string p, Vector3 o)
	{
		name = n;
		path = p;
		offset = o;
	}
}
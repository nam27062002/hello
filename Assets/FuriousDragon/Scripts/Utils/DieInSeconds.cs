using UnityEngine;
using System.Collections;

public class DieInSeconds : MonoBehaviour {

	public float lifetime = 1f;


	void Update () {

		lifetime -= Time.deltaTime;
		if (lifetime < 0f)
			DestroyObject (this.gameObject);
	}
}

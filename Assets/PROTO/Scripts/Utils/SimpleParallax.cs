using UnityEngine;
using System.Collections;

public class SimpleParallax : MonoBehaviour {

	public float parallaxSpeed = 1.2f;
	Vector3 oldCamPos;
	public bool randomize = true;

	// Use this for initialization
	void Start () {

		oldCamPos = Camera.main.transform.position;
		if (randomize)
			parallaxSpeed = Random.Range (-0.5f,-2.5f); 
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 cpos = Camera.main.transform.position;
		Vector3 delta = oldCamPos - cpos;
		oldCamPos = cpos;

		transform.position = Vector3.Lerp (transform.position, transform.position+delta*parallaxSpeed,0.4f);

	}
}

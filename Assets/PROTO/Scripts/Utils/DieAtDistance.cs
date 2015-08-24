using UnityEngine;
using System.Collections;

public class DieAtDistance : MonoBehaviour {

	public float distance = 4000f;


	Transform player;
	float sqrDistance;
	 
	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Player").transform;
		sqrDistance = distance*distance;
	}
	
	// Update is called once per frame
	void Update () {

		if ((transform.position-player.position).sqrMagnitude > sqrDistance)
			DestroyObject(gameObject);
	}
}

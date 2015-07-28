using UnityEngine;
using System.Collections;

public class ExplosionExpansion : MonoBehaviour {

	public float finalRadius = 400f;
	public float expansionSpeed = 1000f;
	public Vector3 center = Vector3.zero;

	float timer = 0f;
	float radius = 1f;

	// Update is called once per frame
	void Update () {
	

		float d = radius*radius;
		Messenger.Broadcast<Vector3,float>("OnExplosion",center,d);

		radius = Mathf.MoveTowards(radius,finalRadius,expansionSpeed*Time.deltaTime);

		if (radius >= finalRadius){

			d = radius*radius;
			Messenger.Broadcast<Vector3,float>("OnExplosion",center,d);

			DestroyObject (this.gameObject);
		}
	}
}

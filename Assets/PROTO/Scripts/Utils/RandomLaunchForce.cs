using UnityEngine;
using System.Collections;

public class RandomLaunchForce : MonoBehaviour {

	void Awake () {
	
			Vector3 force = new Vector3(Random.Range(-0.4f,0.4f),1f,0f);
			force *= Random.Range (25f,50f)*800f;
			GetComponent<Rigidbody>().AddForce(force);
			
			GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-1f,1f)*100000f,Random.Range(-1f,1f)*100000f,Random.Range(-1f,1f)*100000f));

	}

}

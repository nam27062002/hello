using UnityEngine;
using System.Collections;

public class EdibleChunkBehaviour : MonoBehaviour {


	Vector3 force;
	Vector3 torque;
	float timer;
	float life;

	// Use this for initialization
	void Start () {
	
		// set initial forces
		force = new Vector3(Random.Range (-200f, 200f),
		                    Random.Range (100f,200f),
		                    0f);

		torque = new Vector3(Random.Range (-360f,360f),Random.Range (-360f,360f),Random.Range (-360f,360f));

		timer = 0f;
		life  = 0.75f;
	}
	
	// Update is called once per frame
	void Update () {
	
		transform.Rotate ( torque*Time.deltaTime);

		force += Vector3.down*Time.deltaTime*700f;

		transform.position = transform.position + force*Time.deltaTime;

		timer += Time.deltaTime;
		float delta = timer/life;
		if (delta < 1f){
			GetComponent<Renderer>().material.color = new Color(1f,1f,1f,1f-delta);
		}else{
			DestroyObject(this.gameObject);
		}
	}
}

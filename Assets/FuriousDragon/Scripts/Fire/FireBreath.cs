using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FireBreath : DragonFireInterface {


	public float fireRate = 10f; // Fire particles per second
	public float firePower = 1f;
	float timer;

    const int maxFireParticles = 256;
	GameObject[] fire = new GameObject[maxFireParticles];

	public float collisionDepth = 1000f;

	Transform mouthPosition;
	Transform headPosition;

	void Start () {

		Transform instances = GameObject.Find ("Instances").transform;

		Object firePrefab;
		firePrefab = Resources.Load("Proto/Flame");


		for(int i=0;i<maxFireParticles;i++){
			GameObject fireObj = (GameObject)Object.Instantiate(firePrefab);
			fireObj.transform.parent = instances;
			fireObj.transform.localPosition = Vector3.zero;
			fireObj.SetActive(false);
			fire[i] = fireObj;
		}

		timer = 0f;

		mouthPosition = transform.FindSubObjectTransform("eat");
		headPosition = transform.FindSubObjectTransform("head");

	}
	
	void Update () {
		timer -= Time.deltaTime;
	}

	override public void Fire(Vector3 direction){


		Vector3 dir = mouthPosition.position - headPosition.position;
		dir.z = 0f;
		dir.Normalize();
		dir *= direction.magnitude;
	
		for(int i=0;i<2;i++){
			foreach(GameObject fireObj in fire){
				if (!fireObj.activeInHierarchy){
					fireObj.GetComponent<FlameParticle>().Activate(mouthPosition.position+dir.normalized*(i*1100f*Time.deltaTime),dir, collisionDepth, firePower);
					// We inlcude some of the dragon momentum in the particles initial speed
					// also, for eahc frame we fire two particles so we need to space them properly
					timer = 1f/fireRate;
					break;
				}
			}
		}
	}
}

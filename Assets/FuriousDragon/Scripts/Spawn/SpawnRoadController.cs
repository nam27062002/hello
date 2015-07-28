using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnRoadController : MonoBehaviour {


	public float[] spawnRate = new float[2]; // Time between cars, range 
	public float[] vechicleSpeed = new float[2]; // Time between cars, range 

	public Object[] vehiclePrefabs;

	public int maxVehicles = 5;


	List<GameObject> vehicleInstances = new List<GameObject>();

	Transform roadStart;
	Transform roadEnd;

	float timer;
	int nextSpawn;

	// Use this for initialization
	void Start () {

		roadStart = transform.FindChild("RoadStart");
		roadEnd   = transform.FindChild("RoadEnd");

		Transform instances = GameObject.Find ("Instances").transform;

		for(int i=0;i<maxVehicles;i++){
			int vehicleIdx = Random.Range (0,vehiclePrefabs.Length);
			GameObject prefab = (GameObject)Object.Instantiate(vehiclePrefabs[vehicleIdx]);
			prefab.transform.parent = instances;
			prefab.SetActive(false);
			vehicleInstances.Add (prefab);
		}
	
		timer = spawnRate[0];
		nextSpawn = 0;
	}
	
	// Update is called once per frame
	void Update () {

		timer -= Time.deltaTime;

		if (timer <= 0f){

			timer = Random.Range (spawnRate[0], spawnRate[1]);

			GameObject obj = vehicleInstances[nextSpawn];
			RoadVehicleBehaviour vehicle = obj.GetComponent<RoadVehicleBehaviour>();
			vehicle.Run(roadStart.position, roadEnd.position);
			vehicle.speed = Random.Range(vechicleSpeed[0],vechicleSpeed[1]);
			vehicle.gameObject.SetActive(true);

			nextSpawn = (nextSpawn+1)%maxVehicles;
			timer = Random.Range (spawnRate[0], spawnRate[1]);
		}
	
	}
}

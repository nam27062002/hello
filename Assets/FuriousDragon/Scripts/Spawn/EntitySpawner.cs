using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour {

	public Object entityPrefab;
	public int numEntities = 4;
	public float range = 1500f;


	GameObject[] entities;
	GameObject player;
	bool 	playerInRange = false;
	float 	sqrRange;
	Bounds  spawnBounds;

	// Use this for initialization
	void Start () {

		entities = new GameObject[numEntities];
		for(int i=0;i<numEntities;i++){

			GameObject obj = (GameObject)Object.Instantiate(entityPrefab);
			obj.transform.parent = transform;
			obj.SetActive(false);
			entities[i] = obj;
		}

		player = GameObject.Find ("Player");
		sqrRange = range*range;

		BoxCollider2D box = GetComponent<BoxCollider2D>();
		if (box != null){
			spawnBounds = box.bounds;
			box.enabled = false;
		}else{
			spawnBounds = new Bounds(transform.position,Vector3.one*100f);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		float d = (transform.position - player.transform.position).sqrMagnitude;

		if (d < sqrRange && !playerInRange){

			playerInRange = true;

			foreach(GameObject obj in entities){

				if (obj.GetComponent<SpawnableBehaviour>().state != SpawnableBehaviour.State.INVALID){
					obj.GetComponent<SpawnableBehaviour>().Spawn(spawnBounds);
					obj.SetActive (true);
				}
			}
		}else if (d > sqrRange && playerInRange){

			playerInRange = false;

			foreach(GameObject obj in entities){
				
				if (obj.GetComponent<SpawnableBehaviour>().state != SpawnableBehaviour.State.INVALID)
					obj.SetActive (false);
			}
		}

	}
}

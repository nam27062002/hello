using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkySpawner : MonoBehaviour {


	public Object[] spawnables;
	public float    spawnDistance;

	DragonMotion player;
	

	class SpawnedGroup{

		public Vector3 	position;
		public GameObject 	group;
	}

	List<SpawnedGroup> spawnedList = new List<SpawnedGroup>();
	int groundMask;
	float timer = 0.0f;

	// Use this for initialization
	void Start () {
	
		player = GameObject.Find ("Player").GetComponent<DragonMotion>();
		groundMask = 1 << LayerMask.NameToLayer("Ground");

		// [AOC] OLD MESSENGER! Messenger.AddListener<GameObject>("SpawnOutOfRange",OnSpawnOutOfRange);
	}

	void OnDestroy(){
		// [AOC] OLD MESSENGER! Messenger.RemoveListener<GameObject>("SpawnOutOfRange",OnSpawnOutOfRange);
	}
	
	// Update is called once per frame
	void Update () {
	
		timer -= Time.deltaTime;
		if (timer < 0f){
		
			Vector3 dir = player.direction;

			Vector3 pos = player.transform.position+dir*spawnDistance;
			if (!GroupExists(pos)){
				CreateGroup (pos);
				timer = 1f;
			}else{
				timer = 0.25f;
			}
		}
	}

	bool GroupExists(Vector3 pos) {

		// first test if the position is in the sky
		float minHeight = pos.y-400f;
		Vector3 testPoint = pos;
		testPoint.y = 10000f;
		RaycastHit hit;

		if (Physics.Linecast(testPoint, testPoint+Vector3.down*20000f,out hit,groundMask)) {
			// if collision point is to close or above ground
			if (hit.point.y > minHeight)
				return true;
		}

		float spawnDistanceSqr = spawnDistance * spawnDistance;
		for(int i = 0; i < spawnedList.Count; i++) {
			float distSqr = (spawnedList[i].position - pos).sqrMagnitude;
			if (distSqr < spawnDistanceSqr)
				return true;
		}

		return false;
	}

	void CreateGroup(Vector3 position){

		SpawnedGroup sgroup = new SpawnedGroup();
		sgroup.position = position;

		GameObject spawnable = (GameObject)Object.Instantiate(spawnables[Random.Range (0,spawnables.Length)]);
		spawnable.transform.position = position;
		spawnable.transform.parent = this.transform;

		sgroup.group = spawnable;
		spawnedList.Add(sgroup);
	}

	public void OnSpawnOutOfRange(GameObject obj) {

		for(int i = 0; i < spawnedList.Count; i++){
			if (spawnedList[i].group == obj) {
				spawnedList.Remove(spawnedList[i]);
				DestroyObject(obj);
				break;
			}
		}
	}
}

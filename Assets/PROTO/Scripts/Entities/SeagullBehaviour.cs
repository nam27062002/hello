using UnityEngine;
using System.Collections;

public class SeagullBehaviour : MonoBehaviour {


	Vector3 originScale;
	float timer;
	EdibleBehaviour_OLD edible;
	SpawnableBehaviour spawn;
	FlockBehaviour flock;
	bool initialized = false;

	void Initialize(){

		initialized = true;
		spawn = GetComponent<SpawnableBehaviour>();
		edible = GetComponent<EdibleBehaviour_OLD>();
		flock = GetComponent<FlockBehaviour>();
		originScale = transform.localScale;

		// Randomize seagull wing flap
		Animation anim = transform.FindChild ("view").GetComponent<Animation>();
		foreach (AnimationState state in anim) {
			state.speed = Random.Range (0.8f,1.2f);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (!initialized) 
			Initialize();

		if (spawn.state == SpawnableBehaviour.State.SPAWN)
			Spawn ();


		if (edible.state != EdibleBehaviour_OLD.State.NONE){
			flock.enabled = false;
		}

	}

	public void Spawn(){

		spawn.state = SpawnableBehaviour.State.NONE;

		transform.localScale = originScale;

		flock.enabled = true;
		flock.OnSpawn(spawn.bounds);
		edible.OnSpawn();

		GetComponent<GameEntity>().RestoreHealth();
	}
}

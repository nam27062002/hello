﻿using UnityEngine;
using System.Collections;

public class BatBehaviour : MonoBehaviour {

	
	Vector3 originScale;
	float timer;
	EdibleBehaviour edible;
	SpawnableBehaviour spawn;
	FlockBehaviour flock;
	Animator anim;
	bool initialized = false;
	
	void Initialize(){
		
		initialized = true;
		spawn = GetComponent<SpawnableBehaviour>();
		edible = GetComponent<EdibleBehaviour>();
		flock = GetComponent<FlockBehaviour>();
		anim = transform.FindChild("view").GetComponent<Animator>();
		originScale = transform.localScale;

	}
	
	// Update is called once per frame
	void Update () {
		
		if (!initialized) 
			Initialize();
		
		if (spawn.state == SpawnableBehaviour.State.SPAWN)
			Spawn ();
		
		
		if (edible.state != EdibleBehaviour.State.NONE){
			flock.enabled = false;
		}
		
	}
	
	public void Spawn(){
		
		spawn.state = SpawnableBehaviour.State.NONE;
		
		transform.localScale = originScale;
		
		flock.enabled = true;
		flock.OnSpawn(spawn.bounds);
		edible.OnSpawn();

		anim.speed = Random.Range (1.5f,2.2f);

		GetComponent<GameEntity>().RestoreHealth();
	}
}

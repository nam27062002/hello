using UnityEngine;
using System.Collections.Generic;

public class MachineSpawner : MonoBehaviour {

	public GameObject machine;
	public int spawns;

	List<AI.IMachine> flock;

	// Use this for initialization
	void Start () {
	
		for (int i = 0; i < spawns; i++) {
			GameObject go = GameObject.Instantiate(machine);
			//flock.Add(go);
		}

		for (int i = 9; i < flock.Count; i++) {
			//flock[i].SetFlock(flock);
		}
	}
}

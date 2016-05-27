using UnityEngine;
using System.Collections;

public class MachineSpawner : MonoBehaviour {

	public GameObject machine;
	public int spawns;

	// Use this for initialization
	void Start () {
	
		for (int i = 0; i < spawns; i++) {
			GameObject go = GameObject.Instantiate(machine);
		}

	}
}

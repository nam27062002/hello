using UnityEngine;

public class MachineSpawner : MonoBehaviour {

	public GameObject machine;
	public int spawns;

	AI.Group flock;

	// Use this for initialization
	void Start () {
		flock = new AI.Group();
	
		for (int i = 0; i < spawns; i++) {
			GameObject go = GameObject.Instantiate(machine);
			go.transform.position = Random.insideUnitSphere;
			AI.MachineOld m = go.GetComponent<AI.MachineOld>();
			m.EnterGroup(ref flock);
		}
	}
}

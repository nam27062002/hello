using UnityEngine;
using System.Collections;

public class SpawnableBehaviour : MonoBehaviour {

	public enum State{
		NONE,
		SPAWN,
		INVALID
	};


	[HideInInspector] public State  state = State.NONE;
	[HideInInspector] public Bounds bounds;
	[HideInInspector] public Vector3 position;
	[HideInInspector] public Vector3 scale;
	[HideInInspector] public Quaternion rotation;

	void Start(){
	}

	public void Spawn(Bounds spawnBounds)
	{
		state = State.SPAWN;
		bounds = spawnBounds;
	}

	public void InRange(Vector3 position, Vector3 scale, Quaternion rotation){

		this.position = position;
		this.rotation = rotation;
		this.scale = scale;

		state = State.SPAWN;
	}

	public void OutOfRange(){
	
		state = State.NONE;
	}
}

using UnityEngine;
using System.Collections;

public class DragonControlAI : DragonControl {

	DragonPlayer player;
	DragonPlayer dragon;

	// Use this for initialization
	void Start () {

		dragon = GetComponent<DragonPlayer>();
		player = GameObject.Find ("Player").GetComponent<DragonPlayer>();
	}
	
	// Update is called once per frame
	void Update () {
		

	}
	
	override public Vector3 GetImpulse(float desiredVelocity){
		

			return Vector3.zero;

	} 
}

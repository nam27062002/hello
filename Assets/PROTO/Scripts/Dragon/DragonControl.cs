using UnityEngine;
using System.Collections;

public class DragonControl : MonoBehaviour {
	

	[HideInInspector] public bool moving = false;
	[HideInInspector] public bool action = false;
	
	virtual public Vector3 GetImpulse(float desiredVelocity){
		return Vector3.zero;
	} 

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSmashingBall : MonoBehaviour {

	void OnCollisionEnter(Collision _collision) {
		AI.MachineOld machine = _collision.collider.GetComponentInChildren<AI.MachineOld>();
		if ( machine != null && machine.CanBeSmashed() )
		{
			// check it's not a pet
			machine.Smashed();
		}
	}
}

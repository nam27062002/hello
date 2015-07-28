using UnityEngine;
using System.Collections;

public class FlamableHeli : FlamableBehaviour {

	
	override protected void BurnImpl(Vector3 pos, float power){

		entity.health -= power;
	}
}

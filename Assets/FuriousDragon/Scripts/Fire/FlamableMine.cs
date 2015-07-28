using UnityEngine;
using System.Collections;

public class FlamableMine : FlamableBehaviour {

	override protected void BurnImpl(Vector3 pos, float power){

		if (entity.health > 0f){
			entity.health -= power;	// Die instantly when burned

			if (entity.health <= 0f)
				GetComponent<FlyingMineBehaviour>().OnBurn();
		}
	}
}

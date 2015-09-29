using UnityEngine;
using System.Collections;

public class FlamableMine : FlamableBehaviour {

	override protected void BurnImpl(Vector3 pos, float power){

		if (hasBurned)
			GetComponent<OLD_FlyingMineBehaviour>().OnBurn();
	}
}

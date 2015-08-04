﻿using UnityEngine;
using System.Collections;

public class FlamableDragon : FlamableBehaviour {

	override protected void BurnImpl(Vector3 pos, float power){
		
		entity.health -= power;

		GetComponent<DragonControlAI>().OnBurn(pos);
	}
}

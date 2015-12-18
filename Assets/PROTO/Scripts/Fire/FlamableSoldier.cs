using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlamableSoldier : FlamableBehaviour {
	
	override protected void BurnImpl(Vector3 pos, float power){

		if (hasBurned){

		/*	SoldierBehaviour soldier = GetComponent<SoldierBehaviour>();
			if (soldier != null) {
				//soldier.OnBurn();
			} else {
				PersonBehaviour person = GetComponent<PersonBehaviour>();
				if (person != null) {
					person.OnBurn();
				} else {
					CagePersonBehaviour cage = GetComponent<CagePersonBehaviour>();
					if (cage != null) {
						cage.OnBurn();
					}
				}
			}*/
		}
	}


	override protected void ExplodeImpl(Vector3 pos, float power){

		entity.health -= power;
		if (entity.health < 0){
		/*	SoldierBehaviour soldier = GetComponent<SoldierBehaviour>();
			if (soldier != null) {
				//soldier.OnExplode(pos);
			} else {
				PersonBehaviour person = GetComponent<PersonBehaviour>();
				if (person != null) {
					person.OnBurn();
				} else {
					CagePersonBehaviour cage = GetComponent<CagePersonBehaviour>();
					if (cage != null) {
						cage.OnBurn();
					}
				}
			}*/
		}
	}
}

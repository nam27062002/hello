using UnityEngine;
using System.Collections;

public class FlamableAnimal : FlamableBehaviour {

	public Object burnedPrefab;
	public Object burnedParticle;
	public bool destroyOnBurn = false;
	public bool useMeat = false;

	override protected void BurnImpl(Vector3 pos, float power){

		if (hasBurned){

			if (useMeat && burnedPrefab != null){
				GameObject burnedObj = (GameObject)Object.Instantiate(burnedPrefab);
				burnedObj.transform.position = transform.position;
			} else if (!useMeat) {

				if (burnedParticle != null){
					GameObject burnedObj = (GameObject)Object.Instantiate(burnedParticle);
					burnedObj.transform.localPosition = Vector3.zero;
					burnedObj.transform.position = transform.position;
					burnedObj.transform.localScale = Vector3.one;
				}

				// Broadcast message
				Messenger.Broadcast<GameEntity>(GameEvents_OLD.ENTITY_EATEN, GetComponent<GameEntity>());			
			}

			if (destroyOnBurn) {
				DestroyObject(this.gameObject);
			} else {
				this.gameObject.SetActive (false);
			}

		}
	}

	override protected void ExplodeImpl(Vector3 pos, float power){
		
		entity.health -= power;
		if (entity.health < 0){
			AnimalBehaviour animal = GetComponent<AnimalBehaviour>();
			if(animal != null) animal.OnExplode(pos);
		}
	}
}

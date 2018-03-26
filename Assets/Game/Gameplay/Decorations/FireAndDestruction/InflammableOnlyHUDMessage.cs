using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflammableOnlyHUDMessage : MonoBehaviour {

	void OnCollisionEnter(Collision collision) {
		if (collision.transform.CompareTag("Player")) {
			if (InstanceManager.player.dragonBoostBehaviour.IsBoostActive()) {
				// Message : You need boost!
				Messenger.Broadcast(MessengerEvents.BREAK_OBJECT_WITH_FIRE);
				Debug.Log("ONLYWITHFIRE");
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventRewardScreen : MonoBehaviour {

	public void OnRewardButton() {
		GlobalEventManager.currentEvent.CollectReward();
		GlobalEventManager.RequestCurrentEventData();
		InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.DRAGON_SELECTION);
	}
}

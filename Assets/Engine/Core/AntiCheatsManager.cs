using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.Detectors;

public class AntiCheatsManager : Singleton<AntiCheatsManager> {


	public AntiCheatsManager()
	{
		ObscuredCheatingDetector.StartDetection(OnMemoryHackAttempt);
	}

	// Called upon memory hack attempt
	private void OnMemoryHackAttempt()
	{
		Debug.Log("AntiCheatManager :: Memory hack attempt detected");
		MarkUserAsCheater();
	}

	static public void MarkUserAsCheater()
	{
		if ( UsersManager.currentUser != null && !UsersManager.currentUser.isHacker )
		{
			UsersManager.currentUser.isHacker = true;
			HDTrackingManager.Instance.Notify_Hacker();
		}
	}
}

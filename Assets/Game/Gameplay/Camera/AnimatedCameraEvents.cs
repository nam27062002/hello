using UnityEngine;
using System.Collections;

public class AnimatedCameraEvents : MonoBehaviour 
{
	public void IntroDone()
	{
		Messenger.Broadcast(GameEvents.CAMERA_INTRO_DONE);
	}
}

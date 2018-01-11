using UnityEngine;
using System.Collections;

public class AnimatedCameraEvents : MonoBehaviour 
{
	public void IntroDone()
	{
		Messenger.Broadcast(MessengerEvents.CAMERA_INTRO_DONE);
	}
}

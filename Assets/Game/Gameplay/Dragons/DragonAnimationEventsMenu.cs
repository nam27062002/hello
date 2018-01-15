using UnityEngine;

public class DragonAnimationEventsMenu : MonoBehaviour {

	public void WingsSound(){}	// To be deleted

	public void WingsIdleSound(){}

	public void WingsFlyingSound(){}

	public void StrongFlap()
	{
	}

	public void EatStartEvent()
	{
	}

	public void EatStartBigEvent()
	{
	}

	public void EatEvent()
	{

	}

	public void CameraShake()
	{
		Messenger.Broadcast<float, float>(MessengerEvents.CAMERA_SHAKE, 0.5f, 0.25f);
	}

	public void StartFire()
	{
		transform.parent.GetComponent<MenuDragonPreview>().StartFlame();	
	}

	public void EndFire()
	{
		transform.parent.GetComponent<MenuDragonPreview>().EndFlame();
	}


}

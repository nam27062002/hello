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

	public void StartBlood()
	{
		transform.parent.GetComponent<MenuDragonPreview>().StartBlood();
	}

	public void EndBlood()
	{
		transform.parent.GetComponent<MenuDragonPreview>().EndBlood();
	}
    
    // This function is called by Results_intro of the helicopter
    public void TurnOffPropulsors()
    {
        DeactivateFlame("Leg_r");
        DeactivateFlame("Leg_l");
        DeactivateFlame("Arm_r");
        DeactivateFlame("Arm_l");
    }
    
    protected void DeactivateFlame( string childName )
    {
        Transform tr = transform.FindTransformRecursive(childName);
        if ( tr != null )
        {
            ParticleSystem ps = tr.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
                ps.Stop();
        }
        
    }

}

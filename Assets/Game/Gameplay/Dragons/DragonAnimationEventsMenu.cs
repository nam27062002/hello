using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonAnimationEventsMenu : MonoBehaviour {

    ParticleSystem m_particleInstance;
    public ParticleData m_particleData;

    public BroadcastEventType m_toggleEventType = BroadcastEventType.BOOST_TOGGLED;
    protected ToggleParam m_toggleParam;

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
    
    public void PlayExtraParticle(int index)
    {
        transform.parent.GetComponent<MenuDragonPreview>().PlayExtraParticle( index );
    }

    public void StopExtraParticle( int index )
    {
        transform.parent.GetComponent<MenuDragonPreview>().StopExtraParticle( index );
    }
    
    public void GroundHit()
    {
        if (m_particleInstance == null)
        {
            GameObject go = m_particleData.CreateInstance();
            m_particleInstance = go.GetComponent<ParticleSystem>();
            SceneManager.MoveGameObjectToScene(m_particleInstance.gameObject, gameObject.scene);
            m_particleInstance.transform.parent = transform;
            m_particleInstance.transform.localPosition = Vector3.zero;
            m_particleInstance.transform.rotation = Quaternion.LookRotation(Vector3.up);
        }
        m_particleInstance.Play();
    }


    public void ToggleEvent( int value )
    {
        if (m_toggleParam == null)
            m_toggleParam = new ToggleParam();

        if ( value > 0 )
        {
            m_toggleParam.value = true;
            Broadcaster.Broadcast(BroadcastEventType.BOOST_TOGGLED, m_toggleParam);
        }
        else
        {   
            m_toggleParam.value = false;
            Broadcaster.Broadcast(BroadcastEventType.BOOST_TOGGLED, m_toggleParam);
        }
        
    }

}

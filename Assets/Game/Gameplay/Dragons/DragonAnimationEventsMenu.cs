using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonAnimationEventsMenu : MonoBehaviour {

    ParticleSystem m_particleInstance;
    public ParticleData m_particleData;
    public Transform m_particleAnchor;

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
        ToggleParticle("Leg_r", false);
        ToggleParticle("Leg_l", false);
        ToggleParticle("Arm_r", false);
        ToggleParticle("Arm_l", false);
    }

    public void TurnOnPropulsors()
    {
        ToggleParticle("Leg_r", true);
        ToggleParticle("Leg_l", true);
        ToggleParticle("Arm_r", true);
        ToggleParticle("Arm_l", true);
    }

    
    protected void ToggleParticle( string childName, bool active )
    {
        Transform tr = transform.FindTransformRecursive(childName);
        if ( tr != null )
        {
            ParticleSystem ps = tr.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                if ( active ) ps.Play();
                else ps.Stop();
            }
                
        }
        
    }

    public void DeactivateParticle(string particleName )
    {
        ToggleParticle(particleName, false);
    }

    public void ActivateParticle( string particleName)
    {
        ToggleParticle(particleName, true);
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
        PrepareParticle();
        m_particleInstance.transform.rotation = Quaternion.LookRotation(Vector3.up);
        m_particleInstance.gameObject.SetActive(true);
        m_particleInstance.Play();
    }

    public void SpawnParticle()
    {
        PrepareParticle();
        m_particleInstance.gameObject.SetActive(true);
        m_particleInstance.Play();
    }

    void PrepareParticle()
    {
        if (m_particleInstance == null)
        {
            GameObject go = m_particleData.CreateInstance();
            m_particleInstance = go.GetComponent<ParticleSystem>();
            SceneManager.MoveGameObjectToScene(m_particleInstance.gameObject, gameObject.scene);
            if (m_particleAnchor != null)
            {
                m_particleInstance.transform.parent = m_particleAnchor;
            }
            else
            {
                m_particleInstance.transform.parent = transform;
            }
            m_particleInstance.transform.localPosition = GameConstants.Vector3.zero;
            m_particleInstance.transform.localRotation = GameConstants.Quaternion.identity;
        }
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

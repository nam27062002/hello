using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class DragonDinoAnimationEvents : DragonAnimationEvents {

    [Header("Jetpack Sounds")]
    public string m_jetpack1Sound;
    public string m_jetpack2Sound;

    [Header("Step")]
    public string m_stepSound;
    public ParticleData m_stepParticle;
    ParticleSystem m_stepParticleInstance;

    [Header("Stomp")]
    public string m_groundStompSound;
    public ParticleData m_groundStompParticle;
    public string m_groundStompLevel2Sound;
    
    ParticleSystem m_stompParticleInstance;
    string m_realGroundStompSound;

    [Header("Headbutt")]
    public string m_headbuttSound;
    public ParticleData m_headbutt;
    ParticleSystem m_headbuttParticleInstance;

    DragonMotionDino m_dragonMotion;

    protected ParticleSystem m_jetpackParticle;

    protected override void Start()
    {
        base.Start();
        if ( m_stepParticle.IsValid() )
        {
            GameObject go = m_stepParticle.CreateInstance();
            m_stepParticleInstance = go.GetComponent<ParticleSystem>();
            m_stepParticleInstance.gameObject.SetActive(false);
            SceneManager.MoveGameObjectToScene(m_stepParticleInstance.gameObject, gameObject.scene);
        }
        m_dragonMotion = InstanceManager.player.dragonMotion as DragonMotionDino;
        
        if ( m_dragonMotion.powerLevel >= 1 )
        {
            m_wingsWindSound = m_jetpack2Sound;
        }
        else
        {
            m_wingsWindSound = m_jetpack1Sound;
        }
        
        if (m_dragonMotion.powerLevel >= 2)
        {
            m_realGroundStompSound = m_groundStompLevel2Sound;
        }
        else
        {
            m_realGroundStompSound = m_groundStompSound;
        }
        
        if ( m_groundStompParticle.IsValid() )
        {
            GameObject go = m_groundStompParticle.CreateInstance();
            m_stompParticleInstance = go.GetComponent<ParticleSystem>();
            m_stompParticleInstance.gameObject.SetActive(false);
            SceneManager.MoveGameObjectToScene(m_stompParticleInstance.gameObject, gameObject.scene);
        }
        
        if ( m_headbutt.IsValid() )
        {
            GameObject go = m_headbutt.CreateInstance();
            m_headbuttParticleInstance = go.GetComponent<ParticleSystem>();
            m_headbuttParticleInstance.gameObject.SetActive(false);
            SceneManager.MoveGameObjectToScene(m_headbuttParticleInstance.gameObject, gameObject.scene);
        }

        // Search Jetpack particle
        m_jetpackParticle = transform.FindTransformRecursive("jetpack_1").GetComponentInChildren<ParticleSystem>();
    }

    protected void Step()
    {
        PlaySound(m_stepSound);
        if (m_stepParticleInstance)
        {
            m_stepParticleInstance.transform.position = m_dragonMotion.lastGroundHit;
            m_stompParticleInstance.transform.rotation = Quaternion.LookRotation(m_dragonMotion.lastGroundHitNormal);
            m_stepParticleInstance.gameObject.SetActive(true);
            m_stepParticleInstance.Play();
        }
        m_dragonMotion.OnStep();
    }

    public void GroundHit()
    {
        if (m_dragonMotion.OnGroundStomp())
        {
            OnGroundStomp();
        }
    }

    public void OnGroundStomp()
    {
        PlaySound(m_realGroundStompSound);
        if (m_stompParticleInstance)
        {
            m_stompParticleInstance.transform.position = m_dragonMotion.lastGroundHit;
            m_stompParticleInstance.transform.rotation = Quaternion.LookRotation(m_dragonMotion.lastGroundHitNormal);
            m_stompParticleInstance.gameObject.SetActive(true);
            m_stompParticleInstance.Play();
        }
    }
    
    public void OnHeadButt( Vector3 point, Vector3 normal )
    {
        PlaySound(m_headbuttSound);
        if ( m_headbuttParticleInstance )
        {
            m_headbuttParticleInstance.transform.position = point;
            m_headbuttParticleInstance.transform.rotation = Quaternion.LookRotation(normal);
            // Rotation
            m_headbuttParticleInstance.gameObject.SetActive(true);
            m_headbuttParticleInstance.Play();
        }
    }


    protected override void OnKo( DamageType type , Transform _source)
	{
		base.OnKo(type , _source);
        // Pause Jetpack particle
        m_jetpackParticle.Stop();
	}

	protected override void OnRevive(DragonPlayer.ReviveReason reason)
	{
		base.OnRevive(reason);
        // Resume Jetpack particle
        m_jetpackParticle.Play();
	}

}

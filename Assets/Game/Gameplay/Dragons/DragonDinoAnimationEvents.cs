using UnityEngine;
using UnityEngine.Audio;

public class DragonDinoAnimationEvents : DragonAnimationEvents {

    [Header("Step")]
    public string m_stepSound;
    public ParticleData m_stepParticle;
    ParticleSystem m_stepParticleInstance;

    [Header("Stomp")]
    public string m_groundStompSound;
    public ParticleData m_groundStompParticle;
    public string m_groundStompLevel2Sound;
    public ParticleData m_groundStompParticleLevel2;
    
    ParticleSystem m_stompParticleInstance;
    string m_realGroundStompSound;

    [Header("Headbutt")]
    public string m_headbuttSound;
    public ParticleData m_headbutt;
    ParticleSystem m_headbuttParticleInstance;

    DragonMotionDino m_dragonMotion;

    protected override void Start()
    {
        base.Start();
        if ( m_stepParticle.IsValid() )
        {
            GameObject go = m_stepParticle.CreateInstance();
            m_stepParticleInstance = go.GetComponent<ParticleSystem>();
            m_stepParticleInstance.gameObject.SetActive(false);
        }
        m_dragonMotion = InstanceManager.player.dragonMotion as DragonMotionDino;
        
        if (m_dragonMotion.powerLevel >= 2)
        {
            m_realGroundStompSound = m_groundStompLevel2Sound;
            if ( m_groundStompParticle.IsValid() )
            {
                GameObject go = m_groundStompParticle.CreateInstance();
                m_stompParticleInstance = go.GetComponent<ParticleSystem>();
                m_stompParticleInstance.gameObject.SetActive(false);
            }
        }
        else
        {
            m_realGroundStompSound = m_groundStompSound;
            if ( m_groundStompParticleLevel2.IsValid() )
            {
                GameObject go = m_groundStompParticleLevel2.CreateInstance();
                m_stompParticleInstance = go.GetComponent<ParticleSystem>();
                m_stompParticleInstance.gameObject.SetActive(false);
            }
        }
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

}

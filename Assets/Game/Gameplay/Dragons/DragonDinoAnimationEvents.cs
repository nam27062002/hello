using UnityEngine;
using UnityEngine.Audio;

public class DragonDinoAnimationEvents : DragonAnimationEvents {

    public string m_stepSound;
    
    [Space]
    public ParticleData m_stepParticle;
    ParticleSystem m_stepParticleInstance;

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
    }

    protected void Step()
    {
        PlaySound(m_stepSound);
        if (m_stepParticleInstance)
        {
            Vector3 disp = transform.rotation * m_stepParticle.offset;
            m_stepParticleInstance.transform.position = m_dragonMotion.lastGroundHit + disp;
            m_stepParticleInstance.gameObject.SetActive(true);
            m_stepParticleInstance.Play();
        }
        m_dragonMotion.OnStep();
    }


}

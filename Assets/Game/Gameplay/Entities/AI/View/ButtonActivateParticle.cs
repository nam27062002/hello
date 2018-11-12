using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonActivateParticle : MonoBehaviour {

    public Transform m_particleParent;
    public string m_particleName;
    public AI.Pilot.Action m_action = AI.Pilot.Action.Button_A;
    protected ParticleSystem m_particle;
    protected AI.Pilot m_pilot = null;
    protected bool m_activated = false;

    // Use this for initialization
    private void Awake()
    {
        m_pilot = GetComponent<AI.Pilot>();
        if (!string.IsNullOrEmpty(m_particleName)){
            m_particle = ParticleManager.InitLeveledParticle( m_particleName, m_particleParent );
            m_particle.gameObject.SetActive( true );
            m_particle.Stop();
        }
        
        if ( m_particle == null || m_pilot == null)
        {
            Debug.Log("This is useless");
            
        }
    }

    // Update is called once per frame
    void Update () 
    {
        if ( m_activated )
        {
            if ( !m_pilot.IsActionPressed( m_action) )
            {
                m_particle.Stop();
                m_activated = false;
            }
        }
        else if ( m_pilot.IsActionPressed( m_action ) )
        {
            m_activated = true;
            m_particle.Play();
        }
	}
}

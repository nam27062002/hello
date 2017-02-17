using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AmbientSoundArea : MonoBehaviour 
{
	public string m_ambientSound;	
	//public AudioMixerSnapshot m_onEnterSnapshot;

	void OnTriggerEnter( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
            InstanceManager.musicController.Ambience_Play(m_ambientSound, gameObject);
			
            /*
			if (m_onEnterSnapshot != null)
			{
				InstanceManager.musicController.RegisterSnapshot( m_onEnterSnapshot );
			}*/
				
		}
	}

	void OnTriggerExit( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
            InstanceManager.musicController.Ambience_Stop(m_ambientSound, gameObject);

            /*
			if (m_onEnterSnapshot != null)
			{
				InstanceManager.musicController.UnregisterSnapshot( m_onEnterSnapshot );
			}*/
        }
	}    
}

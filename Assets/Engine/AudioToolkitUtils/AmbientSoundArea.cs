using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AmbientSoundArea : MonoBehaviour 
{
	public string m_ambientSound;	
	private bool m_playerInside = false;

	void OnTriggerEnter( Collider other)
	{
		if ( other.CompareTag("Player") && !m_playerInside)	
		{
			m_playerInside = true;
            InstanceManager.musicController.Ambience_Play(m_ambientSound, gameObject);
		}
	}

	void OnTriggerExit( Collider other)
	{
		if ( other.CompareTag("Player") && m_playerInside)	
		{
			m_playerInside = false;
            InstanceManager.musicController.Ambience_Stop(m_ambientSound, gameObject);
        }
	}    
}

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AmbientSoundArea : MonoBehaviour 
{

	public string m_ambientSound;
	private AudioObject m_ambientSoundAO;
	public AudioMixerSnapshot m_onEnterSnapshot;

	void OnTriggerEnter( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
			if ( !string.IsNullOrEmpty( m_ambientSound ) )
				m_ambientSoundAO = AudioController.PlayAmbienceSound(m_ambientSound);
			if (m_onEnterSnapshot != null)
			{
				InstanceManager.musicController.RegisterSnapshot( m_onEnterSnapshot );
			}
				
		}
	}

	void OnTriggerExit( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
			if ( m_ambientSoundAO != null )
			{
				m_ambientSoundAO.Stop();
				m_ambientSoundAO = null;
			}
			if (m_onEnterSnapshot != null)
			{
				InstanceManager.musicController.UnregisterSnapshot( m_onEnterSnapshot );
			}
		}
	}

}

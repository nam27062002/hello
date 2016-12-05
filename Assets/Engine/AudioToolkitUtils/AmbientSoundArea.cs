using UnityEngine;
using System.Collections;

public class AmbientSoundArea : MonoBehaviour 
{

	public string m_ambientSound;
	private AudioObject m_ambientSoundAO;

	void OnTriggerEnter( Collider other)
	{
		if ( other.tag == "Player" )	
		{
			if ( !string.IsNullOrEmpty( m_ambientSound ) )
				m_ambientSoundAO = AudioController.PlayAmbienceSound(m_ambientSound);
		}
	}

	void OnTriggerExit( Collider other)
	{
		if ( other.tag == "Player" )	
		{
			if ( m_ambientSoundAO != null )
			{
				m_ambientSoundAO.Stop();
				m_ambientSoundAO = null;
			}
		}
	}

}

using UnityEngine;
using System.Collections;

public class AmbientSoundNode : MonoBehaviour 
{

	public string m_ambientSound;
	public bool m_reverb = false;
	private bool m_isUsed = false;

	public delegate void OnEvent( AmbientSoundNode node );
	public OnEvent m_onEnter;
	public OnEvent m_onExit;

	void OnDrawGizmos() 
	{
		if ( !m_isUsed )
			Gizmos.color = new Color(1.0f, 0, 1, 1);
		else
			Gizmos.color = new Color(1.0f, 1.0f, 0.12f, 1);
		Gizmos.DrawSphere(transform.position, 0.5f * transform.localScale.x);

		Gizmos.color = new Color(0.09f, 0.69f, 0.12f, 0.5f);
		Gizmos.DrawRay( transform.position, transform.forward);
	}

	void OnTriggerEnter( Collider other)
	{
		if ( other.tag == "Player" )	
		{
			// Tell Ambient Manager to use this one
			if (m_onEnter != null)
				m_onEnter(this);
		}
	}


	void OnTriggerExit( Collider other)
	{
		if ( other.tag == "Player" )	
		{
			// Tell Ambient Manager to use this one
			if (m_onExit != null)
				m_onExit(this);
		}
	}

}

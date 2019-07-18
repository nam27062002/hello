using UnityEngine;
using System.Collections;

public class FogArea : MonoBehaviour, IBroadcastListener
{
	
	public float m_insideScale = 1.5f;
	public FogManager.FogAttributes m_attributes;
	public bool m_isFireFog = false;

	public bool m_drawInside = false;
	public Vector3 m_startScale;
	private bool m_playerInside = false;

	public float m_enterTransitionDuration = 1.6f;
	public float m_exitTransitionDuration = 1.6f;
    
    private void Awake()
    {
        m_startScale = transform.localScale;
    }
    
    void Start()
	{
		if ( !FeatureSettingsManager.instance.IsFogOnDemandEnabled )
		{
			InstanceManager.fogManager.CheckTextureAvailability(m_attributes);
		}
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_EXIT, this);
	}

	void OnDestroy()
	{
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_EXIT, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_EXIT:
            {
                OnAreaExit();
            }break;
        }
    }

    private void OnDisable()
    {
        if ( m_playerInside && ApplicationManager.IsAlive)
        {
            m_playerInside = false;
            InstanceManager.fogManager.DeactivateArea( this );
            transform.localScale = m_startScale;
        }
    }

    void OnTriggerEnter( Collider other)
	{
		if ( other.CompareTag("Player") && !m_playerInside)	
		{
			m_playerInside = true;
			if ( InstanceManager.player.IsIntroMovement() )
				InstanceManager.fogManager.firstTime = true;
		    InstanceManager.fogManager.ActivateArea( this );
			transform.localScale = m_startScale * m_insideScale;
		}
	}

	void OnTriggerExit( Collider other)
	{
		if ( other.CompareTag("Player") && m_playerInside)	
		{
			m_playerInside = false;
			InstanceManager.fogManager.DeactivateArea( this );
			transform.localScale = m_startScale;
		}
	}

	void OnAreaExit()
	{
		if (!m_playerInside)
		{
			// Fog manager will clean all textures so we lose our reference to it
			m_attributes.texture = null;
		}
	}

	public void EditorFogSetup() {
		if (m_attributes.texture == null)
		{
            FogManager fogManager = FindObjectOfType<FogManager>();
			
			if ( fogManager != null )
			{
				fogManager.CheckTextureAvailability( m_attributes, true);
			}
			else
			{
				m_attributes.CreateTexture();	
			}
		}

		m_attributes.RefreshTexture();

		if (!Application.isPlaying )
		{
			Shader.SetGlobalFloat( GameConstants.Material.FOG_START, m_attributes.m_fogStart);
			Shader.SetGlobalFloat( GameConstants.Material.FOG_END, m_attributes.m_fogEnd);
			Shader.SetGlobalTexture( GameConstants.Material.FOG_TEXTURE, m_attributes.texture);
		}
	}

	void OnDrawGizmosSelected()
	{
		EditorFogSetup();

		if (m_drawInside)
		{
			Color c = m_attributes.m_fogGradient.Evaluate(1);
			c.a = c.a / 2.0f;
			Gizmos.color = c;
			MeshFilter meshFilter = GetComponent<MeshFilter>();
			if ( meshFilter != null )
			{
				Gizmos.DrawMesh( meshFilter.sharedMesh, transform.position, transform.rotation, transform.localScale );	
			}
		}


		Gizmos.color = Color.white;
		Vector3 _from = transform.position;
		_from.z = m_attributes.m_fogStart;
		Vector3 _to = transform.position;
		_to.z = m_attributes.m_fogEnd;

		Gizmos.DrawLine( _from, _to);
		float crossSize = 2;
		Gizmos.DrawLine( _from + Vector3.up * crossSize, _from + Vector3.down * crossSize );
		Gizmos.DrawLine( _from + Vector3.left * crossSize, _from + Vector3.right * crossSize );

		Gizmos.DrawLine( _to + Vector3.up * crossSize, _to + Vector3.down * crossSize );
		Gizmos.DrawLine( _to + Vector3.left * crossSize, _to + Vector3.right * crossSize );
		// 
	}


}

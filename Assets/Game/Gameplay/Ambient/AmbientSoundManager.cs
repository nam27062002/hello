using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientSoundManager : MonoBehaviour 
{
	// Music layers!

	// Ambient Sound
	public AudioSource m_audioSoure_1;
	public AudioSource m_audioSoure_2;
	private int sign;

	private AudioClip m_waitingAudioClip;
	private string m_defaultSound = "Ambient_Battle_Wind";

	List<AmbientSoundNode> m_currentNodes;

	public float m_fadeSoundSpeed = 1.0f;

	void Awake()
	{
		sign = 1;
		m_currentNodes = new List<AmbientSoundNode>();
	}

	IEnumerator Start()
	{
		if ( Application.isPlaying )
		{
			while( !InstanceManager.GetSceneController<GameSceneControllerBase>().IsLevelLoaded())
			{
				yield return null;
			}

			StartCoroutine( AudioClipLoad( m_defaultSound ) );

			// Find all ambient nodes
			AmbientSoundNode[] _ambientNodes = FindObjectsOfType(typeof(AmbientSoundNode)) as AmbientSoundNode[];
			for( int i = 0; i<_ambientNodes.Length; i++ )
			{
				_ambientNodes[i].m_onEnter += OnEnterSoundNode;
				_ambientNodes[i].m_onExit += OnExitSoundNode;
			}
		}
	}

	void Update()
	{
		// Music Layers

		// Ambient Sound
		if ( m_audioSoure_1.clip != null && m_audioSoure_2.clip != null)
		{
			// Fading!
			m_audioSoure_1.volume += Time.deltaTime * sign * m_fadeSoundSpeed;
			m_audioSoure_1.volume = Mathf.Clamp01( m_audioSoure_1.volume );

			m_audioSoure_2.volume += Time.deltaTime * -sign * m_fadeSoundSpeed;
			m_audioSoure_2.volume = Mathf.Clamp01( m_audioSoure_2.volume );

			if ( sign > 0 )
			{
				if ( (m_audioSoure_1.volume >= 1 && m_audioSoure_2.volume <= 0) )
				{
					m_audioSoure_2.Stop();
					m_audioSoure_2.clip = null;
				}
			}
			else
			{
				if ( (m_audioSoure_2.volume >= 1 && m_audioSoure_1.volume <= 0) )
				{
					m_audioSoure_1.Stop();
					m_audioSoure_1.clip = null;
				}
			}
		}
		else if ( m_waitingAudioClip != null )
		{
			if ( m_audioSoure_1.clip == null )
			{
				m_audioSoure_1.clip = m_waitingAudioClip;
				m_audioSoure_1.Play();
				sign = 1;
			}
			else
			{
				m_audioSoure_2.clip = m_waitingAudioClip;
				m_audioSoure_2.Play();
				sign = -1;
			}
			m_waitingAudioClip = null;
		}
		else
		{
			// First Case!!
			// if just one of them has clip it volume should be 1
			if ( m_audioSoure_1.clip != null )
			{
				m_audioSoure_1.volume += Time.deltaTime;
				if ( m_audioSoure_1.volume >= 1 )
					m_audioSoure_1.volume = 1;
			}
			else if ( m_audioSoure_2.clip != null )
			{
				m_audioSoure_2.volume += Time.deltaTime;
				if ( m_audioSoure_2.volume >= 1 )
					m_audioSoure_2.volume = 1;
			}
		}
	}


	public void OnEnterSoundNode( AmbientSoundNode node )
	{
		if ( !m_currentNodes.Contains(node) )
		{
			m_currentNodes.Add( node );
			StartCoroutine( AudioClipLoad( node.m_ambientSound ) );
		}
	}

	private void OnExitSoundNode( AmbientSoundNode node )
	{
		if ( m_currentNodes.Contains(node) )
		{
			m_currentNodes.Remove(node);
			if ( m_currentNodes.Count <= 0 )
				StartCoroutine( AudioClipLoad( m_defaultSound ) );
		}
	}


	IEnumerator AudioClipLoad( string audioName )
	{
		ResourceRequest request = Resources.LoadAsync<AudioClip>( "audio/sfx/" + audioName );
		yield return request;

		AudioClip clip = request.asset as AudioClip;
		if ( m_audioSoure_1.clip == null )
		{
			m_audioSoure_1.clip = clip;
			m_audioSoure_1.volume = 0;
			m_audioSoure_1.Play();
			sign = 1;
		}
		else if ( m_audioSoure_2.clip == null )
		{
			m_audioSoure_2.clip = clip;
			m_audioSoure_2.volume = 0;
			m_audioSoure_2.Play();
			sign = -1;
		}
		else
		{
			m_waitingAudioClip = clip;	
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientSoundManager : MonoBehaviour 
{
	DragonPlayer m_player;

	// Music layers!
	AudioManager m_audioManager;
	private float m_furyVolume;
	private float m_starvingVolume;

	private bool m_slowMotionOn;
	private bool m_slowMoJustStarted;

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

		m_audioManager = AudioManager.instance;

		/*
		m_audioManager.MusicCrossFade( AudioManager.Channel.DEFAULT, "audio/music/Piano", 0.1f);

		m_audioManager.MusicCrossFade( AudioManager.Channel.LAYER_1, "audio/music/Synth", 0.1f);
		m_furyVolume = 0;
		m_audioManager.SetMusicVolume( AudioManager.Channel.LAYER_1, m_furyVolume);

		m_audioManager.MusicCrossFade( AudioManager.Channel.LAYER_2, "audio/music/Sax", 0.1f);
		m_starvingVolume = 0;
		m_audioManager.SetMusicVolume( AudioManager.Channel.LAYER_2, m_starvingVolume);
		*/
#if UNITY_EDITOR
		// m_audioManager.MuteAll();
#endif
		Messenger.AddListener<bool>(GameEvents.SLOW_MOTION_TOGGLED, OnSlowMotion);


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

			m_player = InstanceManager.player;
		}
	}

	private void OnDestroy() 
	{
		Messenger.RemoveListener<bool>(GameEvents.SLOW_MOTION_TOGGLED, OnSlowMotion);

	}

	void Update()
	{
		// Music Layers
// #if !UNITY_EDITOR
		if ( m_player != null )
		{
			if ( m_player.IsFuryOn() )
				m_furyVolume += Time.deltaTime;
			else
				m_furyVolume -= Time.deltaTime;
			m_furyVolume = Mathf.Clamp01( m_furyVolume );
			m_audioManager.SetMusicVolume( AudioManager.Channel.LAYER_1, m_furyVolume);

			if ( m_player.IsStarving() )
				m_starvingVolume += Time.deltaTime;
			else
				m_starvingVolume -= Time.deltaTime;
			m_starvingVolume = Mathf.Clamp01( m_starvingVolume );
			m_audioManager.SetMusicVolume( AudioManager.Channel.LAYER_2, m_starvingVolume);
		}
// #endif

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
			if ( node.m_reverb )
				m_audioManager.SfxReverb( 0.5f );
			StartCoroutine( AudioClipLoad( node.m_ambientSound ) );
		}
	}

	private void OnExitSoundNode( AmbientSoundNode node )
	{
		if ( m_currentNodes.Contains(node) )
		{
			m_currentNodes.Remove(node);
			if ( m_currentNodes.Count <= 0 )
			{
				m_audioManager.SfxNormal( 0.5f);
				StartCoroutine( AudioClipLoad( m_defaultSound ) );
			}
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


	//------------------------------------------------------------------//
	// Callbacks														//
	//------------------------------------------------------------------//

	private void OnSlowMotion( bool _enabled)
	{
		m_slowMotionOn = _enabled;
		m_slowMoJustStarted = _enabled;
	}

}

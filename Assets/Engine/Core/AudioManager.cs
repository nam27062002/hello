using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioManager :  SingletonMonoBehaviour<AudioManager> 
{
	private const float AUDIO_MUSIC_VOLUME_DEFAULT = 0;
	private const float AUDIO_SFX_VOLUME_DEFAULT = 0;


	public AudioMixer m_masterMixer;
	// With Music group we will control music volume
	public AudioMixerGroup m_musicGroup;
	// With sfx group we will control all sounds volume and other effects
	public AudioMixerGroup m_sfxGroup;
	public AudioMixerSnapshot m_sfxNormalSnapshot;
	public AudioMixerSnapshot m_sfxReverbSnapshot;


	// Music variables
	private float m_masterMusicVolume;	// dB
	private bool m_masterMusicMuted;
	public enum Channel
	{
		DEFAULT,
		LAYER_1,
		LAYER_2,
		ALL
	}
	Dictionary<Channel, MusicChannel> m_musicChannels;
	Channel m_maxValidChannel = Channel.LAYER_2;	// We will use this to control different setups 

	// Sfx variables
	float m_sfxVolume = 0;	// dB
	bool m_sfxMuted;
	Pool m_audioSourcePool;
	
	void Awake()
	{
		m_masterMusicMuted = false;
		SetMasterMusicVolume( AUDIO_MUSIC_VOLUME_DEFAULT );
		m_sfxMuted = false;
		SetSfxVolume( AUDIO_SFX_VOLUME_DEFAULT );

		m_musicChannels = new Dictionary<Channel, MusicChannel>();

		GameObject prefab = (GameObject)Resources.Load("audio/AudioSource");
		m_audioSourcePool = new Pool(prefab, transform, 5, true, false);
	}
	
	// Use this for initialization
	void Start () 
	{
		/*
		SettingsManager settingsMng = InstanceManager.SettingsManager;
		if (!settingsMng.IsMusicEnabled())
			MuteMusic();
		if (!settingsMng.IsSoundEnabled())
			MuteSfx();
		*/
	}

	void Update()
	{
		foreach( KeyValuePair<Channel, MusicChannel> pair in m_musicChannels)
			pair.Value.Update( Time.deltaTime );
	}
	
	public void MuteAll()
	{
		MuteMusic();
		MuteSfx();
	}
	
	public void UnMuteAll()
	{
		UnMuteMusic();
		UnMuteSfx();
	}
	
	// MUSIC FUNCTIONS
	public void MuteMusic()
	{
		m_masterMusicMuted = true;
		m_masterMixer.SetFloat("MusicVolume", -80);
	}

	public void UnMuteMusic( )
	{
		m_masterMusicMuted = false;
		SetMasterMusicVolume( m_masterMusicVolume);
	}

	public void SetMasterMusicVolume( float dB )
	{
		m_masterMusicVolume = dB;
		if ( !m_masterMusicMuted )
			m_masterMixer.SetFloat("MusicVolume", m_masterMusicVolume);	
	}

	public void SetMusicVolume( Channel _channel = Channel.DEFAULT, float volume = 0, float transitionTime = 0)
	{
		if ( _channel == Channel.ALL )
		{
			foreach( KeyValuePair<Channel, MusicChannel> p in m_musicChannels )
				p.Value.SetMusicVolume( volume, transitionTime );
		}
		else
		{
			if ( m_musicChannels.ContainsKey( _channel) )
			{
				m_musicChannels[ _channel ].SetMusicVolume( volume, transitionTime );
			}
		}
	}

	public void MusicCrossFade( Channel _channel,  string fileName, float transitionDuration, bool loop = true, bool _pauseCurrent = false, float _nextMusicStartTime = 0)
	{
		if ( _channel == Channel.ALL )
		{
			Debug.Log("Not a Valid Channel");
			return;
		}

		if (!string.IsNullOrEmpty(fileName))
		{
			AudioClip aClip = Resources.Load(fileName) as AudioClip;
			if ( aClip != null)
				MusicCrossFade( _channel, aClip, transitionDuration, loop, _pauseCurrent, _nextMusicStartTime );
			else
				Debug.LogError( "Missing music " + fileName );
		}
	}

	public void MusicCrossFade( Channel _channel, AudioClip audio, float transitionDuration, bool loop = true, bool _pauseCurrent = false, float _nextMusicStartTime = 0)
	{
		if ( _channel == Channel.ALL )
		{
			Debug.Log("Not a Valid Channel");
			return;
		}

		if ( !m_musicChannels.ContainsKey( _channel ) )
		{
			// Create channel
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.loop = true;
			audioSource.outputAudioMixerGroup = m_musicGroup;
			MusicChannel mc = new MusicChannel( audioSource );
			m_musicChannels.Add(_channel, mc);
		}

		m_musicChannels[ _channel ].MusicCrossFade(audio, transitionDuration, loop, _pauseCurrent, _nextMusicStartTime);
		
	}

	public bool IsMusicChannelPlaying( Channel _channel )
	{
		return false;
	}

	/// 
	/// SFX SECTION
	/// 

	// SFX Functions
	public void MuteSfx()
	{
		m_sfxMuted = true;
		m_masterMixer.SetFloat("MusicVolume", -80);
	}
	
	public void UnMuteSfx()
	{
		m_sfxMuted = false;
		SetSfxVolume( m_sfxVolume );
	}

	public void SetSfxVolume( float dB )
	{
		m_sfxVolume = dB;
		if ( !m_sfxMuted )
		{
			m_masterMixer.SetFloat("MusicVolume", m_sfxVolume);
		}
	}

	public void SfxReverb( float transition )
	{
		m_sfxReverbSnapshot.TransitionTo( transition );
	}

	public void SfxNormal( float transition )
	{
		m_sfxNormalSnapshot.TransitionTo( transition );
	}

	public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 pos, float pitch = 1.0f)
	{
		GameObject tempGO = m_audioSourcePool.Get();
		tempGO.transform.position = pos; // set its position
		AudioSource aSource = tempGO.GetComponent<AudioSource>();
		aSource.clip = clip; // define the clip
		// set other aSource properties here, if desired

		tempGO.GetComponent<FollowTransform>().m_follow = null;
		aSource.rolloffMode = AudioRolloffMode.Linear;	// Default roll off
		aSource.volume = 1.0f;
		aSource.pitch = pitch;

		aSource.Play(); // start the sound		
		// Destroy(tempGO, clip.length); // destroy object after clip duration
		StartCoroutine(ReturnToPool( tempGO, clip.length / pitch ));
		return aSource; // return the AudioSource reference
	}
	
	public AudioSource PlayClipAndFollow(AudioClip clip, Transform toFollow, float pitch = 1.0f)
	{
		GameObject tempGO = m_audioSourcePool.Get();
		tempGO.transform.localPosition = Vector3.zero;
		AudioSource aSource = tempGO.GetComponent<AudioSource>();
		aSource.clip = clip; // define the clip
		// set other aSource properties here, if desired

		tempGO.GetComponent<FollowTransform>().m_follow = toFollow;
		aSource.rolloffMode = AudioRolloffMode.Linear;	// Default roll off
		aSource.volume = 1.0f;
		aSource.pitch = pitch;

		aSource.Play(); // start the sound		
		StartCoroutine(ReturnToPool( tempGO, clip.length / pitch ));
		return aSource; // return the AudioSource reference
	}
	
	/**
	*	Searchs a clip on resources and plays it on position 0,0,0. Useful for 2D sounds
	*/
	public AudioSource PlayClip( string sfx, float pitch = 1f )
	{
		if (!string.IsNullOrEmpty(sfx))
		{
			AudioClip aClip = Resources.Load(sfx) as AudioClip;
			if ( aClip != null)
				return PlayClipAtPoint( aClip, Vector3.zero, pitch );
			Debug.LogError( "Missing sfx " + sfx );
		}
		return null;
	}
	
	public AudioSource PlayClip(AudioClip aClip)
	{
		if (aClip != null)
		{
			return PlayClipAtPoint( aClip, Vector3.zero );
		}
		return null;
	}
	
	IEnumerator ReturnToPool(GameObject obj, float time)
	{
		yield return new WaitForSeconds(time);		

		obj.GetComponent<AudioSource>().clip = null;
		obj.GetComponent<FollowTransform>().m_follow = null;		
		//audioSourcePool.ReturnToPool(obj);
		obj.SetActive(false);
	}

	public void FreeMemory()
	{		
		m_audioSourcePool.Clear();
	}

	/// 
	/// END SFX SECTION
	/// 
}

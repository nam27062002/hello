using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager :  SingletonMonoBehaviour<AudioManager> 
{
	private const float AUDIO_MUSIC_VOLUME_DEFAULT = 0.5f;
	private const float AUDIO_SFX_VOLUME_DEFAULT = 1f;

	// With AudioListener.volume we will control all sfx volume
	// Music AudioSource will have ignoreListenerVolume set to true and we will controll music volume manually
	
	// Mute -> AudioListener.volume = 0 and AudioSource for music volume = 0
	// Music volume level -> set AudioSource for music volume
	// Sfx volume -> AudioListener.volume = volume value
	
	// Music variables
	AudioSource m_musicSource;
	AudioClip m_nextMusic = null;
	bool m_nextLoop = true;
	float m_nextTime = 0;
	
	float m_targetMusicVolume = 0;
	float m_currentMusicVolume = 0;
	float m_startMusicVolume = 0;
    bool m_pauseCurrent = false;
	
	float m_muteMusicVolume;

    private Dictionary<string, float> m_pausedMusics;
	
	public enum EMusicState
	{
		MUSIC_GOING_OUT,
		MUSIC_GOUND_IN,
		IDLE
	}
	public EMusicState m_currentState = EMusicState.IDLE;
	
	float m_transitionDuration = 0;
	float m_currentTransitionTime = 0;
	
	// Sfx variables
	float m_sfxVolume = 0;
	float m_muteSfxVolume;
	Pool m_audioSourcePool;
	
	void Awake()
	{
		m_musicSource = gameObject.AddComponent<AudioSource>();
		m_musicSource.ignoreListenerVolume = true;
		
		m_targetMusicVolume = AUDIO_MUSIC_VOLUME_DEFAULT;
		m_currentMusicVolume = AUDIO_MUSIC_VOLUME_DEFAULT;
		m_muteMusicVolume = AUDIO_MUSIC_VOLUME_DEFAULT;
		
		m_musicSource.volume = m_currentMusicVolume;
		m_musicSource.loop = true;
		
		m_sfxVolume = AUDIO_SFX_VOLUME_DEFAULT;
		m_muteSfxVolume = AUDIO_SFX_VOLUME_DEFAULT;

		GameObject prefab = (GameObject)Resources.Load("audio/AudioSource");
		m_audioSourcePool = new Pool(prefab, instance.transform, 5, true, false);
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
		if (m_targetMusicVolume > 0) {
			m_muteMusicVolume = m_targetMusicVolume;
			SetMusicVolume( 0 );
		}
	}

	public void UnMuteMusic()
	{
		SetMusicVolume( m_muteMusicVolume );
	}
	
	public void SetMusicVolume( float vol, float transitionTime = 0)
	{
		m_targetMusicVolume = vol;
		if (transitionTime == 0)
		{
			m_currentState = EMusicState.IDLE;
			m_musicSource.volume = vol;
			m_currentMusicVolume = vol;
		}
		else
		{
			m_startMusicVolume = m_currentMusicVolume;
			m_currentState = EMusicState.MUSIC_GOUND_IN;
			m_currentTransitionTime = 0;
			m_transitionDuration = transitionTime;
		}
		
		
	}

	void PlayMusic( AudioClip music, bool loop, float time = 0 )
	{
        if (m_musicSource.isPlaying)
        {
            if (m_musicSource.clip.name == music.name)
                return;

            m_musicSource.Stop();
        }
					
		m_musicSource.clip = music;
		m_musicSource.loop = loop;
		m_musicSource.time = time;
		m_musicSource.Play();
	}

    void PauseMusic()
    {
        if (m_musicSource.isPlaying)
        {
            if (m_pausedMusics == null)
                m_pausedMusics = new Dictionary<string, float>();

            if (!m_pausedMusics.ContainsKey(m_musicSource.clip.name))
                m_pausedMusics.Add(m_musicSource.clip.name, m_musicSource.time);
            else
                m_pausedMusics[m_musicSource.clip.name] = m_musicSource.time;

            m_musicSource.Stop();
        }
    }


    public void UnpauseMusic(string musicName)
    {
        if (!string.IsNullOrEmpty(musicName))
        {
            AudioClip aClip = Resources.Load(musicName) as AudioClip;
            if (aClip != null)
                UnpauseMusic(aClip);
            else
                Debug.LogError("Missing music " + musicName);
        }

    }

    public void UnpauseMusic(AudioClip clip)
    {
        if (!m_musicSource.isPlaying)
        {
            m_musicSource.clip = clip;

            if (m_pausedMusics != null && m_pausedMusics.ContainsKey(clip.name))
                m_musicSource.time = m_pausedMusics[clip.name];

            m_musicSource.Play();
        }
    }
	
	public void MusicCrossFade( string fileName, float duration, bool loop = true, bool _pauseCurrent = false, float[] arr = null)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			AudioClip aClip = Resources.Load(fileName) as AudioClip;
			if ( aClip != null)
				MusicCrossFade( aClip, duration, loop, _pauseCurrent, arr );
			else
				Debug.LogError( "Missing music " + fileName );
		}
	}
	
	public void MusicCrossFade( AudioClip music, float duration, bool loop = true, bool _pauseCurrent = false, float[] arr = null)
	{
		m_transitionDuration = duration / 2.0f;
		if (m_musicSource.isPlaying)
		{
			if ( m_musicSource.clip != music )
			{
				m_currentTransitionTime = 0;
				m_startMusicVolume = m_currentMusicVolume;
				m_currentState = EMusicState.MUSIC_GOING_OUT;
				m_nextMusic = music;
				m_nextLoop = loop;
				m_nextTime = 0;
				if (arr != null)
				{
					m_nextTime = arr[Random.Range(0, arr.Length)];
				}
                m_pauseCurrent = _pauseCurrent;
			}
		}
		else
		{
			m_nextTime = 0;
			if (arr != null)
			{
				m_nextTime = arr[Random.Range(0, arr.Length)];
			}
			m_pauseCurrent = false;
            m_currentMusicVolume = 0;
			m_musicSource.volume = 0;
			PlayMusic( music, loop, m_nextTime );
			SetMusicVolume( m_targetMusicVolume, m_transitionDuration );
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch( m_currentState )
		{
			case EMusicState.MUSIC_GOING_OUT:
			{
				m_currentTransitionTime += Time.deltaTime;
				float delta = m_currentTransitionTime / m_transitionDuration;
				if (delta >= 1) 
				{
					delta = 1;
					
					m_currentMusicVolume = 0;
					m_musicSource.volume = 0;
				
					if (m_nextMusic != null)	
					{
                        if (m_pauseCurrent)
                            PauseMusic();

                        PlayMusic( m_nextMusic, m_nextLoop, m_nextTime );
						SetMusicVolume( m_targetMusicVolume, m_transitionDuration );
					}
					else
					{
						m_musicSource.Stop();
						m_currentState = EMusicState.IDLE;
					}
				}
				else
				{
					m_currentMusicVolume = Mathf.Lerp( m_startMusicVolume, 0, delta);
					m_musicSource.volume = m_currentMusicVolume;
				}
				
			}break;
			case EMusicState.MUSIC_GOUND_IN:
			{
				m_currentTransitionTime += Time.deltaTime;
				float delta = m_currentTransitionTime / m_transitionDuration;
				if (delta >= 1) 
				{
					delta = 1;
					SetMusicVolume( m_targetMusicVolume, 0 );
					m_currentState = EMusicState.IDLE;
				}
				else
				{
					m_currentMusicVolume = Mathf.Lerp( m_startMusicVolume, m_targetMusicVolume, delta);
					m_musicSource.volume = m_currentMusicVolume;
				}
			}break;
		}
	}
	
	// SFX Functions
	public void SetSfxVolume( float vol )
	{
		m_sfxVolume = vol;
		AudioListener.volume = vol;
	}
	
	public void MuteSfx()
	{
		if (m_sfxVolume > 0)
		{
			m_muteSfxVolume = m_sfxVolume;
			SetSfxVolume(0);
		}
	}
	
	public void UnMuteSfx()
	{
		SetSfxVolume( m_muteSfxVolume );
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
}

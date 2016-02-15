using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Music variables
public class MusicChannel
{
	private const float AUDIO_MUSIC_VOLUME_DEFAULT = 1.0f;

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

	public MusicChannel( AudioSource source )
	{
		m_musicSource = source;

		m_musicSource.ignoreListenerVolume = true;
	
		m_targetMusicVolume = AUDIO_MUSIC_VOLUME_DEFAULT;
		m_currentMusicVolume = AUDIO_MUSIC_VOLUME_DEFAULT;
		m_muteMusicVolume = AUDIO_MUSIC_VOLUME_DEFAULT;
		
		m_musicSource.volume = m_currentMusicVolume;
		m_musicSource.loop = true;
	}

	public void Mute()
	{
		if (m_targetMusicVolume > 0) 
		{
			m_muteMusicVolume = m_targetMusicVolume;
			SetMusicVolume( 0 );
		}
	}

	public void UnMute()
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
	
	public void MusicCrossFade( string fileName, float duration, bool loop = true, bool _pauseCurrent = false, float _nextMusicStartTime = 0)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			AudioClip aClip = Resources.Load(fileName) as AudioClip;
			if ( aClip != null)
				MusicCrossFade( aClip, duration, loop, _pauseCurrent, _nextMusicStartTime );
			else
				Debug.LogError( "Missing music " + fileName );
		}
	}
	
	public void MusicCrossFade( AudioClip music, float transitionDuration, bool loop = true, bool _pauseCurrent = false, float _nextMusicStartTime = 0)
	{
		m_transitionDuration = transitionDuration / 2.0f;
		if (m_musicSource.isPlaying)
		{
			if ( m_musicSource.clip != music )
			{
				m_currentTransitionTime = 0;
				m_startMusicVolume = m_currentMusicVolume;
				m_currentState = EMusicState.MUSIC_GOING_OUT;
				m_nextMusic = music;
				m_nextLoop = loop;
				m_nextTime = _nextMusicStartTime;
                m_pauseCurrent = _pauseCurrent;
			}
		}
		else
		{
			m_nextTime = _nextMusicStartTime;
			m_pauseCurrent = false;
            m_currentMusicVolume = 0;
			m_musicSource.volume = 0;
			PlayMusic( music, loop, m_nextTime );
			SetMusicVolume( m_targetMusicVolume, m_transitionDuration );
		}
	}



	// Update is called once per frame
	public void Update ( float deltaTime ) 
	{
		switch( m_currentState )
		{
			case EMusicState.MUSIC_GOING_OUT:
			{
				m_currentTransitionTime += deltaTime;
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
				m_currentTransitionTime += deltaTime;
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
}
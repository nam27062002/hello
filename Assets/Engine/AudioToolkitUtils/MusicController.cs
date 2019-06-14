using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour, IBroadcastListener
{
    List<AudioObject> m_delayedReturnObjects = new List<AudioObject>();
    #region monobehaviour
    // Use this for initialization
    void Awake ()
    {        
        InstanceManager.musicController = this;

        m_defaultAudioSnapshot = InstanceManager.masterMixer.FindSnapshot("Default");

        Messenger.AddListener<string>(MessengerEvents.SCENE_PREUNLOAD, OnScenePreunload);
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);

        Reset();        

        Music_Init();
	}

    void OnDestroy()
    {
		Messenger.RemoveListener<string>(MessengerEvents.SCENE_PREUNLOAD, OnScenePreunload);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
        InstanceManager.musicController = null;
    }	        	

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
                    OnGameLevelLoaded();
            }break;
            case BroadcastEventType.FURY_RUSH_TOGGLED:
            {
                FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                OnFuryRushToggled(furyRushToggled.activated, furyRushToggled.type);
            }break;
        }
    }

    void Update()
    {
        if (IsEnabled)
        {
            Music_Update();
        }

        int max = m_delayedReturnObjects.Count - 1;
        for (int i = max; i >= 0; --i)
        {
            AudioObject ao = m_delayedReturnObjects[i];
            ao.transform.parent = null;
            ao.completelyPlayedDelegate = null;
            if (ao.IsPlaying() && ao.audioItem.Loop != AudioItem.LoopMode.DoNotLoop)
                ao.Stop();
        }
        m_delayedReturnObjects.Clear();
    }

    private void Reset()
    {
        Music_StopCurrent();
        Music_CurrentKey = null;
        Music_LastKey = null;
        Ambience_ToPlay.Reset();
        Music_OffsetAccummulated = 0f;
        Music_Lengths = null;
        secondsToSwitchMusic = 0;

        // We don't want the music to start playing on the loading screen so we need to wait for the game to load completely before starting playing the music
        IsEnabled = false;
    }
    #endregion

    public bool IsEnabled { get; set; }

    private void OnScenePreunload(string scene)
    {
        Reset();
    }

    private void OnGameLevelLoaded()
    {        
        IsEnabled = true;
    }


    #region main_music
    // This region is responsible for handling the main music of the game. The main music is the music that has to be played by default, however there are some areas in the game, such as the
    // castle, where an ambience music instead of the default music has to be played

    public string m_mainMusicKey = "amb_bed";
    public string m_fireRushMusic = "";
    public string m_megaFireRushMusic = "";
    private bool m_useFireRushMusic = false;
	private DragonBreathBehaviour.Type m_fireRushType = DragonBreathBehaviour.Type.None;
    #endregion

    #region music
    // This region is responsible for handling the music being played or the one to be played. For the sake of robustness and performance only one music is allowed to be played simultaneously

    public float m_musicFadeOut = 0.3f;

    public float m_musicVolume = 0.2f;

    /// <summary>
    /// Returns the key of the music that is the current music to play
    /// </summary>
    private string Music_CurrentKey { get; set; }
    private string Music_LastKey { get; set; }

    public float minSecondsToSwitchMusic = 20.0f;
    private float secondsToSwitchMusic;

    /// <summary>
    /// Returns whether or not a music is being played
    /// </summary>
    private bool Music_IsPlaying
    {
        get
        {
            return string.IsNullOrEmpty(Music_CurrentKey);
        }
    }

    private AudioObject Music_CurrentAudioObject { get; set; }

    /// <summary>
    /// Key: music key
    /// Value: Length in seconds
    /// </summary>
    private Dictionary<string, float> Music_Lengths { get; set; }

    private void Music_Init()
    {
        Music_Lengths = new Dictionary<string, float>();

        // We need to loop through all audio items in MUSIC category to retrieve their length.
        // We want all musics to be in sync so all musics must have the same duration or a whole fraction of the longest duration
        AudioCategory category = AudioController.GetCategory(AMBIENCE_CATEGORY_MUSIC);
        if (category != null)
        {
            AudioItem[] items = category.AudioItems;
            if (items != null)
            {                                
                AudioSubItem[] subitems;                
                int count = items.Length;
                for (int i = 0; i < count; i++)
                {
                    subitems = items[i].subItems;
                    if (subitems != null)
                    {
                        if (subitems.Length > 0)
                        {                            
                            Music_Lengths.Add(items[i].Name, subitems[0].Clip.length);                           
                        }                        
                    }
                }
            }
        }
    }

    /*
    private void Music_PlayCurrent(float offset)
    {        
        if (!string.IsNullOrEmpty(Music_CurrentKey) && Music_Lengths.ContainsKey(Music_CurrentKey))
        {           
            float length = Music_Lengths[Music_CurrentKey];
            if (offset > length)
            {
                Music_OffsetAccummulated = ((int)(offset / length)) * length;
                offset %= length;
            }
            else
            {
                Music_OffsetAccummulated = 0;
            }

            Music_CurrentAudioObject = AudioController.PlayMusic(Music_CurrentKey, m_musicVolume, 0, offset);                        
        }
        else
        {
            LogError("<" + Music_CurrentKey + " is not a valid music to play");
        }
    }
    */

    private void Music_StopCurrent()
    {
        if (!string.IsNullOrEmpty(Music_CurrentKey) && secondsToSwitchMusic <= 0)
        {
            AudioController.StopMusic(m_musicFadeOut);
            Music_LastKey = Music_CurrentKey;
            Music_CurrentKey = null;
            Music_CurrentAudioObject = null;
        }
    }
    
    private float Music_OffsetAccummulated { get; set; }

    private void Music_Update()
    {
        // By default the main music has to be played, unless there's an ambience music
        if (secondsToSwitchMusic > 0)
        {
            secondsToSwitchMusic -= Time.deltaTime;
        }

		string keyToPlay = null;
		bool waitToPlay = false;
		float musicFadeOut = m_musicFadeOut;
		if (!m_useFireRushMusic)
		{
			if ( !m_waitingMusicToFinish )
			{
				if (Ambience_ToPlay.IsValid())
		        {
		            keyToPlay = Ambience_ToPlay.music_key;
					waitToPlay = Ambience_ToPlay.wait_to_finish;
		        }
		        else
		        {
		        	keyToPlay = m_mainMusicKey;
		        }
	        }           
	        else
	        {
				keyToPlay = Music_CurrentKey;
	        }
        }
        else
        {
        	switch( m_fireRushType )
        	{
				default: 
        		case DragonBreathBehaviour.Type.Standard: keyToPlay = m_fireRushMusic;break;
				case DragonBreathBehaviour.Type.Mega: keyToPlay = m_megaFireRushMusic;break;

        	}
			musicFadeOut = 0;
            secondsToSwitchMusic = 0;
        }
          
        if (secondsToSwitchMusic <= 0)
		if (keyToPlay != Music_CurrentKey || (Music_CurrentAudioObject != null && (Music_CurrentAudioObject.IsPaused(true) || !Music_CurrentAudioObject.IsPlaying())) )
        {
			if (Music_CurrentAudioObject != null)
			{
				if (!Music_CurrentAudioObject.IsPlaying() || musicFadeOut <= 0)
				{
					Music_CurrentAudioObject.Stop();	// Force stop
					Music_CurrentKey = keyToPlay;
					Music_CurrentAudioObject = AudioController.PlayMusic(Music_CurrentKey, m_musicVolume);
                    if ( Music_CurrentAudioObject != null )
                    {
    					m_waitingMusicToFinish = waitToPlay;
    					if ( m_waitingMusicToFinish )
    					{
    						Ambience_Stop( Ambience_ToPlay.music_key, Ambience_ToPlay.game_object);
    						Music_CurrentAudioObject.completelyPlayedDelegate = OnMusicCompleted;
    					}
    					// AudioController.PauseMusic();	// Pause Music manually to fade in while unpausing!
    					// AudioController.UnpauseMusic( musicFadeOut );
    					if ( musicFadeOut > 0 )
    						Music_CurrentAudioObject.FadeIn( musicFadeOut );
                        secondsToSwitchMusic = minSecondsToSwitchMusic;
                    }
                }
				else if ( Music_CurrentAudioObject.IsPlaying() && !Music_CurrentAudioObject.isFadingOut)
				{
                    //Fading out
					// AudioController.PauseMusic( musicFadeOut );
					AudioController.StopMusic(musicFadeOut);
				}
			}
			else
			{
				Music_CurrentKey = keyToPlay;
				Music_CurrentAudioObject = AudioController.PlayMusic(Music_CurrentKey, m_musicVolume);
                if (Music_CurrentAudioObject != null)
                {
    				m_waitingMusicToFinish = waitToPlay;
    				if ( m_waitingMusicToFinish )
    				{
    					Music_CurrentAudioObject.completelyPlayedDelegate = OnMusicCompleted;
    				}
                    secondsToSwitchMusic = minSecondsToSwitchMusic;
                }
            }
           
        }
    }

	void OnMusicCompleted( AudioObject ao )
	{
		m_waitingMusicToFinish = false;
	}


	void OnFuryRushToggled( bool fire, DragonBreathBehaviour.Type fireType)
	{
		m_useFireRushMusic = fire;
		m_fireRushType = fireType;
	}

    #endregion

    #region ambience
    // This region is responsible for playing ambience musics. An ambience music is the music that has to be played instead of the main music in some map areas such as the castle

	private const string AMBIENCE_CATEGORY_MUSIC = "MUSIC";
    private const string AMBIENCE_CATEGORY_SFX = "SFX";
    private const string AMBIENCE_CATEGORY_SFX_2D = "SFX 2D";

    struct MusicPlaying
    {
    	public GameObject game_object;
    	public string music_key;
    	public bool wait_to_finish;

    	public MusicPlaying(string key, GameObject go, bool wait )
    	{
    		music_key = key;
    		game_object = go;
    		wait_to_finish = wait;
    	}

    	public void Reset()
    	{
			music_key = "";
    		game_object = null;
    		wait_to_finish = false;
    	}

    	public bool IsValid()
    	{
    		return !string.IsNullOrEmpty(music_key);
    	}

    }
    List<MusicPlaying> m_musicsPlaying;
	List<MusicPlaying> m_priorityMusicsPlaying;
	private MusicPlaying Ambience_ToPlay;

    private bool m_waitingMusicToFinish = false;

    /// <summary>
    /// Plays the ambience music of sound effect corresponding to the key passed as a parameter.
    /// </summary>
    /// <param name="key">Key of the music of sound effect to play. This key has to exist in AudioController_GamePlay2D</param>
    public void Ambience_Play(string key, GameObject from)
    {
        // Checks if the key is a music or sound effect
        AudioItem audioItem = AudioController.GetAudioItem(key);
        if (audioItem == null)
        {
            LogError("No audio item defined for key <" + key + "> in any AudioController");
        }
        else
        {
            string categoryName = audioItem.category.Name;
            if (categoryName == AMBIENCE_CATEGORY_MUSIC)
            {
				// Register object
				if (m_musicsPlaying == null || m_priorityMusicsPlaying == null)
				{
					m_musicsPlaying = new List<MusicPlaying>();
					m_priorityMusicsPlaying = new List<MusicPlaying>();
				}

            	// Check if music has to play!!
            	AudioItem item = AudioController.GetAudioItem(key);
				MusicPlaying mp = new MusicPlaying( key, from, item.Loop == AudioItem.LoopMode.DoNotLoop );
				if ( item.Loop == AudioItem.LoopMode.DoNotLoop )
				{
					if (item._lastPlayedTime <= 0 || (AudioController.systemTime - item._lastPlayedTime) >= item.MinTimeBetweenPlayCalls)
					{
						// Add it
						m_priorityMusicsPlaying.Add( mp );
						Ambience_ToPlay = mp;
					}
				}
				else
				{
					m_musicsPlaying.Add( mp );
					if ( m_priorityMusicsPlaying.Count <= 0)
					{
						Ambience_ToPlay = mp;	
					}
				}
            }
            else if (categoryName == AMBIENCE_CATEGORY_SFX || categoryName == AMBIENCE_CATEGORY_SFX_2D )
            {
                AmbienceSfx_Play(key, from);
            }
            else
            {
                LogError("Category <" + categoryName + ">  defined for key <" + key + "> can not be used for ambience");
            }
        }        
    }

    public void Ambience_Stop(string key, GameObject from)
    {
        // Checks if the key is a music or a sound effect
        AudioItem audioItem = AudioController.GetAudioItem(key);
        if (audioItem == null)
        {
            LogError("No audio item defined for key <" + key + "> in any AudioController");
        }
        else
        {
            string categoryName = audioItem.category.Name;
            if (categoryName == AMBIENCE_CATEGORY_MUSIC)
            {
            	// unregister object
				int count = m_musicsPlaying.Count;
                for( int i = m_musicsPlaying.Count - 1; i >= 0; --i )
                {
                	if ( m_musicsPlaying[i].game_object == from )	
                	{
                		m_musicsPlaying.RemoveAt(i);
                		break;
                	}
                }

				count = m_priorityMusicsPlaying.Count;
				for( int i = m_priorityMusicsPlaying.Count - 1; i >= 0; --i )
                {
					if ( m_priorityMusicsPlaying[i].game_object == from )	
                	{
						m_priorityMusicsPlaying.RemoveAt(i);
                		break;
                	}
                }

                if ( m_priorityMusicsPlaying.Count > 0 )
                {
					Ambience_ToPlay = m_priorityMusicsPlaying[ m_priorityMusicsPlaying.Count - 1 ];
                }
				else if (m_musicsPlaying.Count > 0)
				{
					Ambience_ToPlay = m_musicsPlaying[ m_musicsPlaying.Count - 1 ];
				}
				else
				{
					Ambience_ToPlay.Reset();
				}
            }
            else if (categoryName == AMBIENCE_CATEGORY_SFX || categoryName == AMBIENCE_CATEGORY_SFX_2D )
            {
                AmbienceSfx_Stop(key, from);
            }
            else
            {
                LogError("Category <" + categoryName + ">  defined for key <" + key + "> can not be used for ambience");
            }
        }       
    }
    #endregion

    #region ambience_sfx
    // This region is responsible for playing ambience sound effect

    /// <summary>
    /// Dictionary containing all ambience sound effects being currently played
    /// </summary>
    private Dictionary<string, Dictionary<GameObject, AudioObject>> AmbienceSfx_Playing { get; set; }

    private void AmbienceSfx_Play(string key, GameObject from)
    {
        if (AmbienceSfx_Playing == null)
        {
            AmbienceSfx_Playing = new Dictionary<string, Dictionary<GameObject, AudioObject>>();
        }
        
        if (!AmbienceSfx_Playing.ContainsKey(key))
        {
            AmbienceSfx_Playing.Add(key, new Dictionary<GameObject, AudioObject>());
        }

        AudioObject audioObject = AudioController.PlayAmbienceSound(key);
        Dictionary<GameObject, AudioObject> sounds = AmbienceSfx_Playing[key];
        sounds.Add(from, audioObject);
    }

    private void AmbienceSfx_Stop(string key, GameObject from)
    {
        if (AmbienceSfx_Playing != null && AmbienceSfx_Playing.ContainsKey(key))
        {
            Dictionary<GameObject, AudioObject> sounds = AmbienceSfx_Playing[key];
            if (sounds != null && sounds.ContainsKey(from))
            {
                sounds[from].Stop();
                sounds.Remove(from);
            }
            else
            {
                LogError("No ambience sfx is playing from game object <" + from + ">");
            }
        }
        else 
        {
            LogError("No ambience sfx is playing for key <" + key + ">");
        }
    }
    #endregion

    #region snapshots
    protected AudioMixerSnapshot m_defaultAudioSnapshot;
    private List<AudioMixerSnapshot> m_currentSnapshots = new List<AudioMixerSnapshot>();
    private AudioMixerSnapshot m_usingSnapshot = null;
    private const float m_transitionDuration = 0.1f;

    public void RegisterSnapshot(AudioMixerSnapshot snapshot)
    {
        m_currentSnapshots.Add(snapshot);
        snapshot.TransitionTo(m_transitionDuration);
        m_usingSnapshot = snapshot;
    }

    public void UnregisterSnapshot(AudioMixerSnapshot snapshot)
    {
        for (int i = m_currentSnapshots.Count - 1; i >= 0; i--)
        {
            if (m_currentSnapshots[i] == snapshot)
            {
                m_currentSnapshots.RemoveAt(i);
                break;
            }
        }

        if (m_currentSnapshots.Count > 0)
        {
            if (m_usingSnapshot != m_currentSnapshots[m_currentSnapshots.Count - 1])
            {
                m_usingSnapshot = m_currentSnapshots[m_currentSnapshots.Count - 1];
                m_usingSnapshot.TransitionTo(m_transitionDuration);
            }
        }
        else
        {
            if (m_usingSnapshot != m_defaultAudioSnapshot)
            {
                m_usingSnapshot = m_defaultAudioSnapshot;
                m_usingSnapshot.TransitionTo(m_transitionDuration);
            }
        }
    }
    #endregion

    #region AudioObjectTools
    public void DelayedRemoveAudioParent( AudioObject ao)
    {
        m_delayedReturnObjects.Add(ao);
    }
    #endregion

    #region log
    private const string PREFIX = "MusicController:";

    public static void Log(string message)
    {
        Debug.Log(PREFIX + message);
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning(PREFIX + message);
    }

    public static void LogError(string message)
    {
        Debug.LogError(PREFIX + message);
    }
    #endregion
}

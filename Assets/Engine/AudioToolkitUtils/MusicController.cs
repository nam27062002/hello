using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour
{
    #region monobehaviour
    // Use this for initialization
    void Awake ()
    {        
        InstanceManager.musicController = this;

        Messenger.AddListener<string>(EngineEvents.SCENE_PREUNLOAD, OnScenePreunload);
        Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnGameLevelLoaded);

        Reset();        

        Music_Init();
	}

    void OnDestroy()
    {
        Messenger.RemoveListener<string>(EngineEvents.SCENE_PREUNLOAD, OnScenePreunload);
        Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnGameLevelLoaded);
        InstanceManager.musicController = null;
    }	        	

    void Update()
    {
        if (IsEnabled)
        {
            Music_Update();
        }
    }    

    private void Reset()
    {
        Music_StopCurrent();
        Music_CurrentKey = null;
        Ambience_KeyToPlay = null;        
        Music_OffsetAccummulated = 0f;
        Music_Lengths = null;

        // We don't want the music to start playing on the loading screen so we need to wait for the game to load completely before starting playing the music
        IsEnabled = false;
    }
    #endregion

    private bool IsEnabled { get; set; }

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
    #endregion

    #region music
    // This region is responsible for handling the music being played or the one to be played. For the sake of robustness and performance only one music is allowed to be played simultaneously

    public float m_musicFadeOut = 0.3f;

    public float m_musicVolume = 0.2f;

    /// <summary>
    /// Returns the key of the music that is the current music to play
    /// </summary>
    private string Music_CurrentKey { get; set; }    

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
        if (!string.IsNullOrEmpty(Music_CurrentKey))
        {
            AudioController.StopMusic(m_musicFadeOut);            
            Music_CurrentKey = null;
            Music_CurrentAudioObject = null;
        }
    }
    
    private float Music_OffsetAccummulated { get; set; }

    private void Music_Update()
    {
        // By default the main music has to be played, unless there's an ambience music
        string keyToPlay = m_mainMusicKey;
        if (Ambience_KeyToPlay != null)
        {
            keyToPlay = Ambience_KeyToPlay;
        }           
          
        if (keyToPlay != Music_CurrentKey)
        {
			if (Music_CurrentAudioObject != null)
			{
				if (Music_CurrentAudioObject.IsPaused(false))
				{
					Music_CurrentKey = keyToPlay;
					Music_CurrentAudioObject = AudioController.PlayMusic(Music_CurrentKey, m_musicVolume);
					AudioController.UnpauseMusic( m_musicFadeOut );	
				}
				else if ( !Music_CurrentAudioObject.IsPaused(true) )
				{
					AudioController.PauseMusic( m_musicFadeOut );
				}
			}
			else
			{
				Music_CurrentKey = keyToPlay;
				Music_CurrentAudioObject = AudioController.PlayMusic(Music_CurrentKey, m_musicVolume);
			}
        }
    }
    #endregion

    #region ambience
    // This region is responsible for playing ambience musics. An ambience music is the music that has to be played instead of the main music in some map areas such as the castle

	private const string AMBIENCE_CATEGORY_MUSIC = "MUSIC";
    private const string AMBIENCE_CATEGORY_SFX = "SFX";

    struct MusicPlaying
    {
    	public GameObject game_object;
    	public string music_key;
    	public MusicPlaying(string key, GameObject go )
    	{
    		music_key = key;
    		game_object = go;
    	}
    }
    List<MusicPlaying> m_musicsPlaying;
    private string Ambience_KeyToPlay { get; set; }

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
				if (m_musicsPlaying == null)
					m_musicsPlaying = new List<MusicPlaying>();
				m_musicsPlaying.Add( new MusicPlaying( key, from ) );
                Ambience_KeyToPlay = key;
            }
            else if (categoryName == AMBIENCE_CATEGORY_SFX)
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
				if (m_musicsPlaying.Count > 0)
				{
					Ambience_KeyToPlay = m_musicsPlaying[ m_musicsPlaying.Count - 1 ].music_key;
				}
				else
				{
					Ambience_KeyToPlay = null;
				}
				
            }
            else if (categoryName == AMBIENCE_CATEGORY_SFX)
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
    public AudioMixerSnapshot m_defaultAudioSnapshot;
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

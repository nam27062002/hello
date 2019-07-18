// WorldFeedbackSpawner.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Spawner in charge of showing all the feedback in the world scene
/// </summary>
public class WorldFeedbackSpawner : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[Separator("Feedback Prefabs")]
	[SerializeField] private GameObject m_scoreFeedbackPrefab = null;
	[SerializeField] private GameObject m_coinsFeedbackPrefab = null;
	[SerializeField] private GameObject m_pcFeedbackPrefab = null;
	[SerializeField] private GameObject m_killFeedbackPrefab = null;
	[SerializeField] private GameObject m_flockBonusFeedbackPrefab = null;
	[SerializeField] private GameObject m_escapedFeedbackPrefab = null;
    [SerializeField] private ParticlesTrailFX m_cakeEatenFeedbackPrefab = null;

    [Separator("Container References")]
	[SerializeField] private GameObject m_scoreFeedbackContainer = null;
	[SerializeField] private GameObject m_killFeedbackContainer = null;
	[SerializeField] private GameObject m_escapeFeedbackContainer = null;
	[SerializeField] private GameObject m_3dFeedbackContainer = null;
    [SerializeField] private Transform m_cakeCrumbsDestination = null;

    // Internal
    private Queue<WorldFeedbackController> m_feedbacksQueue = new Queue<WorldFeedbackController>();	// [AOC] In order to prevent too many feedbacks appearing at once, use a queue to show them sequentially

    public int m_scoreFeedbackMax = 15;
    public int m_coinsFeedbackMax = 5;
    public int m_killFeedbackMax = 5;
    public int m_flockBonusFeedbackMax = 2;
    public int m_escapedFeedbackMax = 2;

	private PoolHandler m_scoreFeedbackPoolHandler;
	private PoolHandler m_coinsFeedbackPoolHandler;
	private PoolHandler m_pcFeedbackPoolHandler;
	private PoolHandler m_killFeedbackPoolHandler;
	private PoolHandler m_flockBonusFeedbackPoolHandler;
	private PoolHandler m_escapedFeedbackPoolHandler;
    private PoolHandler m_cakeFeedbackPoolHandler;

    private bool m_particlesFeedbackEnabled = false;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//      	
    private void Awake() {
		m_particlesFeedbackEnabled = FeatureSettingsManager.instance.IsParticlesFeedbackEnabled;
    }

    private void Start() {
		// Create the pools
		// No more than X simultaneous messages on screen!
		// Use container if defined to keep hierarchy clean

		if ( m_particlesFeedbackEnabled ){
			// Score
			if(m_scoreFeedbackPrefab != null) {
				// Must be created within the canvas
				Transform parent = this.transform;
				if(m_scoreFeedbackContainer != null) {
					parent = m_scoreFeedbackContainer.transform;
				}
				m_scoreFeedbackPoolHandler = UIPoolManager.CreatePool(m_scoreFeedbackPrefab, parent, m_scoreFeedbackMax, true, false);
			}

			// Coins
			if(m_coinsFeedbackPrefab != null) {
				m_coinsFeedbackPoolHandler = UIPoolManager.CreatePool(m_coinsFeedbackPrefab, m_coinsFeedbackMax, true, false);
			}

			// Kill Feedback
			if(m_killFeedbackPrefab != null) {
				Transform parent = this.transform;
				if(m_killFeedbackContainer != null) {
					parent = m_killFeedbackContainer.transform;
				}
				m_killFeedbackPoolHandler = UIPoolManager.CreatePool(m_killFeedbackPrefab, parent, m_killFeedbackMax, false, false);
			}
				
			// Flock Bonus
			if(m_flockBonusFeedbackPrefab != null) { 
				Transform parent = this.transform;
				if(m_scoreFeedbackContainer != null) {
					parent = m_scoreFeedbackContainer.transform;
				}
				m_flockBonusFeedbackPoolHandler = UIPoolManager.CreatePool(m_flockBonusFeedbackPrefab, parent, m_flockBonusFeedbackMax, true, false);
			}

			// Escape
			if ( m_escapedFeedbackPrefab != null )
			{
				Transform parent = this.transform;
				if(m_escapeFeedbackContainer != null) {
					parent = m_escapeFeedbackContainer.transform;
				}
				m_escapedFeedbackPoolHandler = UIPoolManager.CreatePool(m_escapedFeedbackPrefab, parent, m_escapedFeedbackMax, false, false);
			}
		}
		// PC
		if(m_pcFeedbackPrefab != null) {
			// Use a dedicated camera as parent, that way the feedback will be positioned relative to the viewport
			m_pcFeedbackPoolHandler = UIPoolManager.CreatePool(m_pcFeedbackPrefab, m_3dFeedbackContainer.transform, 2, false, false);
		}


        // Start with the 3D feedback container disabled - will be enabled on demand
        m_3dFeedbackContainer.SetActive(false);

		if ( m_particlesFeedbackEnabled )
        	Cache_Init();
        Offsets_Init();

        if (FeatureSettingsManager.IsDebugEnabled)
            Debug_Awake();
    }

    /// <summary>
    /// The spawner has been enabled.
    /// </summary>
    private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.AddListener(MessengerEvents.UI_INGAME_PC_FEEDBACK_END, OnPCFeedbackEnd);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);

		if ( m_particlesFeedbackEnabled )
		{
			Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_EATEN, OnEaten);
			Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, OnBurned);
			Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_DESTROYED, OnDestroyed);
			Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.FLOCK_EATEN, OnFlockEaten);
			Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.STAR_COMBO, OnStarCombo);
            Messenger.AddListener<Vector3>(MessengerEvents.ANNIVERSARY_CAKE_SLICE_EATEN, OnEatCake);

            Messenger.AddListener<Transform>(MessengerEvents.ENTITY_ESCAPED, OnEscaped);
	        
        }
		
    }
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.RemoveListener(MessengerEvents.UI_INGAME_PC_FEEDBACK_END, OnPCFeedbackEnd);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);

		if ( m_particlesFeedbackEnabled )
		{
			Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_EATEN, OnEaten);
			Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, OnBurned);
			Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_DESTROYED, OnDestroyed);
			Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.FLOCK_EATEN, OnFlockEaten);
			Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.STAR_COMBO, OnStarCombo);
            Messenger.RemoveListener<Vector3>(MessengerEvents.ANNIVERSARY_CAKE_SLICE_EATEN, OnEatCake);
            Messenger.RemoveListener<Transform>(MessengerEvents.ENTITY_ESCAPED, OnEscaped);
        }
		
    }
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
        }
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
        if (ApplicationManager.IsAlive && FeatureSettingsManager.IsDebugEnabled)
            Debug_OnDestroy();

        Clear();
    }

    public void Clear()
    {
        Cache_Clear();
        Offsets_Clear();
    }

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Trigger feedbacks one at a time
		if(m_feedbacksQueue.Count > 0) {
			WorldFeedbackController fb = m_feedbacksQueue.Dequeue();
			fb.Spawn();
		}
	}    

    //------------------------------------------------------------------//
    // INTERNAL															//
    //------------------------------------------------------------------//
    /// <summary>
    /// Spawn a kill message feedback with the given text and entity.
    /// </summary>
    /// <param name="_type">The type of feedback to be displayed.</param>
    /// <param name="_entity">The source of the kill.</param>
    private void SpawnKillFeedback(FeedbackData.Type _type, Transform _t, IEntity _e) {
		// Some checks first
		if(_t == null) return;

        // Get the feedback data from the source entity
        Entity entity = _e as Entity;
		if(entity == null) return;

        // Check that there's actually some text to be spawned
        string text = entity.feedbackData.GetFeedback(_type);        
        if (string.IsNullOrEmpty(text)) return;

        // Get an instance from the pool and spawn it!
        TextCacheItemData itemData = m_cacheDatas[ECacheTypes.Kill].GetCacheItemDataAvailable() as TextCacheItemData;
        if (itemData != null)
        {
            itemData.SpawnText(CacheWatch.ElapsedMilliseconds, _t.position, text);            
            m_feedbacksQueue.Enqueue(itemData.Controller);
        }       
    }

	private void SpawnEscapedFeedback( Transform _entity)
	{
		string tid = "TID_FEEDBACK_ESCAPED";
		switch( UnityEngine.Random.Range(0,3))
		{
			case 0: tid = "TID_FEEDBACK_ESCAPED";break;
			case 1: tid = "TID_FEEDBACK_SO_CLOSE";break;
			case 2: tid = "TID_FEEDBACK_GOT_AWAY";break;
		}

        // Get an instance from the pool and spawn it!
        TextCacheItemData itemData = m_cacheDatas[ECacheTypes.Escaped].GetCacheItemDataAvailable() as TextCacheItemData;
        if (itemData != null)
        {
            itemData.SpawnText(CacheWatch.ElapsedMilliseconds, _entity.position, LocalizationManager.SharedInstance.Get(tid));
            m_feedbacksQueue.Enqueue(itemData.Controller);
        }        
	}
	
	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A reward has been applied, show feedback for it.
	/// </summary>
	/// <param name="_reward">The reward that has been applied.</param>
	/// <param name="_entity">The entity that triggered the reward. Can be null.</param>
	private void OnRewardApplied(Reward _reward, Transform _entity) {

		if ( m_particlesFeedbackEnabled )
		{
			// Find out spawn position
			Vector3 worldPos = Vector3.zero;
	        if (_entity != null) {
	            // A random offset is added in order to prevent several particles from being located at the same position
	            worldPos = _entity.position + Offsets_GetRandomOffset();            
	        }        

	        // Show different feedback for different reward types
	        // Score
	        if (_reward.score > 0) {
				// Get a score feedback instance and initialize it with the target reward
				if(m_scoreFeedbackPrefab != null) {               
	                ScoreCacheItemData itemData = m_cacheDatas[ECacheTypes.Score].GetCacheItemDataAvailable() as ScoreCacheItemData;
	                if (itemData != null)
	                {
	                    itemData.Spawn(CacheWatch.ElapsedMilliseconds, worldPos, Mathf.FloorToInt(_reward.score));
	                    m_feedbacksQueue.Enqueue(itemData.Controller);
	                }
				}
			}

			// Coins
			if(m_coinsFeedbackPrefab != null && _reward.coins > 0 && ((_reward.origin != null && _reward.origin.CompareTo("GoodJunkCoin") != 0 && _reward.origin.CompareTo("letter") != 0) || _reward.origin == null )) {	// if its no coin
	            CacheItemData itemData = m_cacheDatas[ECacheTypes.Coins].GetCacheItemDataAvailable();
	            if (itemData != null)
	            {
					itemData.Spawn(CacheWatch.ElapsedMilliseconds, worldPos, Mathf.FloorToInt(_reward.coins));
	            }
			}
		}

		// PC
		if(m_pcFeedbackPrefab != null && _reward.pc > 0) {
			m_pcFeedbackPoolHandler.pool.containerObj.SetActive(true);
			m_pcFeedbackPoolHandler.GetInstance();
		}
	}

	/// <summary>
	/// An entity has been eaten.
	/// </summary>
	/// <param name="_entity">The eaten entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnEaten(Transform _t, IEntity _e, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.EAT, _t, _e);
	}

	/// <summary>
	/// An entity has been burned.
	/// </summary>
	/// <param name="_entity">The burned entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnBurned(Transform _t, IEntity _e, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.BURN, _t, _e);
	}

	/// <summary>
	/// An entity has been destroyed.
	/// </summary>
	/// <param name="_entity">The destroyed entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnDestroyed(Transform _t, IEntity _e, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.DESTROY, _t, _e);
	}
		
	/// <summary>
	/// A full flock has been eaten.
	/// </summary>
	/// <param name="_entity">Entity.</param>
	/// <param name="_reawrd">Reawrd.</param>
	private void OnFlockEaten(Transform _t, IEntity _e, Reward _reward) {		
		// Spawn flock feedback bonus, score will be displayed as any other score feedback		
        string text = LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_FLOCK_BONUS");

        // Get an instance from the pool and spawn it!
        TextCacheItemData itemData = m_cacheDatas[ECacheTypes.FlockBonus].GetCacheItemDataAvailable() as TextCacheItemData;
        if (itemData != null)
        {
            itemData.SpawnText(CacheWatch.ElapsedMilliseconds, _t.position, text);
            m_feedbacksQueue.Enqueue(itemData.Controller);
        }           		
	}

	/// <summary>
	/// A full group of stars has been eaten.
	/// </summary>
	/// <param name="_entity">Entity.</param>
	/// <param name="_reawrd">Reawrd.</param>
	private void OnStarCombo(Transform _t, IEntity _e, Reward _reward) {		
		// Spawn flock feedback bonus, score will be displayed as any other score feedback		
		string text = LocalizationManager.SharedInstance.Localize("TID_STARS_REWARD");

		// Get an instance from the pool and spawn it!
		TextCacheItemData itemData = m_cacheDatas[ECacheTypes.FlockBonus].GetCacheItemDataAvailable() as TextCacheItemData;
		if (itemData != null)
		{
			itemData.SpawnText(CacheWatch.ElapsedMilliseconds, _t.position, text);
			m_feedbacksQueue.Enqueue(itemData.Controller);
		}           		
	}

    /// <summary>
    /// A piece of cake has been eaten.
    /// </summary>
    /// <param name="_position">The cake pieces position.</param>
    private void OnEatCake(Vector3 _position)
    {
        if (m_cakeEatenFeedbackPrefab != null)
        {

            // From the center of the screen
            Vector3 source = transform.position;

            // To the cake icon
            Vector3 sink = m_cakeCrumbsDestination.position;

            ParticlesTrailFX.InstantiateAndLaunch(m_cakeEatenFeedbackPrefab.gameObject, transform, source, sink);

        }
    }   


    private void OnEscaped(Transform _entity) {
		SpawnEscapedFeedback(_entity);
	}

    private void OnGameEnded() {
        Clear();
    }

	/// <summary>
	/// 3D PC Feedback ended.
	/// </summary>
	private void OnPCFeedbackEnd() {
		// Disable pool container for better performance
		m_pcFeedbackPoolHandler.pool.containerObj.SetActive(false);
	}

    #region offsets
    // This region is responsible for storing some offsets to be used to spawn feedback particles, typically when an entity is eaten. Every time an entity is eaten a particle
    // has to be spawned at the eater's mouth position plus an offset in order to prevent all particles from being spawned at the same position
    private int OFFSETS_MAX = 40;
    private Vector3[] m_offsets;

    private void Offsets_Init() {
        if (m_offsets == null) {
            Offsets_CreateOffsets(1.6f, OFFSETS_MAX);
        }
    }

    private void Offsets_Clear() {
        m_offsets = null;             
    }

    private void Offsets_CreateOffsets(float _radius, int _numPoints) {
        m_offsets = new Vector3[_numPoints];
        Vector2 pos;
        for (int i = 0; i < _numPoints; i++) {
            pos = UnityEngine.Random.insideUnitCircle * _radius;
            m_offsets[i] = new Vector3(pos.x, pos.y, 0f);
        }
    }

    private Vector3 Offsets_GetRandomOffset() {
        int _index = (int)UnityEngine.Random.Range(0, OFFSETS_MAX - 1);        
        return m_offsets[_index];
    }
    #endregion

    #region cache
    // This region is responsible for storing the game object to be used for showing feedback.

    private System.Diagnostics.Stopwatch CacheWatch;

    private enum ECacheTypes {
        Score,
        Coins,
        Kill,
        FlockBonus,
        Escaped
    };

    private abstract class CacheData
    {
        private CacheItemData[] CacheItemDatas;
		private PoolHandler m_poolHandler;

		public CacheData(int itemsAmount, PoolHandler _poolHandler) {
            CacheItemDatas = new CacheItemData[itemsAmount];
			m_poolHandler = _poolHandler;

            // Instantiate all game objects in order to make sure they already exist when they have to be spawned
            GameObject go;
            for (int i = 0; i < itemsAmount; i++) {				
				go = m_poolHandler.GetInstance(false);
                CacheItemDatas[i] = CreateItemData(0, go);
            }            
        }

        protected abstract CacheItemData CreateItemData(long timeStamp, GameObject go);
        
        public void Clear() {
            if (CacheItemDatas != null) {
                if (PoolManager.instance != null) {
                    int count = CacheItemDatas.Length;
                    for (int i = 0; i < count; i++) {
                        if (CacheItemDatas[i].GameObject != null) {							
							m_poolHandler.ReturnInstance(CacheItemDatas[i].GameObject);
                        }
                    }
                }

                CacheItemDatas = null;
            }
        }

        public CacheItemData GetCacheItemDataAvailable() {
            // Returns the oldest item
            int index = 0;
            int count = CacheItemDatas.Length;
            long minTimeStamp = long.MaxValue;
            for (int i = 0; i < count; i++) {
                if (CacheItemDatas[i].TimeStamp < minTimeStamp) {
                    index = i;
                    minTimeStamp = CacheItemDatas[i].TimeStamp;
                }
            }

            return CacheItemDatas[index];
        }                       
    }

    private class ScoreCacheData : CacheData {
		public ScoreCacheData(int itemsAmount, PoolHandler _poolHandler) : base(itemsAmount, _poolHandler) {
        }

        protected override CacheItemData CreateItemData(long timeStamp, GameObject go)
        {
            return new ScoreCacheItemData(timeStamp, go);
        }
    }

    private class CoinsCacheData : CacheData {
		public CoinsCacheData(int itemsAmount, PoolHandler _poolHandler) : base(itemsAmount, _poolHandler) {}

        protected override CacheItemData CreateItemData(long timeStamp, GameObject go) {
            return new CoinsCacheItemData(timeStamp, go);
        }
    }

    private class TextCacheData : CacheData
    {
		public TextCacheData(int itemsAmount, PoolHandler _poolHandler) : base(itemsAmount, _poolHandler) { }

        protected override CacheItemData CreateItemData(long timeStamp, GameObject go)
        {
            return new TextCacheItemData(timeStamp, go);
        }
    }

    private class CacheItemData {
        /// <summary>
        /// Time stamp of the last time this object was used 
        /// </summary>
        public long TimeStamp { get; set; }
        public GameObject GameObject { get; set; }

        public CacheItemData(long timeStamp, GameObject go) {
            TimeStamp = timeStamp;
            GameObject = go;
        }

        public void Spawn(long timeStamp, Vector3 worldPos, int value) {
            TimeStamp = timeStamp;
            SpawnExtended(worldPos, value);
        }

        public void SpawnText(long timeStamp, Vector3 worldPos,  string text) {
            TimeStamp = timeStamp;
            SpawnTextExtended(worldPos, text);
        }

        protected virtual void SpawnExtended(Vector3 worldPos, int value) {}
        protected virtual void SpawnTextExtended(Vector3 worldPos, string text) {}
    }

    private class ScoreCacheItemData : CacheItemData
    {
        public ScoreFeedback ScoreFeedback { get; set; }
        public WorldFeedbackController Controller { get; set; }

        public ScoreCacheItemData(long timeStamp, GameObject go) : base(timeStamp, go)
        {            
            ScoreFeedback = go.GetComponent<ScoreFeedback>();                        
            Controller = go.GetComponent<WorldFeedbackController>();            
        }

        protected override void SpawnExtended(Vector3 worldPos, int value) 
        {
            if (ScoreFeedback != null)
            {
                ScoreFeedback.SetScore(value);
            }

            if (Controller != null)
            {
                Controller.Init(worldPos);
            }
        }
    }

    private class CoinsCacheItemData : CacheItemData {
        public CoinsFeedbackController Controller { get; set; }

        public CoinsCacheItemData(long timeStamp, GameObject go) : base(timeStamp, go) {            
            Controller = go.GetComponent<CoinsFeedbackController>();            
        }

        protected override void SpawnExtended(Vector3 worldPos, int value) {
            if (Controller != null) {
                Controller.Launch(worldPos, value);                
            }           
        }
    }

    private class TextCacheItemData : CacheItemData
    {        
        public WorldFeedbackController Controller { get; set; }

        public TextCacheItemData(long timeStamp, GameObject go) : base(timeStamp, go)
        {            
            Controller = go.GetComponent<WorldFeedbackController>();
        }

        protected override void SpawnTextExtended(Vector3 worldPos, string text)        
        {           
            if (Controller != null)
            {
                Controller.Init(text, worldPos);                
            }
        }
    }

    private Dictionary<ECacheTypes, CacheData> m_cacheDatas;
    
    private void Cache_Init() {
        CacheWatch = new System.Diagnostics.Stopwatch();
        CacheWatch.Start();

        if (m_cacheDatas == null) {
            m_cacheDatas = new Dictionary<ECacheTypes, CacheData>();
            
            // Score cache data
			CacheData cacheData = new ScoreCacheData(m_scoreFeedbackMax, m_scoreFeedbackPoolHandler);
            m_cacheDatas.Add(ECacheTypes.Score, cacheData);

            // Coins cache data
			cacheData = new CoinsCacheData(m_coinsFeedbackMax, m_coinsFeedbackPoolHandler);
            m_cacheDatas.Add(ECacheTypes.Coins, cacheData);

            // Kill feedback cache data
			cacheData = new TextCacheData(m_killFeedbackMax, m_killFeedbackPoolHandler);
            m_cacheDatas.Add(ECacheTypes.Kill, cacheData);

            // Flock bonus cache data
            cacheData = new TextCacheData(m_flockBonusFeedbackMax, m_flockBonusFeedbackPoolHandler);
            m_cacheDatas.Add(ECacheTypes.FlockBonus, cacheData);            

            // Escaped feedback cache data
            cacheData = new TextCacheData(m_escapedFeedbackMax, m_escapedFeedbackPoolHandler);
            m_cacheDatas.Add(ECacheTypes.Escaped, cacheData);            
        }
    }

    private void Cache_Clear() {
        if (m_cacheDatas != null) {
            foreach (KeyValuePair<ECacheTypes, CacheData> pair in m_cacheDatas) {
                pair.Value.Clear();
            }

            m_cacheDatas = null;
        }
    }
    #endregion

    #region debug
    // This region is responsible for enabling/disabling the feedback particles for profiling purposes. 
    private void Debug_Awake() {
        Messenger.AddListener<string>(MessengerEvents.CP_PREF_CHANGED, Debug_OnChanged);

        // Enable/Disable object depending on the flag
        Debug_SetActive();
    }

    private void Debug_OnDestroy() {
        Messenger.RemoveListener<string>(MessengerEvents.CP_PREF_CHANGED, Debug_OnChanged);
    }

    private void Debug_OnChanged(string _id) {
        if (_id == DebugSettings.INGAME_PARTICLES_FEEDBACK) {
            // Enable/Disable object
            Debug_SetActive();
        }
    }

    private void Debug_SetActive() {
        enabled = DebugSettings.ingameParticlesFeedback;        
    }
    #endregion
}
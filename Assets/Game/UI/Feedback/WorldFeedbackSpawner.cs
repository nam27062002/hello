﻿// WorldFeedbackSpawner.cs
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
public class WorldFeedbackSpawner : MonoBehaviour {
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

	[Separator("Container References")]
	[SerializeField] private GameObject m_scoreFeedbackContainer = null;
	[SerializeField] private GameObject m_killFeedbackContainer = null;
	[SerializeField] private GameObject m_escapeFeedbackContainer = null;
	[SerializeField] private GameObject m_3dFeedbackContainer = null;

	// Internal
	private Queue<WorldFeedbackController> m_feedbacksQueue = new Queue<WorldFeedbackController>();	// [AOC] In order to prevent too many feedbacks appearing at once, use a queue to show them sequentially

    public int m_scoreFeedbackMax = 15;
    public int m_coinsFeedbackMax = 5;
    public int m_killFeedbackMax = 5;
    public int m_flockBonusFeedbackMax = 2;
    public int m_escapedFeedbackMax = 2;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//      	
    private void Start() {
		// Create the pools
		// No more than X simultaneous messages on screen!
		// Use container if defined to keep hierarchy clean

		// Score
		if(m_scoreFeedbackPrefab != null) {
			// Must be created within the canvas
			Transform parent = this.transform;
			if(m_scoreFeedbackContainer != null) {
				parent = m_scoreFeedbackContainer.transform;
			}
			PoolManager.CreatePool(m_scoreFeedbackPrefab, parent, m_scoreFeedbackMax, true, false);
		}

		// Coins
		if(m_coinsFeedbackPrefab != null) {
			PoolManager.CreatePool(m_coinsFeedbackPrefab, m_coinsFeedbackMax, true, false);
		}

		// PC
		if(m_pcFeedbackPrefab != null) {
			// Use a dedicated camera as parent, that way the feedback will be positioned relative to the viewport
			PoolManager.CreatePool(m_pcFeedbackPrefab, m_3dFeedbackContainer.transform, 2, false, false);
		}

		// Kill Feedback
		if(m_killFeedbackPrefab != null) {
			Transform parent = this.transform;
			if(m_killFeedbackContainer != null) {
				parent = m_killFeedbackContainer.transform;
			}
			PoolManager.CreatePool(m_killFeedbackPrefab, parent, m_killFeedbackMax, false, false);
		}
			
		// Flock Bonus
		if(m_flockBonusFeedbackPrefab != null) { 
			Transform parent = this.transform;
			if(m_scoreFeedbackContainer != null) {
				parent = m_scoreFeedbackContainer.transform;
			}
			PoolManager.CreatePool(m_flockBonusFeedbackPrefab, parent, m_flockBonusFeedbackMax, true, false);
		}

		// Escape
		if ( m_escapedFeedbackPrefab != null )
		{
			Transform parent = this.transform;
			if(m_escapeFeedbackContainer != null) {
				parent = m_escapeFeedbackContainer.transform;
			}
			PoolManager.CreatePool(m_escapedFeedbackPrefab, parent, m_escapedFeedbackMax, false, false);
		}

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
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnEaten);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnBurned);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnDestroyed);
		Messenger.AddListener<Transform, Reward>(GameEvents.FLOCK_EATEN, OnFlockEaten);
		Messenger.AddListener<Transform>(GameEvents.ENTITY_ESCAPED, OnEscaped);
        Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);        
    }
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnEaten);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnBurned);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnDestroyed);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.FLOCK_EATEN, OnFlockEaten);
		Messenger.RemoveListener<Transform>(GameEvents.ENTITY_ESCAPED, OnEscaped);
        Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
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
    private void SpawnKillFeedback(FeedbackData.Type _type, Transform _entity) {
		// Some checks first
		if(_entity == null) return;

		// Get the feedback data from the source entity
		Entity entity = _entity.GetComponent<Entity>();
		if(entity == null) return;

        // Check that there's actually some text to be spawned
        string text = entity.feedbackData.GetFeedback(_type);        
        if (string.IsNullOrEmpty(text)) return;

        // Get an instance from the pool and spawn it!
        TextCacheItemData itemData = m_cacheDatas[ECacheTypes.Kill].GetCacheItemDataAvailable() as TextCacheItemData;
        if (itemData != null)
        {
            itemData.SpawnText(CacheWatch.ElapsedMilliseconds, _entity.position, text);            
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
                    itemData.Spawn(CacheWatch.ElapsedMilliseconds, worldPos, _reward.score);
                    m_feedbacksQueue.Enqueue(itemData.Controller);
                }
			}
		}

		// Coins
		if(m_coinsFeedbackPrefab != null && _reward.coins > 0) {	// if its no coin
			if ( _entity == null || _entity.GetComponent<Entity>().sku != "GoodJunkCoin" )	//if already a Coin we don't show feedback
			{
	            CacheItemData itemData = m_cacheDatas[ECacheTypes.Coins].GetCacheItemDataAvailable();
	            if (itemData != null)
	            {
	                itemData.Spawn(CacheWatch.ElapsedMilliseconds, worldPos, _reward.score);                
	            }
            }           
		}

		// PC
		if(m_pcFeedbackPrefab != null && _reward.pc > 0) {
			GameObject obj = PoolManager.GetInstance(m_pcFeedbackPrefab.name);
			obj.SetActive(true);
		}
	}

	/// <summary>
	/// An entity has been eaten.
	/// </summary>
	/// <param name="_entity">The eaten entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnEaten(Transform _entity, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.EAT, _entity);
	}

	/// <summary>
	/// An entity has been burned.
	/// </summary>
	/// <param name="_entity">The burned entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnBurned(Transform _entity, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.BURN, _entity);
	}

	/// <summary>
	/// An entity has been destroyed.
	/// </summary>
	/// <param name="_entity">The destroyed entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnDestroyed(Transform _entity, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.DESTROY, _entity);
	}
		
	/// <summary>
	/// A full flock has been eaten.
	/// </summary>
	/// <param name="_entity">Entity.</param>
	/// <param name="_reawrd">Reawrd.</param>
	private void OnFlockEaten(Transform _entity, Reward _reward) {		
		// Spawn flock feedback bonus, score will be displayed as any other score feedback		
        string text = LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_FLOCK_BONUS");

        // Get an instance from the pool and spawn it!
        TextCacheItemData itemData = m_cacheDatas[ECacheTypes.FlockBonus].GetCacheItemDataAvailable() as TextCacheItemData;
        if (itemData != null)
        {
            itemData.SpawnText(CacheWatch.ElapsedMilliseconds, _entity.position, text);
            m_feedbacksQueue.Enqueue(itemData.Controller);
        }           		
	}


	private void OnEscaped(Transform _entity) {
		SpawnEscapedFeedback(_entity);
	}

    private void OnGameEnded() {
        Clear();
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

        public CacheData(int itemsAmount, string prefabName) {
            CacheItemDatas = new CacheItemData[itemsAmount];

            // Instantiate all game objects in order to make sure they already exist when they have to be spawned
            GameObject go;
            for (int i = 0; i < itemsAmount; i++) {
                go = PoolManager.GetInstance(prefabName, false);
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
                            PoolManager.ReturnInstance(CacheItemDatas[i].GameObject);
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
        public ScoreCacheData(int itemsAmount, string prefabName) : base(itemsAmount, prefabName) {
        }

        protected override CacheItemData CreateItemData(long timeStamp, GameObject go)
        {
            return new ScoreCacheItemData(timeStamp, go);
        }
    }

    private class CoinsCacheData : CacheData {
        public CoinsCacheData(int itemsAmount, string prefabName) : base(itemsAmount, prefabName) {}

        protected override CacheItemData CreateItemData(long timeStamp, GameObject go) {
            return new CoinsCacheItemData(timeStamp, go);
        }
    }

    private class TextCacheData : CacheData
    {
        public TextCacheData(int itemsAmount, string prefabName) : base(itemsAmount, prefabName) { }

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
            CacheData cacheData = new ScoreCacheData(m_scoreFeedbackMax, m_scoreFeedbackPrefab.name);
            m_cacheDatas.Add(ECacheTypes.Score, cacheData);

            // Coins cache data
            cacheData = new CoinsCacheData(m_coinsFeedbackMax, m_coinsFeedbackPrefab.name);
            m_cacheDatas.Add(ECacheTypes.Coins, cacheData);

            // Kill feedback cache data
            cacheData = new TextCacheData(m_killFeedbackMax, m_killFeedbackPrefab.name);
            m_cacheDatas.Add(ECacheTypes.Kill, cacheData);

            // Flock bonus cache data
            cacheData = new TextCacheData(m_flockBonusFeedbackMax, m_flockBonusFeedbackPrefab.name);
            m_cacheDatas.Add(ECacheTypes.FlockBonus, cacheData);            

            // Escaped feedback cache data
            cacheData = new TextCacheData(m_escapedFeedbackMax, m_escapedFeedbackPrefab.name);
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
        Messenger.AddListener<string>(GameEvents.CP_PREF_CHANGED, Debug_OnChanged);

        // Enable/Disable object depending on the flag
        Debug_SetActive();
    }

    private void Debug_OnDestroy() {
        Messenger.RemoveListener<string>(GameEvents.CP_PREF_CHANGED, Debug_OnChanged);
    }

    private void Debug_OnChanged(string _id) {
        if (_id == DebugSettings.INGAME_PARTICLES_FEEDBACK) {
            // Enable/Disable object
            Debug_SetActive();
        }
    }

    private void Debug_SetActive() {
        enabled = Prefs.GetBoolPlayer(DebugSettings.INGAME_PARTICLES_FEEDBACK, true);        
    }
    #endregion
}
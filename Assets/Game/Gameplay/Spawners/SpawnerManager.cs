// SpawnerManager.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach, Alger Ortín Castellví
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Singleton to manage all the spawners in a level in an efficient way.
/// </summary>
public class SpawnerManager : UbiBCN.SingletonMonoBehaviour<SpawnerManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_INTERVAL = 0.2f;	// Seconds, avoid updating all the spawners all the time for better performance
	private const float FAR_LAYER_Z = 8f;
	public const float BACKGROUND_LAYER_Z = 60f;
    public const float SPAWNING_MAX_TIME = 4f; // Max time (in milliseconds) allowed to spend on spawning entities
    
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Spawners collection
	private List<ISpawner> m_spawners = null;
	private QuadTree<ISpawner> m_spawnersTreeNear = null;
	private QuadTree<ISpawner> m_spawnersTreeFar = null;
	private QuadTree<ISpawner> m_spawnersTreeBG = null;

	// Internal logic
	private bool m_enabled = false;
	private float m_updateTimer = 0f;

	// External references
	private GameCamera m_newCamera = null;
    private Transform m_newCameraTransform;

	// Detection area
	private FastBounds2D m_minRectNear = null;	// From the game camera
	private FastBounds2D m_maxRectNear = null;
	private FastBounds2D m_minRectFar = null;	// From the game camera
	private FastBounds2D m_maxRectFar = null;
	private FastBounds2D m_minRectBG = null;	// From the game camera
	private FastBounds2D m_maxRectBG = null;

	private Rect[] m_subRect = new Rect[4];
	private HashSet<ISpawner> m_selectedSpawners = new HashSet<ISpawner>();
    
    public List<ISpawner> m_spawning;
	private List<ISpawner> m_activeMustCheckCameraBounds;

    private float m_lastX, m_lastY;

    private System.Diagnostics.Stopwatch m_watch = new System.Diagnostics.Stopwatch();

    private Dictionary<int, AbstractSpawnerData> m_spanwersData = new Dictionary<int, AbstractSpawnerData>();

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Inititalization.
    /// </summary>
    private void Awake() {
		m_spawners = new List<ISpawner>();
		m_spawning = new List<ISpawner>();
		m_activeMustCheckCameraBounds = new List<ISpawner>();

        if (FeatureSettingsManager.IsDebugEnabled)
            Debug_Awake();
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        if (ApplicationManager.IsAlive && FeatureSettingsManager.IsDebugEnabled)
            Debug_OnDestroy();
    }

    /// <summary>
    /// Component enabled.
    /// </summary>
    private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.AddListener(GameEvents.GAME_AREA_ENTER, OnAreaEnter);
		Messenger.AddListener(GameEvents.GAME_AREA_EXIT, OnAreaExit);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.RemoveListener(GameEvents.GAME_AREA_ENTER, OnAreaEnter);
		Messenger.RemoveListener(GameEvents.GAME_AREA_EXIT, OnAreaExit);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
	}        

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Only if enabled!
		if(!m_enabled) return;
		if(m_spawnersTreeNear == null) return;

		// Get activation bounds
		// Update every frame in case camera bounds change (i.e. zoom in/out)
		float currentX = 0, currentY = 0;
		if (m_newCamera != null) {
			m_minRectNear = m_newCamera.activationMinRectNear;
			m_maxRectNear = m_newCamera.activationMaxRectNear;

			m_minRectFar = m_newCamera.activationMinRectFar;
			m_maxRectFar = m_newCamera.activationMaxRectFar;

			m_minRectBG = m_newCamera.activationMinRectBG;
			m_maxRectBG = m_newCamera.activationMaxRectBG;

			currentX = m_newCamera.position.x;
			currentY = m_newCamera.position.y;
		}

		const float delta = 0.5f;
		bool checkBottom = (currentY - m_lastY) < -delta;
		bool checkRight = (currentX - m_lastX) > delta;
		bool checkTop = (currentY - m_lastY) > delta;
		bool checkLeft = (currentX - m_lastX) < -delta;

		// Update timer (optimization to avoid performing the update every frame)
		// Pre-optimization: ~10% CPU usage, 36-39 FPS
		// 0.2s: 37-40 FPS
		m_updateTimer += Time.deltaTime;
		if(m_updateTimer > UPDATE_INTERVAL) {
			// Reset timer
			m_updateTimer = 0f;            

			// Only update those spawners closer to the dragon
			// Use the camera detection area for that
			// Since the detection area has a hole in it, divide it in 4 rectangle sub-areas

			//            min.x0                  min.x1
			//              |                       |
			//           _______________________________  _ max.y1
			//          |               2               |
			// min.y1 _ |_______________________________|
			//          |   |                       |   |
			//          |   |                       |   |
			//          | 3 |                       | 1 |
			//          |   |                       |   |
			// min.y0 _ |___|_______________________|___|
			//          |               0               |
			//          |_______________________________| _ max.y0
			//
			//          |                               |
			//        max.x0                         max.x1
			//
			m_selectedSpawners.Clear();			           

			if (checkBottom) {
				// 1: bottom sub-rect
				CheckBottom(ref m_spawnersTreeBG, ref m_minRectBG, ref m_maxRectBG);
				CheckBottom(ref m_spawnersTreeFar, ref m_minRectFar, ref m_maxRectFar);
				CheckBottom(ref m_spawnersTreeNear, ref m_minRectNear, ref m_maxRectNear);

                m_lastY = currentY;
                //Debug.LogError("BOTTOM");
            }
			if (checkRight) {
                // 2: right sub-rect
				CheckRight(ref m_spawnersTreeBG, ref m_minRectBG, ref m_maxRectBG);
				CheckRight(ref m_spawnersTreeFar, ref m_minRectFar, ref m_maxRectFar);
				CheckRight(ref m_spawnersTreeNear, ref m_minRectNear, ref m_maxRectNear);

				m_lastX = currentX;
                //Debug.LogError("RIGHT");
            }
			if (checkTop) {
                // 3: top sub-rect
				CheckTop(ref m_spawnersTreeBG, ref m_minRectBG, ref m_maxRectBG);
				CheckTop(ref m_spawnersTreeFar, ref m_minRectFar, ref m_maxRectFar);
				CheckTop(ref m_spawnersTreeNear, ref m_minRectNear, ref m_maxRectNear);

                m_lastY = currentY;
                //Debug.LogError("TOP");
            }
			if (checkLeft) {
                // 4: left sub-rect
				CheckLeft(ref m_spawnersTreeBG, ref m_minRectBG, ref m_maxRectBG);
				CheckLeft(ref m_spawnersTreeFar, ref m_minRectFar, ref m_maxRectFar);
				CheckLeft(ref m_spawnersTreeNear, ref m_minRectNear, ref m_maxRectNear);

                m_lastX = currentX;
                //Debug.LogError("LEFT");
            }           

            // Process all selected spawners!
            foreach (ISpawner item in m_selectedSpawners) {            
                if (item.CanRespawn()) {
					//add item into respawn stack and begin the respawn process
					m_spawning.Add(item);
					item.Respawn();
				}
			}          
        }

        m_watch.Start();
        long start = m_watch.ElapsedMilliseconds;
        ISpawner sp;

        if (m_spawning.Count > 0)
        {
            // Spawners are sorted so the ones that are closer to the camera are spawner earlier
            m_spawning.Sort(SortSpawners);

            while (m_spawning.Count > 0)
            {
                sp = m_spawning[0];  
                
                // If the spawner is in the deactivation area then its respawning stuff has to be undone as the units respawned would be destroyed anyway             
				bool cancelSpawn = false;

				if (sp.transform.position.z < BACKGROUND_LAYER_Z) 
				{
					cancelSpawn = m_newCamera.IsInsideDeactivationArea(sp.boundingRect);
				}
				else 
				{
					cancelSpawn = m_newCamera.IsInsideBackgroundDeactivationArea(sp.boundingRect);
				}

				if (cancelSpawn)
                {
                    sp.ForceRemoveEntities();
                    m_spawning.RemoveAt(0);
                }
                else if (sp.Respawn())
                {
					if (sp.MustCheckCameraBounds()) 
					{
						m_activeMustCheckCameraBounds.Add(sp);
					}
                    m_spawning.RemoveAt(0);
                }
                if (m_watch.ElapsedMilliseconds - start >= SPAWNING_MAX_TIME)
                {
                    break;
                }
            }
        }

		if (m_activeMustCheckCameraBounds.Count > 0) 
		{
			for (int i = 0; i < m_activeMustCheckCameraBounds.Count; ++i)
			{
				sp = m_activeMustCheckCameraBounds[i];

				if (m_newCamera.IsInsideDeactivationArea(sp.boundingRect))
				{
					sp.ForceRemoveEntities();
					m_activeMustCheckCameraBounds.RemoveAt(i);
					i++;
				} 
				else if (sp.IsRespawing())
				{
					m_activeMustCheckCameraBounds.RemoveAt(i);
					i++;
				}
			}
		}
	}


	//----------------------------------------------------------------------------------------------------------------//
	//            min.x0                  min.x1
	//              |                       |
	//           _______________________________  _ max.y1
	//          |               2               |
	// min.y1 _ |_______________________________|
	//          |   |                       |   |
	//          |   |                       |   |
	//          | 3 |                       | 1 |
	//          |   |                       |   |
	// min.y0 _ |___|_______________________|___|
	//          |               0               |
	//          |_______________________________| _ max.y0
	//
	//          |                               |
	//        max.x0                         max.x1
	//
	private void CheckBottom(ref QuadTree<ISpawner> _qtree, ref FastBounds2D _min, ref FastBounds2D _max) {
		m_subRect[0].Set(
			_max.x0,
			_max.y0,
			_max.w,
			_min.y0 - _max.y0
		);
		_qtree.GetHashSetInRange(m_subRect[0], ref m_selectedSpawners);
	}

	private void CheckRight(ref QuadTree<ISpawner> _qtree, ref FastBounds2D _min, ref FastBounds2D _max) {
		m_subRect[1].Set(
			_min.x1,
			_min.y0,
			_max.x1 - _min.x1,
			_min.h
		);
		_qtree.GetHashSetInRange(m_subRect[1], ref m_selectedSpawners);
	}

	private void CheckTop(ref QuadTree<ISpawner> _qtree, ref FastBounds2D _min, ref FastBounds2D _max) {
		m_subRect[2].Set(
			_max.x0,
			_min.y1,
			_max.w,
			_max.y1 - _min.y1
		);
		_qtree.GetHashSetInRange(m_subRect[2], ref m_selectedSpawners);
	}

	private void CheckLeft(ref QuadTree<ISpawner> _qtree, ref FastBounds2D _min, ref FastBounds2D _max) {
		m_subRect[3].Set(
			_max.x0,
			_min.y0,
			_min.x0 - _max.x0,
			_min.h
		);
		_qtree.GetHashSetInRange(m_subRect[3], ref m_selectedSpawners);
	}
	//----------------------------------------------------------------------------------------------------------------//


    private int SortSpawners(ISpawner a, ISpawner b)
    {
        int returnValue = 0;
        if (a != b)
        {
            Vector3 cameraPosition = m_newCameraTransform.position;
            float toA = Vector3.SqrMagnitude(cameraPosition - a.transform.position);
            float toB = Vector3.SqrMagnitude(cameraPosition - b.transform.position);
            if (toA > toB)
                returnValue = 1;
            else if (toA < toB)
                returnValue = -1;
        }

        return returnValue;
    }

    //------------------------------------------------------------------------//
    // PUBLIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Add a spawner to the manager.
    /// </summary>
    /// <param name="_spawner">The spawner to be added.</param>    
    public void Register(ISpawner _spawner, bool _addToTree) {
		m_spawners.Add(_spawner);
		if (m_spawnersTreeNear != null && _addToTree) {
			if (_spawner.transform.position.z < FAR_LAYER_Z) {
				m_spawnersTreeNear.Insert(_spawner);
			} else { 
				m_spawnersTreeFar.Insert(_spawner);
			}
		}
		_spawner.Initialize();

		// if I have data about this _spawner tell it to load
		if (m_spanwersData.ContainsKey( _spawner.GetSpawnerID() ))
		{
			_spawner.Load( m_spanwersData[ _spawner.GetSpawnerID() ] );
		}
	}

	/// <summary>
	/// Remove a spawner from the manager.
	/// </summary>
	/// <param name="_spawner">The spawner to be removed.</param>
	public void Unregister(ISpawner _spawner, bool _removeFromTree) {
        if (m_spawners.Contains(_spawner))
        {
            // resave _spanwer info
            if (m_spanwersData.ContainsKey(_spawner.GetSpawnerID())) {
                AbstractSpawnerData data = m_spanwersData[_spawner.GetSpawnerID()];
                _spawner.Save(ref data);
            }
            else {
                AbstractSpawnerData data = _spawner.Save();
                if (data != null) {
                    m_spanwersData.Add(_spawner.GetSpawnerID(), data);
                }
            }

            m_spawners.Remove(_spawner);
            if (m_spawnersTreeNear != null && _removeFromTree) {
                if (_spawner.transform.position.z < FAR_LAYER_Z) {
                    m_spawnersTreeNear.Remove(_spawner);
                }
                else {
                    m_spawnersTreeFar.Remove(_spawner);
                }
            }
        }
	}

	/// <summary>
	/// Enable all spawners in the manager.
	/// </summary>
	public void EnableSpawners() {
        // Set flag
        m_enabled = true;

        if (FeatureSettingsManager.IsDebugEnabled)
            Debug_SetActive();
    }

	/// <summary>
	/// Disable all spawners in the manager.
	/// </summary>
	public void DisableSpawners() {
		// Clear spawners
		m_enabled = false;
		for (int i = 0; i < m_spawners.Count; i++) {
			m_spawners[i].Clear();
		}
		m_selectedSpawners.Clear();
    }

	//------------------------------------------------------------------------//
	// DEBUG METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw helper debug stuff.
	/// </summary>
	public void OnDrawGizmosSelected() {
		// Quadtree grid
		if(m_spawnersTreeNear != null) {
			m_spawnersTreeNear.DrawGizmos(Colors.yellow);
			m_spawnersTreeFar.DrawGizmos(Colors.gold);
			m_spawnersTreeBG.DrawGizmos(Colors.white);
		}

		// Selected spawners
		Gizmos.color = Colors.WithAlpha(Colors.yellow, 1.0f);
		foreach(ISpawner item in m_selectedSpawners) {
			Gizmos.DrawSphere(item.boundingRect.center, 0.5f);
		}

		// All spawners gizmos
		for (int i = 0; i < m_spawners.Count; i++) {
			m_spawners[i].DrawStateGizmos();
		}

		// Sub-rectangles
		Gizmos.color = Colors.WithAlpha(Colors.lime, 0.5f);
		Gizmos.DrawCube(new Vector3(m_subRect[0].center.x, m_subRect[0].center.y, 0f), new Vector3(m_subRect[0].width, m_subRect[0].height, 1f));
		Gizmos.DrawCube(new Vector3(m_subRect[2].center.x, m_subRect[2].center.y, 0f), new Vector3(m_subRect[2].width, m_subRect[2].height, 1f));

		Gizmos.color = Colors.WithAlpha(Colors.orange, 0.5f);
		Gizmos.DrawCube(new Vector3(m_subRect[1].center.x, m_subRect[1].center.y, 0f), new Vector3(m_subRect[1].width, m_subRect[1].height, 1f));
		Gizmos.DrawCube(new Vector3(m_subRect[3].center.x, m_subRect[3].center.y, 0f), new Vector3(m_subRect[3].width, m_subRect[3].height, 1f));
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {        
		// Make sure camera reference is valid
		// Spawners are only used in the game and level editor scenes, so we can be sure that both game camera and game scene controller will be present
		Camera gameCamera = InstanceManager.sceneController.mainCamera;
		m_newCamera = gameCamera.GetComponent<GameCamera>();
        if (m_newCamera != null)        
            m_newCameraTransform = m_newCamera.transform;
		OnAreaEnter();
	}

	private void OnAreaEnter() {
		// Create and populate QuadTree
        // Get map bounds!
        Rect bounds = new Rect(-440, -100, 1120, 305);	// Default hardcoded values
		LevelData data = LevelManager.currentLevelData;
		if(data != null) {
			bounds = data.bounds;
		}
		m_spawnersTreeNear = new QuadTree<ISpawner>(bounds.x, bounds.y, bounds.width, bounds.height);
		m_spawnersTreeFar = new QuadTree<ISpawner>(bounds.x, bounds.y, bounds.width, bounds.height);
		m_spawnersTreeBG = new QuadTree<ISpawner>(bounds.x, bounds.y, bounds.width, bounds.height);

		for(int i = 0; i < m_spawners.Count; i++) {
			if (m_spawners[i].transform.position.z < FAR_LAYER_Z) {
				m_spawnersTreeNear.Insert(m_spawners[i]);
			} else if (m_spawners[i].transform.position.z < BACKGROUND_LAYER_Z) {
				m_spawnersTreeFar.Insert(m_spawners[i]);
			} else {
				m_spawnersTreeBG.Insert(m_spawners[i]);
			}
		}

		m_selectedSpawners.Clear();
		m_minRectNear = m_newCamera.activationMinRectNear;
		m_spawnersTreeNear.GetHashSetInRange(m_minRectNear.ToRect(), ref m_selectedSpawners);
		m_spawnersTreeFar.GetHashSetInRange(m_minRectNear.ToRect(), ref m_selectedSpawners);
		m_spawnersTreeBG.GetHashSetInRange(m_minRectNear.ToRect(), ref m_selectedSpawners);

		// Process all selected spawners!
		foreach(ISpawner item in m_selectedSpawners) {
			if (item.CanRespawn()) {
				int iterations = 0;
				do {
					iterations++;
				} while (iterations < 100 && !item.Respawn());
			}
		}

		EnableSpawners();
	}


	private void OnAreaExit() {
		m_selectedSpawners.Clear();
		for( int i = m_spawners.Count-1; i>=0; i-- )
		{
			ISpawner _sp = m_spawners[i];
			Unregister( _sp, false);
			_sp.ForceRemoveEntities();
		}
		m_spawners.Clear();

		m_spawnersTreeNear = null;
		m_spawnersTreeFar = null;
		m_spawnersTreeBG = null;

		DisableSpawners();
	}

	/// <summary>
	/// The game has ended.
	/// </summary>
	private void OnGameEnded() {
		OnAreaExit();

		// Clear QuadTree
		m_spawnersTreeNear = null;
		m_spawnersTreeFar = null;
		m_spawnersTreeBG = null;

        // Drop camera references
        m_newCamera = null;
        m_newCameraTransform = null;  
        
        if (m_spawners != null) {
            m_spawners.Clear();
        }

        if (m_spanwersData != null) {
            m_spanwersData.Clear();
        }             
    }   

    private void ForceResetSpawners() {
        if (m_spawners != null) {
            int count = m_spawners.Count;
            for (int i = 0; i < count; i++) {
                m_spawners[i].ForceReset();
            }
        }

        if (m_spawning != null) {
            m_spawning.Clear();
        }

        if (m_spanwersData != null) {
            m_spanwersData.Clear();
        }
    }

    public void ForceRespawn() {
        ForceResetSpawners();

        if (m_spawners != null) {
            int count = m_spawners.Count;
            for (int i = 0; i < count; i++)
            {
                m_spawning.Add(m_spawners[i]);               
            }            
        }                       
    }

#region debug
    private void Debug_Awake() {        
        Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Debug_OnChanged);

        // Enable/Disable object depending on the flag
        Debug_SetActive();
    }

    private void Debug_OnDestroy() {        
		Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Debug_OnChanged);
    }

    private void Debug_OnChanged(string _id, bool _newValue) {        
        if (_id == DebugSettings.INGAME_SPAWNERS)
        {
            // Enable/Disable object
            Debug_SetActive();
        }
    }

    private void Debug_SetActive() {
		m_enabled = Prefs.GetBoolPlayer(DebugSettings.INGAME_SPAWNERS, true);       
    }
#endregion

#region profiler
    private static float sm_totalLogicUnits = 0f;
    public static float totalLogicUnitsSpawned
    {
        get
        {
            return sm_totalLogicUnits;
        }
    }

    private static int sm_totalEntities = 0;
    public static int totalEntities
    {
        get
        {
            return sm_totalEntities;
        }
    }

    public static void AddToTotalLogicUnits(int amount, string prefabStr)
    {
        float logicUnitsCoef = 1f;
        ProfilerSettings settings = ProfilerSettingsManager.SettingsCached;
        if (settings != null)
        {
            logicUnitsCoef = (string.IsNullOrEmpty(prefabStr)) ? 1 :  settings.GetLogicUnits(prefabStr);
        }

        sm_totalEntities += amount;
        sm_totalLogicUnits += logicUnitsCoef * amount;
        if (sm_totalLogicUnits < 0f)
        {
            sm_totalLogicUnits = 0f;
        }
    }

    public static void RemoveFromTotalLogicUnits(int amount, string prefabStr)
    {
        AddToTotalLogicUnits(-amount, prefabStr);
    }

    public static void ResetTotalLogicUnitsSpawned()
    {
        sm_totalLogicUnits = 0f;
        sm_totalEntities = 0;
    }
#endregion
}

using UnityEngine;
using System.Collections.Generic;

public class WaterAreaManager : Singleton<WaterAreaManager>, IBroadcastListener {
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Spawners collection
	private List<WaterController> m_waterList = null;
	private QuadTree<WaterController> m_waterTree = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Inititalization.
    /// </summary>
    protected override void OnCreateInstance() {
        m_waterList = new List<WaterController>();
	
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

    protected override void OnDestroyInstance() {
        // Unsubscribe from external events
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}


	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
                OnLevelLoaded();
            }break;
            case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
        }
    }
    
	/// <summary>
	/// Add a spawner to the manager.
	/// </summary>
	/// <param name="_spawner">The spawner to be added.</param>
	public void Register(WaterController _water) {
		m_waterList.Add(_water);
		if (m_waterTree != null) m_waterTree.Insert(_water);
	}

	/// <summary>
	/// Remove a spawner from the manager.
	/// </summary>
	/// <param name="_spawner">The spawner to be removed.</param>
	public void Unregister(WaterController _water) {
		m_waterList.Remove(_water);
		if (m_waterTree != null) m_waterTree.Remove(_water);
	}

	public bool IsInsideWater(Vector3 _point) {
		if (m_waterTree != null) {
			return m_waterTree.HasItemsAt(_point);
		}
		return false;
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		// Create and populate QuadTree
		// Get map bounds!
		Rect bounds = new Rect(-440, -100, 1120, 305);	// Default hardcoded values
		LevelData data = LevelManager.currentLevelData;
		if(data != null) {
			bounds = data.bounds;
		}

		m_waterTree = new QuadTree<WaterController>(bounds.x, bounds.y, bounds.width, bounds.height);
		for(int i = 0; i < m_waterList.Count; i++) {
			m_waterTree.Insert(m_waterList[i]);
		}
	}

	/// <summary>
	/// The game has ended.
	/// </summary>
	private void OnGameEnded() {
		m_waterTree = null;
		m_waterList.Clear();
	}


	//------------------------------------------------------------------------//
	// DEBUG METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw helper debug stuff.
	/// </summary>
	public void OnDrawGizmosSelected() {
		// Quadtree grid
		if (m_waterTree != null) {
			m_waterTree.DrawGizmos(Colors.cyan);
		}
	}
}
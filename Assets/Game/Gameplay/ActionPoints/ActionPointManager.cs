using UnityEngine;
using System.Collections.Generic;

public class ActionPointManager : Singleton<ActionPointManager>, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_INTERVAL = 0.2f;	// Seconds, avoid updating all the spawners all the time for better performance
	public const float BACKGROUND_LAYER_Z = 45f;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Spawners collection
	private List<ActionPoint> m_actionPoints = null;
	private List<ActionPoint> actionPoints {
		get { 
			if(m_actionPoints == null) m_actionPoints = new List<ActionPoint>();
			return m_actionPoints;
		}
	}

	private QuadTree<ActionPoint> m_actionPointsTree = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Component enabled.
    /// </summary>
    protected override void OnCreateInstance() {
        // Subscribe to external events
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

    /// <summary>
    /// Component disabled.
    /// </summary>
    protected override void OnDestroyInstance() {
        // Unsubscribe from external events
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}

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

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Add an action point to the manager.
	/// </summary>
	/// <param name="_actionPoint">The action point to be added.</param>
	public void Register(ActionPoint _actionPoint) {
		actionPoints.Add(_actionPoint);
		if (m_actionPointsTree != null) m_actionPointsTree.Insert(_actionPoint);
	}

	/// <summary>
	/// Remove an action point from the manager.
	/// </summary>
	/// <param name="_actionPoint">The action point to be removed.</param>
	public void Unregister(ActionPoint _actionPoint) {
		actionPoints.Remove(_actionPoint);
		if (m_actionPointsTree != null) m_actionPointsTree.Remove(_actionPoint);
	}


	public ActionPoint GetActionPointAt(Vector3 _pos) {
		if (m_actionPointsTree != null) {
			ActionPoint[] list = m_actionPointsTree.GetItemsAt(_pos);
			if (list.Length > 0) {
				return list.First();
			}
		}

		return null;
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

		m_actionPointsTree = new QuadTree<ActionPoint>(bounds.x, bounds.y, bounds.width, bounds.height);
		List<ActionPoint> points = actionPoints;	// Make sure list is initialized by calling the property
		for(int i = 0; i < points.Count; i++) {
			m_actionPointsTree.Insert(points[i]);
		}
	}

	private void OnGameEnded() {
		actionPoints.Clear();
	}

	//------------------------------------------------------------------------//
	// DEBUG METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw helper debug stuff.
	/// </summary>
	public void OnDrawGizmosSelected() {
		// Quadtree grid
		if (m_actionPointsTree != null) {
			m_actionPointsTree.DrawGizmos(Colors.yellow);
		}
	}
}
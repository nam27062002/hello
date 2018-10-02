using UnityEngine;
using System.Collections.Generic;

public class ActionPointManager : UbiBCN.SingletonMonoBehaviour<ActionPointManager> {
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
	/// Inititalization.
	/// </summary>
	private void Awake() {	
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, OnGameEnded);
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
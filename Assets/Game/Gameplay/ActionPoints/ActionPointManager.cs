using UnityEngine;
using System.Collections.Generic;

public class ActionPointManager : SingletonMonoBehaviour<ActionPointManager> {
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
	private QuadTree<ActionPoint> m_actionPointsTree = null;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Inititalization.
	/// </summary>
	private void Awake() {
		m_actionPoints = new List<ActionPoint>();
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}


	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Add an action point to the manager.
	/// </summary>
	/// <param name="_actionPoint">The action point to be added.</param>
	public void Register(ActionPoint _actionPoint) {
		m_actionPoints.Add(_actionPoint);
		if (m_actionPointsTree != null) m_actionPointsTree.Insert(_actionPoint);
	}

	/// <summary>
	/// Remove an action point from the manager.
	/// </summary>
	/// <param name="_actionPoint">The action point to be removed.</param>
	public void Unregister(ActionPoint _actionPoint) {
		m_actionPoints.Remove(_actionPoint);
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
		LevelMapData data = GameObjectExt.FindComponent<LevelMapData>(true);
		if(data != null) {
			bounds = data.mapCameraBounds;
		}

		m_actionPointsTree = new QuadTree<ActionPoint>(bounds.x, bounds.y, bounds.width, bounds.height);
		for(int i = 0; i < m_actionPoints.Count; i++) {
			m_actionPointsTree.Insert(m_actionPoints[i]);
		}
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
// LevelData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Scriptable object meant to store all kind of data related to a game level.
/// </summary>
[CreateAssetMenu(fileName = "NewLevelData", menuName = "HungryDragon/Level Data")]
public class LevelData : ScriptableObject {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string DEFAULT_MAP_PREFAB = "Game/Levels/PF_MapDefault";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private string m_debugName = "New Level";
	public string debugName {
		get { return m_debugName; }
	}

	// [AOC] TODO!!
	// [Separator("Scenes")]
	// [InfoBox("Don't forget to add all the referenced scenes to the Build Settings!")]
	// [SerializeField] private string m_spawnersScene = null;
	// [SerializeField] private string m_collisionScene = null;
	// [SerializeField] private string m_soundScene = null;
	// [SerializeField] private string[] m_artScenes = null;

	[Separator("Other Data")]
	[SerializeField] private Rect m_bounds = new Rect(-500f, 200f, 1000f, 400f);	// Default arbitrary size
	public Rect bounds {
		get { return m_bounds; }
	}

	[SerializeField] private GameObject m_mapPrefab = null;
	public GameObject mapPrefab {
		get {
			// If no prefab is defined, use default one
			if(m_mapPrefab == null) {
				m_mapPrefab = Resources.Load<GameObject>(DEFAULT_MAP_PREFAB);
			}
			return m_mapPrefab;
		}
	}	

	[Separator("Debug")]
	[SerializeField] private Color m_boundsColor = Colors.paleYellow;

	// Volatile members
	// Not stored, must be initialized in runtime using the Init method.
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize volatile data for this level data instance.
	/// </summary>
	/// <param name="_levelDef">The definition of the level to be linked with this data.</param>
	public void Init(DefinitionNode _levelDef) {
		// Just store level's def
		m_def = _levelDef;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}
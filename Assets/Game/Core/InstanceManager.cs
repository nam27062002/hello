// InstanceManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Static access point to non-static objects. All of the objects may be null, so use it carefully.
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class InstanceManager : UbiBCN.SingletonMonoBehaviour<InstanceManager> {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Current scene controller, cast it to the right one for the active scene
	private SceneController m_sceneController = null;
	public static SceneController sceneController {
		get { return instance.m_sceneController; }
		set { if(instance != null) instance.m_sceneController = value; }
	}

	private MusicController m_musicController = null;
	public static MusicController musicController{
		get { return instance.m_musicController; }
		set { if(instance != null) instance.m_musicController = value; }
	}

	//------------------------------------------------------------------//
	// ONLY IN MENU SCENE												//
	//------------------------------------------------------------------//
	public static MenuSceneController menuSceneController {
		get { return instance.m_sceneController as MenuSceneController; }
	}

	//------------------------------------------------------------------//
	// ONLY IN GAME SCENE												//
	//------------------------------------------------------------------//
	public static GameSceneControllerBase gameSceneControllerBase {
		get { return instance.m_sceneController as GameSceneControllerBase; }
	}

	public static GameSceneController gameSceneController {
		get { return instance.m_sceneController as GameSceneController; }
	}

	private DragonPlayer m_player = null;	// DEPRECATED
	public static DragonPlayer player {
		get { return instance.m_player; }
		set { if(instance != null) instance.m_player = value; }
	}

	private GameCamera m_gameCamera = null;
	public static GameCamera gameCamera {
		get { return instance.m_gameCamera; }
		set { if(instance != null) instance.m_gameCamera = value; }
	}

	private MapCamera m_mapCamera = null;
	public static MapCamera mapCamera {
		get { return instance.m_mapCamera; }
		set { if(instance != null) instance.m_mapCamera = value; }
	}

	private ZoneManager m_zoneManager = null;
	public static ZoneManager zoneManager {
		get { return instance.m_zoneManager; }
		set { if(instance != null) instance.m_zoneManager = value; }
	}

	private FogManager m_fogManager = null;
	public static FogManager fogManager{
		get { return instance.m_fogManager; }
		set { if(instance != null) instance.m_fogManager = value; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Clear all references
		m_sceneController = null;
		m_player = null;

		// Call parent
		base.OnDestroy();
	}

	//------------------------------------------------------------------//
	// PUBLIC STATIC METHODS											//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	//TO DELETE
	static System.Collections.Generic.Dictionary<string, Modifier> m_mods  = new System.Collections.Generic.Dictionary<string, Modifier>();
	static System.Collections.Generic.Dictionary<string, Modifier> m_dMods = new System.Collections.Generic.Dictionary<string, Modifier>();

	public static void CREATE_MODIFIERS() {
		System.Collections.Generic.List<DefinitionNode> mods = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LIVE_EVENTS_MODIFIERS);
		for (int i = 0; i < mods.Count; ++i) {
			Modifier m = Modifier.CreateFromDefinition(mods[i]);
			if (m is ModifierDragon) {
				m_dMods.Add(mods[i].sku, m);
			} else {
				m_mods.Add(mods[i].sku, m);
			}
		}
	}

	public static void APPLY_MODIFIERS() {
		m_mods["double_mission"].Apply();
		//m_mods["invasion_dragon"].Apply();
	}
	public static void REMOVE_MODIFIERS() {
		m_mods["double_mission"].Remove();
		//m_mods["invasion_dragon"].Remove();
	}

	public static void APPLY_DRAGON_MODIFIERS() {
		m_dMods["bbq"].Apply();
	}
	public static void REMOVE_DRAGON_MODIFIERS() {
		m_dMods["bbq"].Remove();
	}
	//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}


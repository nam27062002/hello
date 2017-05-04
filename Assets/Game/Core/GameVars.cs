// GameVars.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global volatile game variables to control several states, flags and values 
/// during a single session.
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class GameVars : UbiBCN.SingletonMonoBehaviour<GameVars> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[Comment("Whether the initial play screen has been displayed or not")]
	[SerializeField] private bool m_playScreenShown = false;
	public static bool playScreenShown {
		get { return instance.m_playScreenShown; }
		set { instance.m_playScreenShown = value; }
	}

	[Comment("Screen to go first when loading the menu scene. Will be reset after applied.")]
	[SerializeField] private MenuScreens m_menuInitialScreen = MenuScreens.NONE;
	public static MenuScreens menuInitialScreen {
		get { return instance.m_menuInitialScreen; }
		set { instance.m_menuInitialScreen = value; }
	}

	[Comment("Dragon to select when loading the menu scene. Will be reset after applied. If not defined, current dragon will be used.")]
	[SerializeField] private string m_menuInitialDragon = "";
	public static string menuInitialDragon {
		get { return instance.m_menuInitialDragon; }
		set { instance.m_menuInitialDragon = value; }
	}

	[Comment("Dragon unlocked")]
	[SerializeField] private string m_unlockedDragonSku = "";
	public static string unlockedDragonSku {
		get { return instance.m_unlockedDragonSku; }
		set { instance.m_unlockedDragonSku = value; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
}


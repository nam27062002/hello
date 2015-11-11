// LevelEditorSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Possible shape options for the spawners.
	/// </summary>
	public enum SpawnerShape {
		POINT,
		RECTANGLE,
		CIRCLE
	};

	/// <summary>
	/// Possible type options for the spawners.
	/// </summary>
	public enum SpawnerType {
		STANDARD,
		FLOCK
	};

	/// <summary>
	/// Data class to store preferences for the level editor. Best way to keep 
	/// preferences between sessions and edit/play mode.
	/// </summary>
	public class LevelEditorSettings : ScriptableObject {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		[Separator("General Settings", 20)]
		public float snapSize = 5f;	// Snap size for ground pieces
		public float handlersSize = 1f;	// Size of the custom handlers (i.e. ground pieces)
		public int selectedTab = 0;	// Selected section tab

		[Separator("Level Settings", 20)]
		[HideEnumValuesAttribute(true, true)]
		public DragonId testDragon = DragonId.SMALL;	// Dragon with which to test the level

		[Separator("Ground Settings", 20)]
		public Vector3 groundPieceSize = new Vector3(50f, 1f, 15f);	// Ground pieces default size
		public int groundPieceColorIdx = 0;	// Ground pieces default color

		[Separator("Spawners Settings", 20)]
		public SpawnerShape spawnerShape = SpawnerShape.CIRCLE;	// Shape of the spawner
		public SpawnerType spawnerType = SpawnerType.STANDARD;	// Default behaviour of the spawner
	}
}


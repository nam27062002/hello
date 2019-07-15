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
		FLOCK,
		PATH
	};

	/// <summary>
	/// Possible shape options for the collision objects.
	/// </summary>
	public enum CollisionShape {
		RECTANGLE,
		CIRCLE,
		TRIANGLE
	};

	/// <summary>
	/// Data class to store preferences for the level editor. Best way to keep 
	/// preferences between sessions and edit/play mode.
	/// </summary>
	public class LevelEditorSettings : ScriptableObject {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		public enum Mode {
			SPAWNERS,
			COLLISION,
			ART,
			SOUND,

			COUNT
		}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		[Separator("General Settings", 20)]
		public float snapSize = 5f;	// Snap size for ground pieces
		public float handlersSize = 1f;	// Size of the custom handlers (i.e. ground pieces)
		public int selectedTab = 0;	// Selected section tab
		[HideEnumValues(false, true)] public Mode selectedMode = Mode.SPAWNERS;	// Selected mode

		[Separator("Level Settings", 20)]
		[SkuList(DefinitionsCategory.DRAGONS)]
		public string testDragon = "";	// Dragon with which to test the level
		public bool useIntro = true;
		public bool spawnAtCameraPos = false;
		public string levelSku = "";
		public string spawnPoint = "";
		public bool progressionCustom = false;
		public bool progressionFilterByTier = true;
		public bool progressionFilterBySpawnPoint = true;
		public string progressionOffsetSeconds = "0";
		public string progressionOffsetXP = "0";

		[Separator("Ground Settings", 20)]
		public Vector3 groundPieceSize = new Vector3(50f, 1f, 15f);	// Ground pieces default size
		public int groundPieceColorIdx = 0;	// Ground pieces default color
		public CollisionShape groundPieceShape = CollisionShape.RECTANGLE;

		[Separator("Spawners Settings", 20)]
		public SpawnerShape spawnerShape = SpawnerShape.CIRCLE;	// Shape of the spawner
		public SpawnerType spawnerType = SpawnerType.STANDARD;	// Default behaviour of the spawner

		[Separator("Group Settings", 20)]
		public bool[] groupRewardsFolding = new bool[3] { false, false, true };	// Folded status of the group editor rewards

		[Separator("Particle Manager", 20)]
		public string poolLimit = "unlimited";

		[Separator("Spawners", 20)]
		public bool previewPaths = true;
	}
}


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
	/// Data class to store preferences for the level editor. Best way to keep 
	/// preferences between sessions and edit/play mode.
	/// </summary>
	[CreateAssetMenu]
	public class LevelEditorSettings : ScriptableObject {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// Dragon with which to test the level
		[HideEnumValuesAttribute(true, true)]
		public DragonId testDragon = DragonId.SMALL;

		// Snap size for ground pieces
		public float snapSize = 5f;

		// Ground pieces default size
		public Vector3 groundPieceSize = new Vector3(50f, 1f, 15f);

		// Ground pieces default color
		public int groundPieceColorIdx = 0;
	}
}


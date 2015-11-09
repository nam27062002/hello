// ILevelEditorSection.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/11/2015.
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
	/// Common interface for all sections in the level editor.
	/// </summary>
	public interface ILevelEditorSection {
		//------------------------------------------------------------------//
		// INTERFACE METHODS												//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		void Init();

		/// <summary>
		/// Draw the section.
		/// </summary>
		void OnGUI();
	}
}
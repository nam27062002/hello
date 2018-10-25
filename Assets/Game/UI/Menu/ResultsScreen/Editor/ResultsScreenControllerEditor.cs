// ResultsScreenController_NEWEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ResultsScreenController_NEW class.
/// </summary>
[CustomEditor(typeof(ResultsScreenController), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class ResultsScreenControllerEditor : ReorderableArrayInspector {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private struct StepSetup {
		public ResultsScreenController.Step step;
		public bool visible;
		public bool space;
		public string comment;

		public StepSetup(ResultsScreenController.Step _step, bool _visible, bool _space, string _comment) {
			step = _step;
			visible = _visible;
			space = _space;
			comment = _comment;
		}
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private static StepSetup[] s_stepSetups = null;
	private static StepSetup[] stepSetups {
		get {
			if(s_stepSetups == null){
				s_stepSetups = new StepSetup[] {
					new StepSetup(ResultsScreenController.Step.INIT, false, false, ""),

					new StepSetup(ResultsScreenController.Step.INTRO, true, false, "Always, dragon animation"),
					new StepSetup(ResultsScreenController.Step.SCORE, true, false, "Always, run score + high score feedback"),
					new StepSetup(ResultsScreenController.Step.REWARDS, true, false, "Always, SC earned during the run"),

					new StepSetup(ResultsScreenController.Step.COLLECTIBLES, true, true, "Always, collected Eggs, Chests, etc."),
					new StepSetup(ResultsScreenController.Step.MISSIONS, true, false, "Optional, completed missions"),

					new StepSetup(ResultsScreenController.Step.XP, true, true, "Always, dragon xp progression"),

					new StepSetup(ResultsScreenController.Step.TRACKING, true, true, "Logic step, send end of game tracking - before applying the rewards!"),
					new StepSetup(ResultsScreenController.Step.APPLY_REWARDS, true, false, "Logic step, apply rewards to the profile"),

					new StepSetup(ResultsScreenController.Step.SKIN_UNLOCKED, true, true, "Optional, if a skin was unlocked. As many times as needed if more than one skin was unlocked in the same run"),
					new StepSetup(ResultsScreenController.Step.DRAGON_UNLOCKED, true, false, "Optional, if a new dragon was unlocked"),

					new StepSetup(ResultsScreenController.Step.GLOBAL_EVENT_CONTRIBUTION, true, true, "Optional, if there is an active event and the player has a score to add to it"),
					new StepSetup(ResultsScreenController.Step.GLOBAL_EVENT_NO_CONTRIBUTION, true, false, "Optional, if there is an active event but the player didn't score"),

					new StepSetup(ResultsScreenController.Step.TOURNAMENT_COINS, true, true, "Tournament, gold obtained during the run"),
					new StepSetup(ResultsScreenController.Step.TOURNAMENT_SCORE, true, false, "Tournament, show run score"),
					new StepSetup(ResultsScreenController.Step.TOURNAMENT_LEADERBOARD, true, false, "Tournament, show leaderboard changes"),
					new StepSetup(ResultsScreenController.Step.TOURNAMENT_INVALID_RUN, true, false, "Tournament, run didn't count for the tournament (i.e. \"Eat 100 birds as fast as possible\" but you died before reaching 100 birds)"),
					new StepSetup(ResultsScreenController.Step.TOURNAMENT_SYNC, true, false, "Tournament, sync with server, apply rewards and do tracking"),

					new StepSetup(ResultsScreenController.Step.LEAGUE_SCORE, true, true, "Special Dragons League, show run score and \"new high score\" if moving up the ladder"),
					new StepSetup(ResultsScreenController.Step.LEAGUE_LEADERBOARD, true, false, "Special Dragons League, show leaderboard changes"),
					new StepSetup(ResultsScreenController.Step.LEAGUE_SYNC, true, false, "Special Dragons League, sync with server, apply rewards and do tracking"),

					new StepSetup(ResultsScreenController.Step.COUNT, false, false, "")
				};
			}
			return s_stepSetups;
		}
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Unity's "script" property
			if(p.name == "m_Script") {
				// Draw the property, disabled
				bool wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.PropertyField(p, true);
				GUI.enabled = wasEnabled;
			}

			// Steps array
			else if(p.name == "m_steps") {
				// Fixed length, each step has its enum name as label
				p.arraySize = (int)ResultsScreenController.Step.COUNT;

				// Group in a foldout
				p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
				if(p.isExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// Show them nicely formatted
					for(int i = 0; i < p.arraySize; ++i) {
						// Custom formatting based on step
						StepSetup setup = stepSetups[i];

						// Show?
						if(!setup.visible) continue;

						// Add space before?
						if(setup.space) GUILayout.Space(20f);

						// Add comment?
						if(!string.IsNullOrEmpty(setup.comment)) {
							EditorGUILayout.LabelField(setup.comment, CustomEditorStyles.commentLabelLeft);
						}

						// Just use the default inspector with the enum name as label
						EditorGUILayout.PropertyField(
							p.GetArrayElementAtIndex(i), 
							new GUIContent(setup.step.ToString()),
							true
						);
					}

					// Indent out
					EditorGUI.indentLevel--;
				}
			}

			// Reorderable Lists
			else if(p.name == "m_tournamentStepsSequence"
				|| p.name == "m_defaultStepsSequence"
			    || p.name == "m_specialDragonStepsSequence") {
				base.DrawPropertySortableArray(p);
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}
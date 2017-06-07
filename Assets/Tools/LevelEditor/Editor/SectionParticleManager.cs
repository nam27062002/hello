// SectionParticleManager.cs

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#pragma warning disable 0414

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace LevelEditor {
	
	public class SectionParticleManager : ILevelEditorSection {
		//--------------------------------------------------------------------//
		// CONSTANTS														  //
		//--------------------------------------------------------------------//

		//--------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											  //
		//--------------------------------------------------------------------//
	
		//--------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {
			
		}
		
		/// <summary>
		/// Draw the section.
		/// </summary>
		public void OnGUI() {
			// Title - encapsulate in a nice button to make it foldable
			GUI.backgroundColor = Colors.gray;
			bool folded = Prefs.GetBoolEditor("LevelEditor.SectionParticleManager.folded", false);
			if(GUILayout.Button((folded ? "►" : "▼") + " Particle Manager", LevelEditorWindow.styles.sectionHeaderStyle, GUILayout.ExpandWidth(true))) {
				folded = !folded;
				Prefs.SetBoolEditor("LevelEditor.SectionParticleManager.folded", folded);
			}
			GUI.backgroundColor = Colors.white;

			// -Only show if unfolded
			if(!folded) {
				// Group in a box
				EditorGUILayout.BeginVertical(LevelEditorWindow.styles.sectionContentStyle, GUILayout.Height(1)); {	// [AOC] Requesting a very small size fits the group to its content's actual size
						
					EditorGUILayout.BeginHorizontal(); {
						// Label
						GUILayout.Label("Pool limits");

						// Dragon selector
						string oldOption = LevelEditor.settings.poolLimit;
						string newOption = "";
						string[] options = new string[3];

						options[0] = "unlimited";
						options[1] = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA1";
						options[2] = "PARTICLE_MANAGER_SETTINGS_LEVEL_0_AREA2";

						int oldIdx = ArrayUtility.IndexOf<string>(options, oldOption);
						int newIdx = EditorGUILayout.Popup(Mathf.Max(oldIdx, 0), options);	// Select first dragon if saved dragon was not found (i.e. sku changes)
						if(oldIdx != newIdx) {
							newOption = options[newIdx];
							LevelEditor.settings.poolLimit = newOption;
							EditorUtility.SetDirty(LevelEditor.settings);
							AssetDatabase.SaveAssets();
						}
					} EditorGUILayoutExt.EndHorizontalSafe();
				} EditorGUILayout.EndVertical();
			}
		}

		//--------------------------------------------------------------------//
		// INTERNAL METHODS													  //
		//--------------------------------------------------------------------//

		//--------------------------------------------------------------------//
		// CALLBACKS														  //
		//--------------------------------------------------------------------//

	}
}
//#define SPLIT_SPAWNER_SCENES

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Tool that splits world into variations depending on Quality values set by WorldSplitter
/// </summary>
public static class WorldSplitterTool
{
	public enum WorldSplitterQuality
	{
		Low,
		Medium,
		High
	};

	public static readonly Dictionary<WorldSplitterQuality, string> m_variationNames = new Dictionary<WorldSplitterQuality, string>
	{
		{WorldSplitterQuality.Low, "_low" },
		{WorldSplitterQuality.Medium, "_medium" },
		{WorldSplitterQuality.High, "_high" },
	};


	public static readonly string RegularScenesPath = "Assets/Game/Scenes/Levels/Art/Art_";
	public static readonly string SpawnerScenesPath = "Assets/Game/Scenes/Levels/Spawners/SP_";
	public const string LevelsPath = "Assets/Game/Scenes/Levels/";

	public static readonly List<string> m_knownScenes = new List<string>
	{
		LevelsPath + "Art/ART_Particles",
	};



	public static void SplitAllScenes()
	{
		foreach (string scene in m_knownScenes)
		{
			foreach (WorldSplitterQuality quality in Enum.GetValues(typeof(WorldSplitterQuality)))
			{
				SplitSceneQuality(scene, quality);
			}
		}
	}

	/// <summary>
	/// Actual code that does the scene split
	/// </summary>
	public static void SplitSceneQuality(string name, WorldSplitterQuality quality)
	{
	/*
		string inputPathWorld = RegularScenesPath + name + ".unity";
		string outputPathWorld = RegularScenesPath + name + m_variationNames[quality] + ".unity";

		string inputPathSpawner = SpawnerScenesPath + name + ".unity";
		string outputPathSpawner = SpawnerScenesPath + name + m_variationNames[quality] + ".unity";

		Debug.Log(string.Format("Spliting scene: {0} -> {1} and {2} -> {3}", inputPathWorld, outputPathWorld, inputPathSpawner, outputPathSpawner));

		//	Process step 1
		EditorApplication.OpenScene(inputPathWorld);
		ProcessCurrentScene(quality);
		EditorApplication.SaveScene(outputPathWorld);

		//	Process step 2
#if SPLIT_SPAWNER_SCENES
		EditorApplication.OpenScene(inputPathSpawner);
		ProcessCurrentScene(quality);
		EditorApplication.SaveScene(outputPathSpawner);
#endif
*/

		string inputPathWorld = name + ".unity";
		string outputPathWorld = name + m_variationNames[quality] + ".unity";

		Debug.Log(string.Format("Spliting scene: {0} -> {1}", inputPathWorld, outputPathWorld));

		EditorApplication.OpenScene(inputPathWorld);
		ProcessCurrentScene(quality);
		EditorApplication.SaveScene(outputPathWorld);
	}

	static void ProcessCurrentScene(WorldSplitterQuality quality)
	{
		WorldSplitter[] spliters = GameObject.FindObjectsOfType<WorldSplitter>();

		List<GameObject> delList = new List<GameObject>();
		List<GameObject> keepList = new List<GameObject>();

		//	Find objects and determine should they be removed or not
		foreach (WorldSplitter current in spliters)
		{
			bool shouldRemove = false;

			if (!current.Low && quality == WorldSplitterQuality.Low || !current.Medium && quality == WorldSplitterQuality.Medium || !current.High && quality == WorldSplitterQuality.High)
				shouldRemove = true;

			if (shouldRemove)
			{
				delList.Add(current.gameObject);
			}
			else
			{
				keepList.Add(current.gameObject);
			}
		}

		//	Make sure we don't delete a higher up parent that is needed for a nested child
		//	Copied from HSE
		foreach (GameObject go1 in keepList)
		{
			List<GameObject> dontDel = new List<GameObject>();
			foreach (GameObject go2 in delList)
			{
				if (go1.transform.IsChildOf(go2.transform))
				{
					dontDel.Add(go2);
				}
			}
			foreach (GameObject go in dontDel)
			{
				delList.Remove(go);
			}
		}

		//	Now remove splitters themselves
		foreach (WorldSplitter sp in spliters)
		{
			GameObject.DestroyImmediate(sp);
		}

		//	Now finally remove stuff
		foreach (GameObject go in delList)
		{
			GameObject.DestroyImmediate(go);
		}
	}
}

/// <summary>
/// Editor tool window class
/// </summary>
public class WorldSplitterEditorWindow : EditorWindow
{
	class ToggleState
	{
		public bool enabled;
		public bool low;
		public bool medium;
		public bool high;

		public ToggleState()
		{
			enabled = low = medium = high = true;
		}
	}

	static Dictionary<string, ToggleState> m_stateValues = null;

	[MenuItem("Hungry Dragon/World Splitter/Show Tool")]
	public static void ShowWindow()
	{
		var window = EditorWindow.GetWindow(typeof(WorldSplitterEditorWindow));
		window.titleContent = new GUIContent("World Splitter Tool");

		if (m_stateValues == null)
		{
			m_stateValues = new Dictionary<string, ToggleState>();
			foreach (string scene in WorldSplitterTool.m_knownScenes)
			{
				m_stateValues.Add(scene, new ToggleState());
			}
		}
		window.minSize = new Vector2(450, 200);
		window.Show();
	}

	bool m_restoreLastScene = true;

	void OnGUI()
	{
		GUILayout.BeginVertical();

		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Please select what scenes and variants to split and push button bellow.");
		GUILayout.EndHorizontal();
		GUILayout.Space(10);

		foreach (string scene in WorldSplitterTool.m_knownScenes)
		{
			GUILayout.BeginHorizontal();

			m_stateValues[scene].enabled = GUILayout.Toggle(m_stateValues[scene].enabled, scene, GUILayout.Width(150));

			m_stateValues[scene].low = GUILayout.Toggle(m_stateValues[scene].low, "Low");
			m_stateValues[scene].medium = GUILayout.Toggle(m_stateValues[scene].medium, "Medium");
			m_stateValues[scene].high = GUILayout.Toggle(m_stateValues[scene].high, "High");

			GUILayout.EndHorizontal();
		}

		GUILayout.Space(10);

		m_restoreLastScene = GUILayout.Toggle(m_restoreLastScene, "Restore current scene after its done.");

		GUILayout.Space(10);

		if (GUILayout.Button("Split selected scenes", GUILayout.MinHeight(60)))
		{
			ProcessButton();
		}

		GUILayout.EndVertical();
	}

	void ProcessButton()
	{
		//	Calculate steps required
		float total = 0;
		float index = 0;

		foreach (KeyValuePair<string, ToggleState> scene in m_stateValues)
		{
			if (scene.Value.enabled)
			{
				if (scene.Value.low) total++;
				if (scene.Value.medium) total++;
				if (scene.Value.high) total++;
			}
		}

		if (total == 0)
		{
			EditorUtility.DisplayDialog("Nothing selected", "You have to select at least one scene / variation combination to execute the split", "Yes sir");
			return;
		}

		//	Cache current loaded scene so we can revert
		string lastLoadedScene = EditorApplication.currentScene;
		if (lastLoadedScene != null && m_restoreLastScene)
			total++; // one more step to revert the scene

		EditorUtility.DisplayProgressBar("Spliting scenes", "", 0);

		//	Execute em
		foreach (KeyValuePair<string, ToggleState> scene in m_stateValues)
		{
			if (scene.Value.enabled)
			{
				//	Optimize this scene
				if (scene.Value.low)
				{
					EditorUtility.DisplayProgressBar("Generating scene variant", scene.Key + " Low ", ++index / total);
					WorldSplitterTool.SplitSceneQuality(scene.Key, WorldSplitterTool.WorldSplitterQuality.Low);
				}
				if (scene.Value.medium)
				{
					EditorUtility.DisplayProgressBar("Generating scene variant", scene.Key + " Medium ", ++index / total);
					WorldSplitterTool.SplitSceneQuality(scene.Key, WorldSplitterTool.WorldSplitterQuality.Medium);
				}
				if (scene.Value.high)
				{
					EditorUtility.DisplayProgressBar("Generating scene variant", scene.Key + " High ", ++index / total);
					WorldSplitterTool.SplitSceneQuality(scene.Key, WorldSplitterTool.WorldSplitterQuality.High);
				}
			}
		}

		if (lastLoadedScene != null && m_restoreLastScene)
		{
			EditorUtility.DisplayProgressBar("Restoring last open scene", "Restoring scene: " + lastLoadedScene, ++index / total);
			EditorApplication.OpenScene(lastLoadedScene);
		}

		EditorUtility.ClearProgressBar();
		EditorUtility.DisplayDialog("Completed", "All selected scene and variants have been generated. All done.", "Go away");
	}
}

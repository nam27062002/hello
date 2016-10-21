using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

class LightProbePlacer : EditorWindow
{
	int m_minX;
	int m_maxX;
	int m_minY;
	int m_maxY;
	int m_stepX;
	int m_stepY;
	int m_zOffset;
	/*
	[MenuItem("FGOL/Save probes")]
	static void SaveProbes()
	{
		ProbeSaver ps = GameObject.FindObjectOfType<ProbeSaver> ();
		if (ps != null)
		{
			ps.probes = LightmapSettings.lightProbes;

			EditorUtility.SetDirty(LightmapSettings.lightProbes);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        }
	}
	*/

	[MenuItem("Hungry Dragon/Light Probes/Report Black Probes", false, 3)]
	static void ReportBlack()
	{

		var coefficients = LightmapSettings.lightProbes.bakedProbes;
		var positions = LightmapSettings.lightProbes.positions;
		var blackProbes = new List<Vector3>();

//		for (var i = 0; i < coefficients.Length / 27; i++)
//		{
//			var black = true;
//
//			for (var j = 0; j < 27; j++)
//			{
//				if (coefficients[i * 27 + j] != 0)
//				{
//					black = false;
//					break;
//				}
//			}
//
//			if (black)
//				blackProbes.Add(positions[i]);
//		}

		for (var i = 0; i < coefficients.Length; i++)
		{
			var black = true;

			for(int x = 0; x < 3; x++)
			{
				for(int y = 0; y < 9; y++)
				{
					if(coefficients[i][y,x] != 0.0f)
					{
						black = false;
						break;
					}
				}
			}

			if (black)
				blackProbes.Add(positions[i]);
		}

		var root = new GameObject("Black Probes").transform;

		for (int i = 0; i < blackProbes.Count; i++)
		{
			var go = new GameObject(i.ToString());
			go.transform.parent = root;
			go.transform.position = blackProbes[i];
		}
	}

	[MenuItem("Hungry Dragon/Light Probes/Light Probe Placer", false, 2)]
	static void GetWindow()
	{
		GetWindow<LightProbePlacer>();
	}

	void OnGUI()
	{
		m_minX = EditorGUILayout.IntField("Min X", m_minX);
		m_maxX = EditorGUILayout.IntField("Max X", m_maxX);
		m_minY = EditorGUILayout.IntField("Min Y", m_minY);
		m_maxY = EditorGUILayout.IntField("Max Y", m_maxY);
		m_stepX = EditorGUILayout.IntField("Step X", m_stepX);
		m_stepY = EditorGUILayout.IntField("Step Y", m_stepY);
		m_zOffset = EditorGUILayout.IntField("Z Offset", m_zOffset);

		if (GUILayout.Button("Place Probes"))
			Run();
	}

	void Run()
	{
		var existing = FindObjectsOfType<LightProbeGroup>().Length > 0;

		if (existing && !EditorUtility.DisplayDialog("Light Probe Placer", "Existing light probes found.", "Continue", "Cancel"))
			return;

		var response = EditorUtility.DisplayDialogComplex("Light Probe Placer", "Do you want to keep your unsaved changes?", "Keep", "Discard", "Cancel");


		if (response == 0)
			EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		else if (response == 1)
			EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path);
		else if (response == 2)
			return;

		var chunks = GetLightProbePositions();
		var root = new GameObject("LightProbes").transform;

		for (var i = 0; i < chunks.Count; i++)
		{
			var go = new GameObject(i.ToString());
			go.transform.parent = root;
			go.AddComponent<LightProbeGroup>().probePositions = chunks[i];
		}

#if UNITY_5_3_OR_NEWER
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
#else
        EditorApplication.SaveScene();
#endif
    }

	List<Vector3[]> GetLightProbePositions()
	{
		var firstX = true;
		var x = m_minX;
		var positions = new List<Vector3>();
		var chunks = new List<Vector3[]>();
		var columns = 0;

		while (x < m_maxX)
		{
			if (firstX)
				firstX = false;
			else
				x += m_stepX;

			var firstY = true;
			var y = m_minY;

			while (y < m_maxY)
			{
				if (firstY)
					firstY = false;
				else
					y += m_stepY;

				var pos = new Vector3(x, y, 0);

				if (!IsInsideMesh(pos))
				{
					positions.Add(new Vector3(x, y, -m_zOffset));
					positions.Add(new Vector3(x, y, m_zOffset));
				}
			}

			if (++columns == 10)
			{
				chunks.Add(positions.ToArray());
				positions.Clear();
				columns = 0;
			}
		}

		if (positions.Count > 0)
			chunks.Add(positions.ToArray());

		return chunks;
	}

	static bool IsInsideMesh(Vector3 position)
	{
		var start = position;
		start.y = 100;
		var hits = CountHits(start, position);
		hits += CountHits(position, start);
		return hits%2 == 1;
	}

	static int CountHits(Vector3 from, Vector3 to)
	{
		var offset = (to - from).normalized/100.0f;
		RaycastHit hit;
		var hits = 0;

		while (Physics.Linecast(from, to, out hit))
		{
			hits++;
			from = hit.point + offset;
		}

		return hits;
	}
}
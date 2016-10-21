using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Track objects of Monobehaviour type
public class ActiveMonobehaviourProfiler : MonoBehaviour
{
	private struct ScriptProfilerInfo
	{
		public Vector3 m_position;
		public string  m_type;

		public ScriptProfilerInfo(Vector3 pos, string type)
		{
			m_position = pos;
			m_type = type;
		}
	}

	private const float UPDATE_INTERVAL = 10;

	private const float GIZMO_SIZE = 2;

	private float m_timer = 0;

	private List<ScriptProfilerInfo> m_scripts = new List<ScriptProfilerInfo>();

	private List<Type> m_excludeTypes = new List<Type>()
	{
        //[DGR] No support added yet
		/*
        typeof(UIRect),
		typeof(UITweener),
		typeof(UICamera),
		typeof(UIPanel),
		typeof(UIScrollView),
		typeof(UIRoot),
		typeof(UIWidgetContainer),
		typeof(UICenterOnChild),
		typeof(UIPlayTween),
        */
		typeof(ActiveMonobehaviourProfiler),
	};

	private void Awake()
	{

#if PRODUCTION || PREPRODUCTION
		Destroy(gameObject);
#endif

		DontDestroyOnLoad(gameObject);
	}

	// Update is called once per frame
	private void Update()
	{
		m_timer += Time.unscaledDeltaTime;

		if(m_timer >= UPDATE_INTERVAL)
		{
			m_timer = 0;

			FindPositions();
		}

#if UNITY_EDITOR
		if(Input.GetKeyUp(KeyCode.F3))
		{
			WriteToFile();
		}
#endif
	}

	private void FindPositions()
	{
		m_scripts.Clear();

		MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();

		for(int i = 0;i < scripts.Length;i++)
		{
			Type t = scripts[i].GetType();

			Type baseType = GetDeepestBaseType(t);
			// Hack to not log UnityEngine.Analytics.GameObserver as it's protected and we cannot get its type
			if(!m_excludeTypes.Contains(t) && !m_excludeTypes.Contains(baseType) && t.ToString() != "UnityEngine.Analytics.GameObserver")
            {
				m_scripts.Add(new ScriptProfilerInfo(scripts[i].transform.position, scripts[i].GetType().ToString()));
			}
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Color prevColor = Gizmos.color;	

		for(int i = 0;i < m_scripts.Count;i++)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(m_scripts[i].m_position, GIZMO_SIZE);

			Gizmos.color = Color.white;
			Handles.Label(new Vector3(m_scripts[i].m_position.x - GIZMO_SIZE, m_scripts[i].m_position.y + GIZMO_SIZE * 1.2f, m_scripts[i].m_position.z), m_scripts[i].m_type);
		}

		Gizmos.color = prevColor;
	}
#endif

	private Type GetDeepestBaseType(Type type)
	{
		// null does not have base type
		if(type == null)
		{
			return null;
		}

		// check all base types
		var currentType = type;
		var returnType = currentType;
		while(currentType != null && currentType != typeof(MonoBehaviour))
		{
			returnType = currentType;
			currentType = currentType.BaseType;
		}

		return returnType;
	}

#if UNITY_EDITOR
	private void WriteToFile()
	{
		string pathToSave = EditorUtility.SaveFilePanel("Write to file", "", "Result", "csv");
        if(pathToSave.Length > 0)
		{
			StringBuilder sBuilder = new StringBuilder();
			sBuilder.AppendLine("Script name, Attached to object, Position, Update allocations(KB), Fixed update allocations(KB), Late update allocations(KB), Update duration(ms), Fixed update duration(ms), Late update duration(ms)");

			MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();

			//Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

			for(int i = 0; i < scripts.Length; i++)
			{
				/*
				if(!GeometryUtility.TestPlanesAABB(planes, new Bounds(scripts[i].transform.position, new Vector3(0.2f, 0.2f, 0.2f))))
				{
					continue;
				}
				*/

				EditorUtility.DisplayProgressBar("Processing", string.Format("{0}/{1}", i, scripts.Length), (float)i / scripts.Length);

				if(!scripts[i].isActiveAndEnabled || !scripts[i].gameObject.activeSelf)
				{
					continue;
				}

				Type t = scripts[i].GetType();
				Type baseType = GetDeepestBaseType(t);
				if(m_excludeTypes.Contains(t) || m_excludeTypes.Contains(baseType) || t.ToString() == "UnityEngine.Analytics.GameObserver")
				{
					continue;
				}

				// Calculate update
				double updateDuration = 0;
				float updateAllocations = 0;
				CalculateTimeAndAllocationsForMethod(scripts[i], "Update", out updateDuration, out updateAllocations);

				// Calculate fixed update
				double fixedUpdateDuration = 0;
				float fixedUpdateAllocations = 0;
				CalculateTimeAndAllocationsForMethod(scripts[i], "FixedUpdate", out fixedUpdateDuration, out fixedUpdateAllocations);

				// Calculate late update
				double lateUpdateDuration = 0;
				float lateUpdateAllocations = 0;
				CalculateTimeAndAllocationsForMethod(scripts[i], "LateUpdate", out lateUpdateDuration, out lateUpdateAllocations);

				string position = string.Format("{0}_{1}_{2}", scripts[i].transform.position.x, scripts[i].transform.position.y, scripts[i].transform.position.z);

				string parent = "NONE";
				if(scripts[i].gameObject != null)
				{
					parent = scripts[i].gameObject.name;
                }

				sBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6:0.00},{7:0.00},{8:0.00}", scripts[i].GetType().ToString(), parent, position, 
					updateAllocations, fixedUpdateAllocations, lateUpdateDuration, updateDuration, fixedUpdateDuration, lateUpdateDuration));
			}

			System.IO.File.WriteAllText(pathToSave, sBuilder.ToString());

			EditorUtility.ClearProgressBar();
        }
	}

	private void CalculateTimeAndAllocationsForMethod(MonoBehaviour script, string method, out double time, out float memory)
	{
		System.Reflection.MethodInfo methodInfo = script.GetType().GetMethod(method, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		if(methodInfo == null)
		{
			time = 0;
			memory = 0;
			return;
		}

		DateTime startProcessTime = DateTime.UtcNow;
		uint prevHeap = UnityEngine.Profiler.usedHeapSize;			
		script.SendMessage(method);
		memory = (float)(UnityEngine.Profiler.usedHeapSize - prevHeap) / 1000;
		time = (DateTime.UtcNow - startProcessTime).TotalMilliseconds;
	}
#endif
}

using System.Collections;
using UnityEngine;

class Coroutine : MonoBehaviour
{
	static Coroutine m_runner;

	public static void Start(IEnumerator routine)
	{
		if (m_runner == null)
		{
			m_runner = new GameObject { hideFlags = HideFlags.HideAndDontSave }.AddComponent<Coroutine>();
			DontDestroyOnLoad(m_runner);
		}

		m_runner.StartCoroutine(routine);
	}
}
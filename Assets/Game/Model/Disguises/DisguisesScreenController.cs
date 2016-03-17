using UnityEngine;
using System.Collections;

public class DisguisesScreenController : MonoBehaviour {



	[SerializeField] RectTransform m_layout;


	private DisguisePill[] m_disguises;


	// Use this for initialization
	void Awake() {
		m_disguises = new DisguisePill[9];
		GameObject prefab = Resources.Load<GameObject>("UI/Popups/Disguises/PF_DisguisesPill");

		// Test
		for (int i = 0; i < 9; i++) {
			GameObject pill = GameObject.Instantiate<GameObject>(prefab);
			pill.transform.parent = m_layout;
			pill.transform.localScale = Vector3.one;

			m_disguises[i] = pill.GetComponent<DisguisePill>();
		}
	}
}

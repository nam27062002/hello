using UnityEngine;
using System.Collections;

public class MenuDragonLoader : MonoBehaviour {

	[SerializeField] private string m_dragonMenuPrefab;

	public void LoadDragonPreview() {
		foreach (Transform child in transform) {
			GameObject.DestroyImmediate(child.gameObject);
		}

		GameObject dragonRef = Resources.Load<GameObject>("UI/Menu/Dragons/" + m_dragonMenuPrefab);

		if (dragonRef != null) {
			GameObject dragonInstance = GameObject.Instantiate(dragonRef);
			dragonInstance.transform.parent = transform;
			dragonInstance.transform.localPosition = Vector3.zero;
			dragonInstance.transform.localScale = Vector3.one;
			dragonInstance.transform.localRotation = Quaternion.identity;
		}
	}
}

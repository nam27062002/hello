using UnityEngine;
using System.Collections;

public class MenuDragonLoader : MonoBehaviour {

	[SerializeField] private string m_dragonMenuPrefab;

	private GameObject m_dragonInstance = null;
	public GameObject dragonInstance {
		get { return m_dragonInstance; }
	}

	/// <summary>
	/// Loads the preview of the target dragon.
	/// </summary>
	/// <param name="_dragonSku">Dragon sku. If empty, the preview defined in Unity's inspector will be loaded instead.</param>
	public void LoadDragonPreview(string _dragonSku = "") {
		// Destroy any previously loaded dragons
		UnloadDragonPreview();

		// Load dragon prefab
		string path = "UI/Menu/Dragons/" + m_dragonMenuPrefab;
		if(!string.IsNullOrEmpty(_dragonSku)) {
			DefinitionNode dragonDef = DefinitionsManager.GetDefinition(DefinitionsCategory.DRAGONS, _dragonSku);
			if(dragonDef == null) {
				Debug.LogError("Couldn't find definition for dragon with sku " + _dragonSku);
				return;
			}
			path = dragonDef.GetAsString("menuPrefab");
		}
		GameObject dragonPrefab = Resources.Load<GameObject>(path);

		// Instantiate a copy as child of this object
		if(dragonPrefab != null) {
			m_dragonInstance = GameObject.Instantiate(dragonPrefab);
			m_dragonInstance.transform.SetParentAndReset(this.transform);
			m_dragonInstance.SetLayerRecursively(this.gameObject.layer);
		}
	}

	/// <summary>
	/// Destroy current loaded dragon, if any.
	/// </summary>
	public void UnloadDragonPreview() {
		// Just make sure the object doesn't have anything attached
		foreach(Transform child in transform) {
			GameObject.DestroyImmediate(child.gameObject);
			m_dragonInstance = null;
		}
	}
}

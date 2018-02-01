using UnityEngine;

public class DestroyInSeconds : MonoBehaviour {

	[SerializeField] private float m_lifeTime = 1f;
	public float lifeTime {
		get { return m_lifeTime; }
		set { m_lifeTime = value; }
	}

	[Tooltip("Optionally replace with the given prefab before destroying, cloning transform and name properties. Full prefab path from resources.")]
	[FileListAttribute("Resources", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_replacementPrefab = "";
		
    void Awake() {
        // If it has to be destroyed immediately (typically because this game object is a placeholder object used in edit mode)
        // then it's done as soon as possible in order to prevent other objects retrieving components from
        // getting components in this game object.
        if (m_lifeTime <= 0f) {
			SelfDestroy();
        }
    }

	void Update() {
        if (m_lifeTime > 0f) {
            m_lifeTime -= Time.deltaTime;
            if (m_lifeTime < 0f)
				SelfDestroy();
        }
	}

	private void SelfDestroy() {
		// Must be replaced by another prefab?
		if(!string.IsNullOrEmpty(m_replacementPrefab)) {
			GameObject prefab = Resources.Load<GameObject>(m_replacementPrefab);
			if(prefab != null) {
				// Instantiate and copy transform and layer
				GameObject instance = GameObject.Instantiate<GameObject>(prefab, this.transform.parent, false);
				instance.transform.CopyFrom(this.transform);
				instance.layer = this.gameObject.layer;
			}
		}

		// Do the actual destruction
		DestroyObject(this.gameObject);
	}
}

//--------------------------------------------------------------------------------
// AutoParenter
//--------------------------------------------------------------------------------
// This re-parents an object on Awake() and then destroys itself.  This is used
// for things that want to be attached to a sub object in a character's hierarchy
// but to avoid problems with prefabs updating, it is placed at the root and then
// re-parented at runtime.
//--------------------------------------------------------------------------------
using UnityEngine;

public class AutoParenter : MonoBehaviour {
	
	[SerializeField] private string m_parentName;

	void Awake() {
		if (!string.IsNullOrEmpty(m_parentName)) {
			Transform t = transform;
			Transform p = t.root.FindTransformRecursive(m_parentName);

			if (p == null) {
                string parentObjName = transform.name;

				DragonPlayer dragon = GetComponentInParent<DragonPlayer>();

				if (dragon != null) {
					parentObjName = dragon.data.def.sku;
                }
                Debug.LogError(string.Format("Can't find transform for {0} on object {1}", m_parentName, parentObjName));
			} else {
				t.parent = p;
			}
		}

		Destroy(this);
	}

}

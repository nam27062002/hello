//--------------------------------------------------------------------------------
// AutoParenter
//--------------------------------------------------------------------------------
// This re-parents an object on Awake() and then destroys itself.  This is used
// for things that want to be attached to a sub object in a character's hierarchy
// but to avoid problems with prefabs updating, it is placed at the root and then
// re-parented at runtime.
//--------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;

public class AutoParenter : MonoBehaviour {
	
	[SerializeField] private string m_parentName;
	public string parentName
	{
		get{ return m_parentName; }
	}
	[FormerlySerializedAs("m_lookAtRoot")]
	[SerializeField] private Transform m_parentRoot;
	[SerializeField] private bool m_worldPositionStays = true;

	void Awake() {
		if (!string.IsNullOrEmpty(m_parentName)) {
			Transform t = transform;
			Transform p;
			if (m_parentRoot == null)
				p = t.parent.FindTransformRecursive(m_parentName);
			else
				p = m_parentRoot.FindTransformRecursive(m_parentName);

			if (p == null) {
                string parentObjName = t.name;
                Debug.LogError(string.Format("Can't find transform for {0} on object {1}", m_parentName, parentObjName));
			} else {
				t.SetParent(p, m_worldPositionStays);
			}
		}

		Destroy(this);
	}



}

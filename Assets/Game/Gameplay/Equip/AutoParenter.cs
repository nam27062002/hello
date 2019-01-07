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
	
	[SerializeField] private string m_parentName = "";
	public string parentName
	{
		get{ return m_parentName; }
	}
	[FormerlySerializedAs("m_lookAtRoot")]
	[SerializeField] private Transform m_parentRoot;
	public Transform parentRoot
	{
		get{ return m_parentRoot; }
        set{ m_parentRoot = value; }
	}
	[SerializeField] private bool m_worldPositionStays = true;
	[SerializeField] private bool m_resetScale = false;
    public enum When
    {
        AWAKE,
        START,
		MANUAL
    };
    [SerializeField] private When m_when = When.AWAKE;
	public When when { get { return m_when; }}

	void Awake() {
        if (m_when == When.AWAKE)
            Reparent();
	}
    
    void Start() {
        if (m_when == When.START)
            Reparent();
    }
    
    public void Reparent()
    {
        if (!string.IsNullOrEmpty(m_parentName)) {
            Transform t = transform;
            Transform p = GetNewParent();
            if (p == null) {
                if (FeatureSettingsManager.IsDebugEnabled) {
                    string parentObjName = t.name;
                    Debug.LogWarning(string.Format("Can't find transform for {0} on object {1}", m_parentName, parentObjName));
                }
            } else {
                t.SetParent(p, m_worldPositionStays);
                if (m_resetScale) {
                    t.localScale = GameConstants.Vector3.one;
                }
            }
        }

        Destroy(this);
    }

	public void CopyTargetPosAndRot()
	{
		if (!string.IsNullOrEmpty(parentName)) {
			Transform t = transform;
			Transform p = GetNewParent();
			if (p != null) {
				t.position = p.position;
				t.rotation = p.rotation;
			} 
		}
	}

	private Transform GetNewParent() {
		Transform p = null;
		if(m_parentRoot == null) {
			p = this.transform.parent.FindTransformRecursive(m_parentName);
		} else if(string.IsNullOrEmpty(m_parentName)) {
			p = m_parentRoot;
		} else {
			p = m_parentRoot.FindTransformRecursive(m_parentName);
		}
		return p;
	}





}

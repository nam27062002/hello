using UnityEngine;

[ExecuteInEditMode]
public class HungryLettersPlaceholderEditor : MonoBehaviour
{
#if UNITY_EDITOR
	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------
	[SerializeField]
	private HungryLettersManager.Difficulties m_gizmo = HungryLettersManager.Difficulties.Easy;
	public HungryLettersManager.Difficulties difficulty{ get{ return m_gizmo; } }

	[SerializeField]
	private bool m_showGizmo = true;


    //------------------------------------------------------------
    // Private Variables:
    //------------------------------------------------------------
    private static readonly string Path_Easy_Letter = "HungryLetters/H_Easy.png";
    private static readonly string Path_Normal_Letter = "HungryLetters/H_Normal.png";
    private static readonly string Path_Hard_Letter = "HungryLetters/H_Hard.png";

	private Transform m_transform;
	

	//------------------------------------------------------------
	// Untiy Lifecycle:
	//------------------------------------------------------------
	protected void Awake() {
		m_transform = transform;	
	}

    void OnDrawGizmos() {
        if (m_showGizmo) {
            switch (m_gizmo) {
                case HungryLettersManager.Difficulties.Easy:    Gizmos.DrawIcon(m_transform.position, Path_Easy_Letter, false);   break;
                case HungryLettersManager.Difficulties.Normal:  Gizmos.DrawIcon(m_transform.position, Path_Normal_Letter, false); break;
                case HungryLettersManager.Difficulties.Hard:    Gizmos.DrawIcon(m_transform.position, Path_Hard_Letter, false);   break;
            }
		}
	}
#endif
}
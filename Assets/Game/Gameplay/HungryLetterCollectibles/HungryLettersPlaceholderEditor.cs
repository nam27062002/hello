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

	private Transform m_transform;
	private Vector3 m_position;
	private string m_currentIcon = "";

	//------------------------------------------------------------
	// Untiy Lifecycle:
	//------------------------------------------------------------

	protected void Awake()
	{
		m_transform = transform;
		m_currentIcon = m_gizmo.ToString();
	}

	protected void LateUpdate()
	{
		m_position = m_transform.position;
		if(!m_currentIcon.Equals(m_gizmo.ToString()))
		{
			m_currentIcon = m_gizmo.ToString();
		}
	}

	void OnDrawGizmos()
	{
		if(m_showGizmo)
		{
			Gizmos.DrawIcon(m_position, "HungryLetters/H_" + m_currentIcon + ".png", false);
		}
	}
#endif
}
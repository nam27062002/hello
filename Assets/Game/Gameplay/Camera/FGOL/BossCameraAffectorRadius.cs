//#define SHOW_DEBUG_GIZMOS

using UnityEngine;

public class BossCameraAffectorRadius : MonoBehaviour
{
	//--------------------------------------------------------
	// Inspector Variables:
	//--------------------------------------------------------

	[SerializeField]
	private BossCameraAffector m_bca;
	[SerializeField]
	private SphereCollider m_collider;

	//--------------------------------------------------------
	// Inspector Variables:
	//--------------------------------------------------------

	private bool m_notified;

	//--------------------------------------------------------
	// Unity Lifecycle:
	//--------------------------------------------------------

	public void Start()
	{
		DebugUtils.Assert(m_bca != null, "If there is not a BossCameraAffector connected to this gameobject you probably don't want a BossCameraAffectorRadius component among its children !!   ;-)");
		DebugUtils.Assert(m_collider != null, "No collider");

		if(m_bca.permanentlyDisabled)
		{
			// if the performance of the device are not enough, disable everything.
			enabled = m_collider.enabled = false;
		}
		else
		{
			gameObject.layer = LayerMask.NameToLayer("Triggers");
			m_collider.enabled = true;
			m_collider.isTrigger = true;
			// the lossyscale is because we don't want the value set by the designers to be affected by the scale of the parent transform.
			m_collider.radius = m_bca.radius / transform.lossyScale.x;
			// at this point the boss camera affector it's just a variables holder...
			m_bca.enabled = false;
		}
	}

	protected void OnDisable()
	{
		RemoveBossCam();
	}

	protected void OnDestroy()
	{
		RemoveBossCam();
	}

	protected void OnTriggerEnter()
	{
		// leaving this check here to be absolutely sure that nothing will go wrong...
		if(!m_bca.permanentlyDisabled)
		{
			NotifyBossCam();
        }
	}

	protected void OnTriggerExit()
	{
		RemoveBossCam();
	}

	//--------------------------------------------------------
	// Private Methods:
	//--------------------------------------------------------

	private void NotifyBossCam()
	{
		InstanceManager.gameCamera.NotifyBoss(m_bca);
		m_notified = true;
	}

	private void RemoveBossCam()
	{
		if(enabled && m_notified)
		{
			// make this check because it could happen that some entities (like treasure chests) could be destroyed/disabled
			// before to enter in the level, so, before the camera to be instantiated.
			if(InstanceManager.gameCamera != null)
			{
				InstanceManager.gameCamera.RemoveBoss(m_bca);
			}
			m_notified = false;
		}
	}

#if UNITY_EDITOR && SHOW_DEBUG_GIZMOS
	private Color m_solidDiscColor = new Color(1f, 0f, 0f, 0.1f);
	private Color m_labelColor = new Color(1f, 0f, 0f, 0.7f);
	private Transform m_transform;
	private GUIStyle m_textStyle;

	void OnDrawGizmos()
	{
		UnityEditor.Handles.color = m_solidDiscColor;
		if(m_transform == null)
		{
			m_transform = transform;
		}
		Vector3 pos = m_transform.position;
		UnityEditor.Handles.DrawSolidDisc(pos, Vector3.back, m_collider.radius);
		pos.y = pos.y + m_collider.radius - 5f;
		if(m_textStyle == null)
		{
			m_textStyle = new GUIStyle(GUI.skin.GetStyle("label"));
			m_textStyle.normal.textColor = m_labelColor;
			m_textStyle.alignment = TextAnchor.MiddleCenter;
		}
		UnityEditor.Handles.Label(pos, "BossCamAffectorRadius");
	}
#endif
}
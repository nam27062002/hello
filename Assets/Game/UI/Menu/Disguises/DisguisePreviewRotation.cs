using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DisguisePreviewRotation : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	[Comment("Degrees per second")]
	[SerializeField] float m_autoRotationSpeed = 15f;

	[Comment("Degrees per drag unit", 3)]
	[SerializeField] float m_manualRotationSpeed = 0.5f;

	private Transform m_dragonWorldTransform;
	private float m_pointerX;
	private float m_angle;
	private float m_defaultAngle = 0f;
	private bool m_dragging = false;

	// Use this for initialization
	void Awake () {
		// find the 3D dragon position
		GameObject disguiseScene = GameObject.Find("PF_MenuDisguisesScene");
		if (disguiseScene != null) {
			m_dragonWorldTransform = disguiseScene.transform.FindChild("CurrentDragon");
			if(m_dragonWorldTransform != null) {
				m_defaultAngle = m_dragonWorldTransform.localRotation.eulerAngles.y;
			}
		}
	}

	private void OnEnable() {
		// Reset rotation angle
		m_angle = m_defaultAngle;
		SetAngle(0f);
	}
	
	// Update is called once per frame
	void Update () {
		// [AOC] Auto-rotate - not while dragging!
		if(m_dragonWorldTransform && !m_dragging) {
			m_angle = (m_angle + Time.deltaTime * m_autoRotationSpeed) % 360f;
			SetAngle(0f);
		}
	}

	/// <summary>
	/// The input has started dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnBeginDrag(PointerEventData _event) {
		m_dragging = true;
		if (m_dragonWorldTransform) {
			m_pointerX = _event.position.x;
			m_angle = m_dragonWorldTransform.rotation.eulerAngles.y;
		}
	}

	/// <summary>
	/// The input is dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event.</param>
	public void OnDrag(PointerEventData _event) {
		SetAngle(m_pointerX - _event.position.x);
	}

	public void OnEndDrag(PointerEventData _event) {
		m_dragging = false;
		SetAngle(m_pointerX - _event.position.x);
		m_angle = m_dragonWorldTransform.rotation.eulerAngles.y;
	}

	private void SetAngle(float _deltaDegrees) {
		if(m_dragonWorldTransform) {
			m_dragonWorldTransform.rotation = Quaternion.Euler(new Vector3(0, m_angle + _deltaDegrees, 0));
		}
	}
}

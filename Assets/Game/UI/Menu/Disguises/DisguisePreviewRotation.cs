using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DisguisePreviewRotation : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	private Transform m_dragonWorldTransform;
	private float m_pointerX;
	private float m_angle;

	// Use this for initialization
	void Start () {
		// find the 3D dragon position
		GameObject disguiseScene = GameObject.Find("PF_MenuDisguisesScene");
		if (disguiseScene != null) {
			m_dragonWorldTransform = disguiseScene.transform.FindChild("CurrentDragon");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// The input has started dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnBeginDrag(PointerEventData _event) {
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
		SetAngle(m_pointerX - _event.position.x);
	}

	private void SetAngle(float _deltaX) {
		if (m_dragonWorldTransform) {
			float delta = _deltaX;
			m_dragonWorldTransform.rotation = Quaternion.Euler(new Vector3(0, m_angle + delta, 0));
		}
	}
}

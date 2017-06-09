using UnityEngine;
using System.Collections;

public class CollisionEventForwarding : MonoBehaviour {

	[SerializeField] private GameObject m_receiver;
	[SeparatorAttribute]
	[SerializeField] private bool m_onCollisionEnter = false;
	[SerializeField] private bool m_onCollisionStay = false;
	[SerializeField] private bool m_onCollisionExit = false;
	[SeparatorAttribute]
	[SerializeField] private bool m_onTriggerEnter = false;
	[SerializeField] private bool m_onTriggerStay = false;
	[SerializeField] private bool m_onTriggerExit = false;
	[SeparatorAttribute]
	[SerializeField] private bool m_onMouseDown = false;
	[SerializeField] private bool m_onMouseDrag = false;
	[SerializeField] private bool m_onMouseUp = false;
	[SerializeField] private bool m_onMouseUpAsButton = false;
	[SeparatorAttribute]
	[SerializeField] private bool m_onMouseEnter = false;
	[SerializeField] private bool m_onMouseExit = false;
	[SerializeField] private bool m_onMouseOver = false;
		
	void OnCollisionEnter(Collision _other) { if (m_onCollisionEnter) 	m_receiver.SendMessage("OnCollisionEnter", _other); 	} 
	void OnCollisionStay(Collision _other) 	{ if (m_onCollisionStay) 	m_receiver.SendMessage("OnCollisionStay", _other); 		}
	void OnCollisionExit(Collision _other) 	{ if (m_onCollisionExit) 	m_receiver.SendMessage("OnCollisionExit", _other);		}

	void OnTriggerEnter(Collider _other) 	{ if (m_onTriggerEnter) m_receiver.SendMessage("OnTriggerEnter", _other); 	} 
	void OnTriggerStay(Collider _other) 	{ if (m_onTriggerStay) 	m_receiver.SendMessage("OnTriggerStay", _other); 	}
	void OnTriggerExit(Collider _other) 	{ if (m_onTriggerExit) 	m_receiver.SendMessage("OnTriggerExit", _other);	}

	/*
	 * OnMouseDown	OnMouseDown is called when the user has pressed the mouse button while over the GUIElement or Collider.
OnMouseDrag	OnMouseDrag is called when the user has clicked on a GUIElement or Collider and is still holding down the mouse.
OnMouseEnter	Called when the mouse enters the GUIElement or Collider.
OnMouseExit	Called when the mouse is not any longer over the GUIElement or Collider.
OnMouseOver	Called every frame while the mouse is over the GUIElement or Collider.
OnMouseUp	OnMouseUp is called when the user has released the mouse button.
OnMouseUpAsButton
*/

	void OnMouseDown() 			{ if (m_onMouseDown) 		m_receiver.SendMessage("OnMouseDown");	}
	void OnMouseDrag() 			{ if (m_onMouseDrag) 		m_receiver.SendMessage("OnMouseDrag");	}
	void OnMouseUp() 			{ if (m_onMouseUp) 			m_receiver.SendMessage("OnMouseUp");	}
	void OnMouseUpAsButton() 	{ if (m_onMouseUpAsButton) 	m_receiver.SendMessage("OnMouseUpAsButton");	}

	void OnMouseEnter() { if (m_onMouseEnter) 	m_receiver.SendMessage("OnMouseEnter");	}
	void OnMouseExit() 	{ if (m_onMouseExit) 	m_receiver.SendMessage("OnMouseExit");	}
	void OnMouseOver() 	{ if (m_onMouseOver) 	m_receiver.SendMessage("OnMouseOver");	}
}

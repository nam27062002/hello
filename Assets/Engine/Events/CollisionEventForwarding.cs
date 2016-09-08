using UnityEngine;
using System.Collections;

public class CollisionEventForwarding : MonoBehaviour {

	[SerializeField] private GameObject m_receiver;
	[SeparatorAttribute]
	[SerializeField] private bool m_onCollisionEnter;
	[SerializeField] private bool m_onCollisionStay;
	[SerializeField] private bool m_onCollisionExit;
	[SeparatorAttribute]
	[SerializeField] private bool m_onTriggerEnter;
	[SerializeField] private bool m_onTriggerStay;
	[SerializeField] private bool m_onTriggerExit;
		
	void OnCollisionEnter(Collision _other) { if (m_onCollisionEnter) 	m_receiver.SendMessage("OnCollisionEnter", _other); 	} 
	void OnCollisionStay(Collision _other) 	{ if (m_onCollisionStay) 	m_receiver.SendMessage("OnCollisionStay", _other); 		}
	void OnCollisionExit(Collision _other) 	{ if (m_onCollisionExit) 	m_receiver.SendMessage("OnCollisionExit", _other);		}

	void OnTriggerEnter(Collider _other) 	{ if (m_onTriggerEnter) m_receiver.SendMessage("OnTriggerEnter", _other); 	} 
	void OnTriggerStay(Collider _other) 	{ if (m_onTriggerStay) 	m_receiver.SendMessage("OnTriggerStay", _other); 	}
	void OnTriggerExit(Collider _other) 	{ if (m_onTriggerExit) 	m_receiver.SendMessage("OnTriggerExit", _other);	}
}

using UnityEngine;
using System.Collections;

public class CollisionEventForwarding : MonoBehaviour {

	[SerializeField] private MonoBehaviour m_receiver;
	[SerializeField] private bool m_onCollisionEnter;
	[SerializeField] private bool m_onCollisionStay;
	[SerializeField] private bool m_onCollisionExit;
		
	void OnCollisionEnter(Collision _other) {
		if (m_onCollisionEnter) m_receiver.SendMessage("OnCollisionEnter", _other);
	}

	void OnCollisionStay(Collision _other) {
		if (m_onCollisionStay) m_receiver.SendMessage("OnCollisionStay", _other);
	}

	void OnCollisionExit(Collision _other) {
		if (m_onCollisionExit) m_receiver.SendMessage("OnCollisionExit", _other);
	}
}

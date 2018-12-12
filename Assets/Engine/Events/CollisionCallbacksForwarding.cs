using UnityEngine;

public abstract class CollisionCallbackReceiver : MonoBehaviour {
    public abstract void OnCollisionEnter(Collision _other);
    public abstract void OnCollisionStay(Collision _other);
    public abstract void OnCollisionExit(Collision _other);
}

public class CollisionCallbacksForwarding : MonoBehaviour {

    [SerializeField] private CollisionCallbackReceiver m_receiver = null;

    [SeparatorAttribute]
	[SerializeField] private bool m_onCollisionEnter = false;
	[SerializeField] private bool m_onCollisionStay = false;
	[SerializeField] private bool m_onCollisionExit = false;
	

    void OnCollisionEnter(Collision _other) { if (m_onCollisionEnter) 	m_receiver.OnCollisionEnter(_other);    } 
    void OnCollisionStay(Collision _other) 	{ if (m_onCollisionStay) 	m_receiver.OnCollisionStay(_other); 	}
    void OnCollisionExit(Collision _other) 	{ if (m_onCollisionExit) 	m_receiver.OnCollisionExit(_other);		}
}
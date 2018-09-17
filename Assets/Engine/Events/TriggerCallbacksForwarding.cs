using UnityEngine;

public abstract class TriggerCallbackReceiver : MonoBehaviour {
    public abstract void OnTriggerEnter(Collider _other);
    public abstract void OnTriggerStay(Collider _other);
    public abstract void OnTriggerExit(Collider _other);   
}

public class TriggerCallbacksForwarding : MonoBehaviour {

    [SerializeField] private TriggerCallbackReceiver m_receiver;

	[SeparatorAttribute]
	[SerializeField] private bool m_onTriggerEnter = false;
	[SerializeField] private bool m_onTriggerStay = false;
	[SerializeField] private bool m_onTriggerExit = false;

	
    void OnTriggerEnter(Collider _other) 	{ if (m_onTriggerEnter) m_receiver.OnTriggerEnter(_other); 	} 
    void OnTriggerStay(Collider _other) 	{ if (m_onTriggerStay) 	m_receiver.OnTriggerStay(_other); 	}
    void OnTriggerExit(Collider _other) 	{ if (m_onTriggerExit) 	m_receiver.OnTriggerExit(_other);	}
}
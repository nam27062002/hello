using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonDiveToggle : MonoBehaviour {

	private bool m_enabled;
	private DragonMotion m_motionComponent;
	
	void Start() {
		if (InstanceManager.player != null) {
			m_motionComponent = InstanceManager.player.GetComponent<DragonMotion>();
		}
		OnToggleChange( DebugSettings.dive );
	}
	
	public void OnToggleChange(bool _value) {
		if (m_motionComponent != null) 
		{
			m_motionComponent.canDive = _value;
			m_enabled = _value;
		}
	}
}

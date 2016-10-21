using UnityEngine;
using System.Collections;

public class AttachPoint : MonoBehaviour {
	[SerializeField] private Equipable.AttachPoint m_point;
	public Equipable.AttachPoint point { get { return m_point; } }

	private Equipable m_item;


	public void Equip(Equipable _item) {
		// store the item related to this Attach Point
		m_item = _item;

		// check type of item
		// use correct method to equip this item
		switch (m_item.type) { 
			case Equipable.Type.Pet:		EquipPet(); 		break;
			case Equipable.Type.Accessory:	EquipAccessory();	break;
		}
	}

	public void Unequip() {
		
	}


	//----------------------------------------------------------//


	private void EquipPet() {
		m_item.transform.position = transform.position;
		AI.AIPilot pilot = m_item.GetComponent<AI.AIPilot>();
		m_item.transform.position = transform.position;
		pilot.homeTransform = transform;
		pilot.Spawn(null);

		ISpawnable[] components = pilot.GetComponents<ISpawnable>();
		foreach (ISpawnable component in components) {
			if (component != pilot ) {
				component.Spawn(null);
			}
		}


	}

	private void EquipAccessory() {
		m_item.transform.parent = transform;
		m_item.transform.localPosition = Vector3.zero;
	}
}
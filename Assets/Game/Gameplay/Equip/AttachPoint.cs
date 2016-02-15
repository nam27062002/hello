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
		if (m_item.type == Equipable.Type.Pet) {
			EquipPet();
		}

	}

	public void Unequip() {
		
	}


	//----------------------------------------------------------//


	private void EquipPet() {
		Initializable[] toInit = m_item.GetComponents<Initializable>();
		m_item.transform.position = transform.position;

		for (int i = 0; i < toInit.Length; i++) {
			toInit[i].Initialize();
		}
	}
}
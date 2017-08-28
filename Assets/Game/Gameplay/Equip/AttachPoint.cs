﻿using UnityEngine;
using System.Collections;

public class AttachPoint : MonoBehaviour {
	[SerializeField] private Equipable.AttachPoint m_point;
	public Equipable.AttachPoint point { 
		get { return m_point; } 
	}

	private Equipable m_item;
	public Equipable item {
		get { return m_item; }
	}


	public void Unequip(bool _destroyItem) {
		if(m_item == null) return;

		if(_destroyItem) {
			GameObject.Destroy(m_item.gameObject);
		}
		m_item = null;
	}


	//----------------------------------------------------------//


	public void EquipPet(Equipable _pet) {
		m_item = _pet;
		m_item.transform.position = transform.position;

		AI.IMachine machine = m_item.GetComponent<AI.IMachine>();
		if(machine != null) {
			machine.Spawn(null);
		}

		AI.AIPilot pilot = m_item.GetComponent<AI.AIPilot>();
		if(pilot != null) {
			pilot.homeTransform = transform;
			pilot.Spawn(null);

			ISpawnable[] components = pilot.GetComponents<ISpawnable>();
			foreach (ISpawnable component in components) {
				if (component != pilot && component != machine) {
					component.Spawn(null);
				}
			}
		}
	}

	public void EquipAccessory( Equipable _accesory, DragonEquip _dragonEquip ) {
		m_item = _accesory;
		m_item.transform.parent = transform;
		m_item.transform.localPosition = Vector3.zero;
		m_item.transform.localScale = Vector3.one;
		m_item.transform.localRotation = Quaternion.identity;
		ScaleEquipableParticle scaler = _accesory.GetComponentInChildren<ScaleEquipableParticle>();
		if ( scaler != null )
		{
			scaler.Setup();
		}
	}
}
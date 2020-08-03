using UnityEngine;
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
		if (m_item == null) return;

		if (_destroyItem) {
			if (Application.isPlaying) {
				GameObject.Destroy(m_item.gameObject);
			} else {
				GameObject.DestroyImmediate(m_item.gameObject);
			}
		}
		m_item = null;
	}


	//----------------------------------------------------------//


	public void EquipPet(Equipable _pet) {
		m_item = _pet;
		m_item.gameObject.SetActive(true);
        _pet.attachPoint = point;
		// m_item.transform.position = transform.position;

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

	public void EquipAccessory( Equipable _accesory) {
		m_item = _accesory;
		m_item.transform.parent = transform;
		m_item.transform.localPosition = Vector3.zero;
		m_item.transform.localScale = Vector3.one;
		m_item.transform.localRotation = Quaternion.identity;
		m_item.gameObject.SetLayerRecursively(transform.gameObject.layer);
	}

	public void EquipAccessory( Equipable _accesory, Vector3 _position, Vector3 _scale, Vector3 _rotation) {
		m_item = _accesory;
		m_item.transform.parent = transform;
		m_item.transform.localPosition = _position;
		m_item.transform.localScale = _scale;
		m_item.transform.localRotation = Quaternion.Euler(_rotation);
		m_item.gameObject.SetLayerRecursively(transform.gameObject.layer);
	}

	public void HideAccessory()
	{
		if ( m_item != null )
			m_item.gameObject.SetActive(false);
	}
}
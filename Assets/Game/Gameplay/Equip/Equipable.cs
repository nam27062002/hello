﻿using UnityEngine;
using System.Collections;

public class Equipable : MonoBehaviour {

	public 
	enum Type {
		Pet = 0,
		Skin,
		Accessory
	};

	public
	enum Slot {
		Pet = 0,
		Texture,
		Head,
		Tail,
		Wing
	};

	public
	enum AttachPoint {
		Skin = 0,
		Pet_1,
		Pet_2,
		Pet_3,
		Pet_4,
		Pet_5,
		Head_1
	};

	//---------------------------------------------------------------------//

	[SerializeField] private Type m_type;
	public Type type { get { return m_type; } }


}

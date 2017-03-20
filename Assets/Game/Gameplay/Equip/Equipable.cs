using UnityEngine;
using System.Collections;

public class Equipable : MonoBehaviour {
	public
	enum AttachPoint {
		Pet_1,
		Pet_2,
		Pet_3,
		Pet_4,
		Pet_5,
		Head_1,
		Neck_1,
		Mouth_1,

		Count
	};

	//---------------------------------------------------------------------//

	[SerializeField] private AttachPoint m_attachPoint;
	public AttachPoint attachPoint { get { return m_attachPoint; } }


}

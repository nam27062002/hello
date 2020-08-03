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
		Chest_1,
		Spine_1,
		Tail_1,
		Tail_2,
		Right_Hand_1,
		R_Shoulder,
		L_Shoulder,
		R_Leg,
		L_Leg,
		Head_2,
		Left_Hand_1,
		Horn_1,
		Horn_2,
		Neck_2,
		Hip_1,
		Neck_3,
		Forearm_1,
		Forearm_2,
		Extra_1,
		Extra_2,
		Extra_3,
		Count
	};

	//---------------------------------------------------------------------//

	[SerializeField] private AttachPoint m_attachPoint;
    public AttachPoint attachPoint { get { return m_attachPoint; } set { m_attachPoint = value; } }

    [SerializeField][EquipableSkuList(true)] private string m_sku = "";
    public string sku { get { return m_sku; } }

    [SerializeField] private bool m_showOnResults = true;
	public bool showOnResults{ get{ return m_showOnResults; } }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipableChain : MonoBehaviour {

	public GameObject m_ChainPrefab;
	public Rigidbody m_ChainAnchor;
	public string m_ChainLinkName;

	public void Start()
	{
		GameObject go = Instantiate<GameObject>(m_ChainPrefab);
		go.transform.position = m_ChainAnchor.transform.position;
		go.transform.rotation = m_ChainAnchor.transform.rotation;
		go.transform.localScale = m_ChainAnchor.transform.lossyScale;

		Transform link = go.transform.Find(m_ChainLinkName);
		link.GetComponent<ConfigurableJoint>().connectedBody = m_ChainAnchor;
		link.transform.position = m_ChainAnchor.transform.position;
		link.transform.rotation = m_ChainAnchor.transform.rotation;
		ConfigurableJoint[] joints = go.transform.GetComponentsInChildren<ConfigurableJoint>();
		for (int i = 0; i < joints.Length; ++i) {
			joints[i].anchor = joints[i].anchor * m_ChainAnchor.transform.lossyScale.x;
		}

	}
	
}

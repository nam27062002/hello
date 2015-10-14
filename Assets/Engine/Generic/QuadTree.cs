using UnityEngine;
using System.Collections.Generic;

public class QuadTree {

	public const uint MAX_ELEMENTS = 4; 
	public const uint MAX_DEPTH = 4;


	private QuadTreeNode m_root;


	Dictionary<Transform, QuadTreeNode> m_indexTable;


	///

	public QuadTree(float _x, float _y, float _w, float _h) {

		m_root = new QuadTreeNode();
		m_root.Init(0, null, _x, _y, _w, _h);

		m_indexTable = new Dictionary<Transform, QuadTreeNode>();
	}

	public void insert(Transform _item) {
		m_root.Insert(_item, ref m_indexTable);
	}

	public void update(Transform _item) {

	}

	public void remove(Transform _item) {

		if (m_indexTable.ContainsKey(_item)) {

			QuadTreeNode node = m_indexTable[_item];

			if (node.Contains(_item.position.x, _item.position.y)) {

			}
		}
	}
	/*
	public List<Transform> getDataInRange(float _x, float _y, float _w, float _h);

	private void preOrderGetNodes(quadtree::Node* _node, std::vector<const quadtree::Node*>& _nodes);
	private void preOrderInRange(quadtree::Node* _node, float _x, float _y, float _w, float _h, std::vector<Data*>& _item);
*/

	// Debug
	public void DrawGizmos() {

		Gizmos.color = Color.gray;

		Gizmos.DrawWireCube(m_root.bounds.center, m_root.bounds.size);
	}
}

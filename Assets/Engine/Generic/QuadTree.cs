using UnityEngine;
using System.Collections.Generic;

public class QuadTree {

	public const uint MAX_ELEMENTS = 4;
	public const uint MAX_DEPTH = 6;

	private QuadTreeNode m_root;
	private List<Transform> m_items;
	private List<QuadTreeNode> m_nodes;
	private Dictionary<Transform, QuadTreeNode> m_indexTable;

	///

	public QuadTree(float _x, float _y, float _w, float _h) {
		m_root = new QuadTreeNode();
		m_items = new List<Transform>();
		m_nodes = new List<QuadTreeNode>();
		m_indexTable = new Dictionary<Transform, QuadTreeNode>();

		m_root.Init(0, null, new Rect(_x, _y, _w, _h));
	}

	public void Insert(Transform _item) {
		m_root.Insert(_item, ref m_indexTable);
	}

	public void Update(Transform _item) {
		if (m_indexTable.ContainsKey(_item)) {
			QuadTreeNode node = m_indexTable[_item];
			
			if (!node.Contains(_item.position)) {
				QuadTreeNode parent = node.parent;
				node.Remove(_item, ref m_indexTable);
				m_indexTable.Remove(_item);
				parent.InsertFromLeaf(_item, ref m_indexTable);
			}
		}
	}

	public void Remove(Transform _item) {
		if (m_indexTable.ContainsKey(_item)) {
			QuadTreeNode node = m_indexTable[_item];
			node.Remove(_item, ref m_indexTable);
			m_indexTable.Remove(_item);
		}
	}

	public Transform[] GetItemsInRange(Rect _rect) {
		m_items.Clear();
		PreOrderInRange(m_root, _rect);
		return m_items.ToArray();
	}

	private void PreOrderInRange(QuadTreeNode _node, Rect _rect) {
		if (_node.IsLeaf()) {
			m_items.AddRange(_node.items);
		} else {
			for (int i = 0; i < 4; i++) {
				if (_node.child[i].Intersects(_rect)) {
					PreOrderInRange(_node.child[i], _rect);
				}
			}
		}
	}

	private void PreOrderGetNodes(QuadTreeNode _node) {
		m_nodes.Add(_node);
		if (!_node.IsLeaf()) {
			for (int i = 0; i < 4; i++) {
				PreOrderGetNodes(_node.child[i]);
			}
		}
	}

	// Debug
	public void DrawGizmos(Color _color) {
		m_nodes.Clear();
		PreOrderGetNodes(m_root);

		Gizmos.color = _color;
		for (int i = 0; i < m_nodes.Count; i++) {
			Gizmos.DrawWireCube(m_nodes[i].bounds.center, m_nodes[i].bounds.size);
		}
	}
}

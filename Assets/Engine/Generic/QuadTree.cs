using UnityEngine;
using System.Collections.Generic;

public class QuadTree {

	public const uint MAX_ELEMENTS = 4;
	public const uint MAX_DEPTH = 6;


	private QuadTreeNode m_root;


	Dictionary<Transform, QuadTreeNode> m_indexTable;


	///

	public QuadTree(float _x, float _y, float _w, float _h) {

		m_root = new QuadTreeNode();
		m_root.Init(0, null, new Rect(_x, _y, _w, _h));
		m_indexTable = new Dictionary<Transform, QuadTreeNode>();
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

	public List<Transform> GetItemsInRange(Rect _rect) {

		List<Transform> items = new List<Transform>();
		PreOrderInRange(m_root, _rect, ref items);
		return items;
	}

	private void PreOrderGetNodes(QuadTreeNode _node, ref List<QuadTreeNode> _nodes) {

		_nodes.Add(_node);
		if (!_node.IsLeaf()) {
			for (int i = 0; i < 4; i++) {
				PreOrderGetNodes(_node.child[i], ref _nodes);
			}
		}
	}

	private void PreOrderInRange(QuadTreeNode _node, Rect _rect, ref List<Transform> _items) {

		if (_node.IsLeaf()) {
			_items.AddRange(_node.items);
		} else {
			for (int i = 0; i < 4; i++) {
				if (_node.child[i].Intersects(_rect)) {
					PreOrderInRange(_node.child[i], _rect, ref _items);
				}
			}
		}
	}

	// Debug
	public void DrawGizmos(Color _color) {

		List<QuadTreeNode> nodes = new List<QuadTreeNode>();
		PreOrderGetNodes(m_root, ref nodes);

		Gizmos.color = _color;
		for (int i = 0; i < nodes.Count; i++) {
			Gizmos.DrawWireCube(nodes[i].bounds.center, nodes[i].bounds.size);
		}
	}
}

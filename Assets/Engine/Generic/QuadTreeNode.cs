using UnityEngine;
using System.Collections.Generic;

public class QuadTreeNode {

	private Bounds m_bounds;
	public  Bounds bounds { get { return m_bounds; } }
	

	private QuadTreeNode m_parent;
	public  QuadTreeNode parent { get { return m_parent; } set { m_parent = value; } }


	private QuadTreeNode[] m_child;
	public  QuadTreeNode[] child { get { return m_child; } }


	private List<Transform> m_item;
	private uint m_depth;



	/***********/
	/** Setup **/
	/***********/
	public QuadTreeNode() {

		m_child = new QuadTreeNode[4];
		m_item = new List<Transform>();

		Init(0, null, 0, 0, 0, 0);
	}


	public void Init(uint _level, QuadTreeNode _parent, float _x, float _y, float _w, float _h) {

		m_depth = _level;
		m_bounds = new Bounds(new Vector3(_x + _w * 0.5f, _y + _h * 0.5f, 0), new Vector3(_w, _h, 0));

		m_parent = _parent;

		for (int i = 0; i < m_child.Length; i++) {
			m_child[i] = null;
		}

		m_item.Clear();
	}
	/***********/


	/*************/
	/** Queries **/
	/*************/
	public bool IsLeaf() 											{ return m_child[0] == null; }
	public bool Contains(float _x, float _y) 						{ return Intersects(_x, _y, 0, 0); }
	public bool Intersects(float _x, float _y, float _w, float _h) 	{ return !(_x > m_bounds.max.x || _x + _w < m_bounds.min.x || _y + _h < m_bounds.max.y || _y > m_bounds.min.y); }
	/*************/


	/*********************/
	/** Node Management **/
	/*********************/
	public QuadTreeNode Insert(Transform _item, ref Dictionary<Transform, QuadTreeNode> _indexTable) {

		if (Contains(_item.position.x, _item.position.y)) {
			if (IsLeaf()) {
				if (m_item.Count < QuadTree.MAX_ELEMENTS || m_depth >= QuadTree.MAX_DEPTH) {
					_indexTable[_item] = this;				
					m_item.Add(_item);
					return this;
				} else {
					Subdivide(ref _indexTable);
				}
			}
			
			for (int i = 0; i < m_child.Length; i++) {
				QuadTreeNode n = m_child[i].Insert(_item, ref _indexTable);
				if (n != null)
					return n;
			}
		}
		
		return null;

	}

	public QuadTreeNode InsertFromLeaf(Transform _item, ref Dictionary<Transform, QuadTreeNode> _indexTable) {

		if (Contains(_item.position.x, _item.position.y))  {
			return Insert(_item, ref _indexTable);
		} else if (m_parent != null) {
			return m_parent.InsertFromLeaf(_item, ref _indexTable);
		}
		
		return null;
	}
	
	public void Remove(Transform _item, ref Dictionary<Transform, QuadTreeNode> _indexTable) {

		m_item.Remove(_item);
		Join(ref _indexTable);
	}
		
	private void Subdivide(ref Dictionary<Transform, QuadTreeNode> _indexTable) {

		for (int i = 0; i < m_child.Length; i++) {
			m_child[i] = new QuadTreeNode();
		}

		m_child[0].Init(m_depth + 1, this, m_bounds.min.x, m_bounds.max.y, m_bounds.extents.x, m_bounds.extents.y);
		m_child[1].Init(m_depth + 1, this, m_bounds.center.x, m_bounds.max.y, m_bounds.extents.x, m_bounds.extents.y);
		m_child[2].Init(m_depth + 1, this, m_bounds.min.x, m_bounds.center.y, m_bounds.extents.x, m_bounds.extents.y);
		m_child[3].Init(m_depth + 1, this, m_bounds.center.y, m_bounds.center.y, m_bounds.extents.x, m_bounds.extents.y);

		for (int i = 0; i < m_item.Count; i++) {
			for (int j = 0; j < m_child.Length; j++) {
				if (m_child[j].Insert(m_item[i], ref _indexTable) != null) {
					break;
				}
			}
		}

		m_item.Clear();
	}

	private void Join(ref Dictionary<Transform, QuadTreeNode> _indexTable) {

		if (IsLeaf()) {
			if (m_parent != null) {
				m_parent.Join(ref _indexTable);
			}
		} else {
			int tmpSize = 0;

			for (int i = 0; i < 4; i++) {
				QuadTreeNode c = m_child[i];
				
				if (c.IsLeaf()) {
					tmpSize += c.m_item.Count;
				} else {
					return;
				}				
			}
			
			if (tmpSize <= QuadTree.MAX_ELEMENTS) {
				// free childs
				for (int i = 0; i < 4; i++)	{
					m_item.AddRange(m_child[i].m_item);
					m_child[i] = null;
				}
				
				// Move all the data
				for (int i = 0; i < m_item.Count; i++) {
					_indexTable[m_item[i]] = this;
				}
			}
		}
	}
	/*************/
}

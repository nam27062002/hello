using UnityEngine;
using System.Collections.Generic;

public class QuadTreeNode<T> where T : IQuadTreeItem {

	private Rect m_bounds;
	public 	Rect bounds { get { return m_bounds; } }

	private QuadTreeNode<T> m_parent;
	public  QuadTreeNode<T> parent { get { return m_parent; } set { m_parent = value; } }

	private QuadTreeNode<T>[] m_child;
	public  QuadTreeNode<T>[] child { get { return m_child; } }

	private List<T> m_items;
	public	List<T> items { get { return m_items; } }

	private uint m_depth;



	/***********/
	/** Setup **/
	/***********/
	public QuadTreeNode() {

		m_child = new QuadTreeNode<T>[4];
		m_items = new List<T>();

		Init(0, null, new Rect());
	}


	public void Init(uint _level, QuadTreeNode<T> _parent, Rect _rect) {

		m_depth = _level;
		m_bounds = _rect;

		m_parent = _parent;

		for (int i = 0; i < m_child.Length; i++) {
			m_child[i] = null;
		}

		m_items.Clear();
	}
	/***********/


	/*************/
	/** Queries **/
	/*************/
	public bool IsLeaf() 					{ return m_child[0] == null; }
	public bool Contains(Vector2 _point)	{ return m_bounds.Contains(_point); }
	//public bool Contains(Vector3 _point)	{ return m_bounds.Contains(_point); }
	public bool Intersects(Rect _rect) 		{ return m_bounds.Overlaps(_rect); }
	/*************/


	/*********************/
	/** Node Management **/
	/*********************/
	public QuadTreeNode<T> Insert(T _item, ref Dictionary<T, QuadTreeNode<T>> _indexTable) {

		if (Contains(_item.transform.position)) {
			if (IsLeaf()) {
				if (m_items.Count < QuadTree<T>.MAX_ELEMENTS || m_depth >= QuadTree<T>.MAX_DEPTH) {
					_indexTable[_item] = this;				
					m_items.Add(_item);
					return this;
				} else {
					Subdivide(ref _indexTable);
				}
			}
			
			for (int i = 0; i < m_child.Length; i++) {
				QuadTreeNode<T> n = m_child[i].Insert(_item, ref _indexTable);
				if (n != null)
					return n;
			}
		}
		
		return null;

	}

	public QuadTreeNode<T> InsertFromLeaf(T _item, ref Dictionary<T, QuadTreeNode<T>> _indexTable) {

		if (Contains(_item.transform.position))  {
			return Insert(_item, ref _indexTable);
		} else if (m_parent != null) {
			return m_parent.InsertFromLeaf(_item, ref _indexTable);
		}
		
		return null;
	}
	
	public void Remove(T _item, ref Dictionary<T, QuadTreeNode<T>> _indexTable) {

		m_items.Remove(_item);
		Join(ref _indexTable);
	}
		
	private void Subdivide(ref Dictionary<T, QuadTreeNode<T>> _indexTable) {

		for (int i = 0; i < m_child.Length; i++) {
			m_child[i] = new QuadTreeNode<T>();
		}

		m_child[0].Init(m_depth + 1, this, new Rect(m_bounds.min.x, 	m_bounds.center.y, 	m_bounds.width * 0.5f, m_bounds.height * 0.5f));
		m_child[1].Init(m_depth + 1, this, new Rect(m_bounds.center.x,	m_bounds.center.y, 	m_bounds.width * 0.5f, m_bounds.height * 0.5f));
        m_child[2].Init(m_depth + 1, this, new Rect(m_bounds.min.x, 	m_bounds.min.y, 	m_bounds.width * 0.5f, m_bounds.height * 0.5f));
        m_child[3].Init(m_depth + 1, this, new Rect(m_bounds.center.x, 	m_bounds.min.y, 	m_bounds.width * 0.5f, m_bounds.height * 0.5f));

		for (int i = 0; i < m_items.Count; i++) {
			for (int j = 0; j < m_child.Length; j++) {
				if (m_child[j].Insert(m_items[i], ref _indexTable) != null) {
					break;
				}
			}
		}

		m_items.Clear();
	}

	private void Join(ref Dictionary<T, QuadTreeNode<T>> _indexTable) {

		if (IsLeaf()) {
			if (m_parent != null) {
				m_parent.Join(ref _indexTable);
			}
		} else {
			int tmpSize = 0;

			for (int i = 0; i < 4; i++) {
				QuadTreeNode<T> c = m_child[i];
				
				if (c.IsLeaf()) {
					tmpSize += c.m_items.Count;
				} else {
					return;
				}				
			}
			
			if (tmpSize <= QuadTree<T>.MAX_ELEMENTS) {
				// free childs
				for (int i = 0; i < 4; i++)	{
					m_items.AddRange(m_child[i].m_items);
					m_child[i] = null;
				}
				
				// Move all the data
				for (int i = 0; i < m_items.Count; i++) {
					_indexTable[m_items[i]] = this;
				}
			}
		}
	}
	/*************/
}

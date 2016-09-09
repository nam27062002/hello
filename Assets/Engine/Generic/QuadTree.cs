// QuadTree.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach, Alger Ortín Castellví
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// All items to be inserted in a quad tree must implement this interface
/// </summary>
public interface IQuadTreeItem {
	//Transform transform { get; }
	Rect boundingRect { get; }
}

/// <summary>
/// Quad tree structure used to partition a two-dimensional space by recursively 
/// subdividing it into four quadrants or regions.
/// Optimizes search by position within the area.
/// See https://en.wikipedia.org/wiki/Quadtree
/// </summary>
public class QuadTree<T> where T : IQuadTreeItem {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const uint MAX_ELEMENTS = 4;
	public const uint MAX_DEPTH = 6;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private QuadTreeNode<T> m_root;
	private List<QuadTreeNode<T>> m_nodes;
	private Dictionary<T, List<QuadTreeNode<T>>> m_indexTable;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create a new QuadTree with the given dimensions.
	/// </summary>
	/// <param name="_x">X position in world coords.</param>
	/// <param name="_y">Y position in world coords.</param>
	/// <param name="_w">Wdith in world coords.</param>.</param>
	/// <param name="_h">Height in world coords.</param>
	public QuadTree(float _x, float _y, float _w, float _h) {
		m_root = new QuadTreeNode<T>();
		m_nodes = new List<QuadTreeNode<T>>();
		m_indexTable = new Dictionary<T, List<QuadTreeNode<T>>>();

		m_root.Init(0, null, new Rect(_x, _y, _w, _h));
	}

	/// <summary>
	/// Insert a new item to the QuadTree.
	/// </summary>
	/// <param name="_item">The item to be inserted.</param>
	public void Insert(T _item) {
		m_root.Insert(_item, ref m_indexTable);
	}

	/// <summary>
	/// Remove an item from the QuadTree.
	/// </summary>
	/// <param name="_item">The item to be removed.</param>
	public void Remove(T _item) {
		if (m_indexTable.ContainsKey(_item)) {			
			List<QuadTreeNode<T>> nodes = m_indexTable[_item];
			while(nodes.Count > 0) {
				QuadTreeNode<T> node = nodes.First();
				node.Remove(_item, ref m_indexTable);
			}
			m_indexTable.Remove(_item);
		}
	}

	/// <summary>
	/// [Not implemented]
	/// Update the QuadTree indexing of an item.
	/// Call this if an item's position has changed.
	/// </summary>
	/// <param name="_item">The item to be updated.</param>
	public void Update(T _item) {
		//TODO: implement when needed
	}

	/// <summary>
	/// Find all items within the given rectangle.
	/// </summary>
	/// <returns>An array with all the items in range.</returns>
	/// <param name="_rect">The rectangle to be checked.</param>
	public T[] GetItemsInRange(Rect _rect) {
		HashSet<T> hashSet = new HashSet<T>();	
		PreOrderInRange(m_root, _rect, ref hashSet);

		int i = 0;
		T[] array = new T[hashSet.Count];
		foreach(T item in hashSet) {
			array[i] = item;
			i++;
		}

		return array;
	}

	/// <summary>
	/// Find all items within the given rectangle.
	/// </summary>
	/// <param name="_rect">The rectangle to be checked.</param>
	/// <param name="_set">HashSet to store the results</param>
	public void GetHashSetInRange(Rect _rect, ref HashSet<T> _set) {		
		PreOrderInRange(m_root, _rect, ref _set);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Recursively find all items within the given rect in the given node and
	/// add them to the given list.
	/// </summary>
	/// <param name="_node">Node to be checked.</param>
	/// <param name="_rect">Rectangle to be checked.</param>
	/// <param name="_set">HashSet where the selected items will be added.</param>
	private void PreOrderInRange(QuadTreeNode<T> _node, Rect _rect, ref HashSet<T> _set) {
		if (_node.IsLeaf()) {
			// Check intersection item by item
			for(int i = 0; i < _node.items.Count; i++) {				
				//if(_rect.Contains(_node.items[i].transform.position)) {
				if(_rect.Overlaps(_node.items[i].boundingRect)) {
					_set.Add(_node.items[i]);
				}
			}
		} else {
			for (int i = 0; i < 4; i++) {
				if (_node.child[i].Intersects(_rect)) {
					PreOrderInRange(_node.child[i], _rect, ref _set);
				}
			}
		}
	}

	/// <summary>
	/// Get all the items in a node and store them in the target list.
	/// </summary>
	/// <param name="_node">The node to be checked.</param>
	/// <param name="_list">The list where to store the values.</param>
	private void PreOrderGetNodes(QuadTreeNode<T> _node, ref List<QuadTreeNode<T>> _list) {
		_list.Add(_node);
		if (!_node.IsLeaf()) {
			for (int i = 0; i < 4; i++) {
				PreOrderGetNodes(_node.child[i], ref _list);
			}
		}
	}

	//------------------------------------------------------------------------//
	// DEBUG METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw helper debug stuff on the scene.
	/// </summary>
	/// <param name="_color">The color to be used to draw the quad tree's grid.</param>
	public void DrawGizmos(Color _color) {
		m_nodes.Clear();
		PreOrderGetNodes(m_root, ref m_nodes);

		Gizmos.color = _color;
		for (int i = 0; i < m_nodes.Count; i++) {
			Gizmos.DrawWireCube(m_nodes[i].bounds.center, m_nodes[i].bounds.size);
		}
	}
}

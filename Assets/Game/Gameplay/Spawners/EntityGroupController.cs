﻿using UnityEngine;
using System.Collections;

public class EntityGroupController : MonoBehaviour 
{
	public AI.Group flock = new AI.Group();
	
	private GameObject[] m_entities;
	public GameObject[] entities { get { return m_entities; } }


	// Use this for initialization
	public void Init(int _maxEntities) {
		flock = new AI.Group();
        m_entities = new GameObject[_maxEntities];
	}

	public void Add(GameObject _entity) {
		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] == null) {
				m_entities[i] = _entity;
				break;
			}
		}
	}

	public void Remove(GameObject _entity) {
		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] == _entity) {
				m_entities[i] = null;
				break;
			}
		}
	}
}

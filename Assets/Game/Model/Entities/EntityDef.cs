// PreyDef.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Definition of an entity.
/// Put here data shared by all/most preys and independent from gameplay prefab/behaviours.
/// </summary>
[System.Serializable]
public class EntityDef : Definition {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] [SkuList(typeof(EntityCategoryDef), false)] private string m_category = "";
	public string categorySku { get { return m_category; }}
	public EntityCategoryDef category { get { return DefinitionsManager.entityCategories.GetDef(m_category); }}

	[Header("Info")]
	[SerializeField] private string m_tidName = "";
	public string tidName { get { return m_tidName; }}

	[SerializeField] private string m_tidDesc = "";
	public string tidDesc { get { return m_tidDesc; }}
}
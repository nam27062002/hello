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

	[Header("Prey Stats")]
	[SerializeField] private Reward m_reward;
	public Reward reward { get { return m_reward; }}

	[SerializeField][Range(0,1)] private float m_goldenChance = 0f;
	public float goldenChance { get { return m_goldenChance; }}

	[SerializeField][Range(0,1)] private float m_pcChance = 0f;
	public float pcChance { get { return m_pcChance; }}

	[Header("Edible")]
	[SerializeField] private DragonTier m_edibleFromTier = 0;
	public DragonTier edibleFromTier { get { return m_edibleFromTier; } }

	[SerializeField][Range(1, 10)] private float m_biteResistance = 1f;
	public float biteResistance { get { return m_biteResistance; }}

	[Header("Feedback")]
	[SerializeField] private FeedbackData m_feedbackData = new FeedbackData();
	public FeedbackData feedbackData { get { return m_feedbackData; }}
}
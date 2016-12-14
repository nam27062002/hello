﻿using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Decoration : IEntity {	
	// Exposed to inspector
	[DecorationSkuList]
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; } }


	//
	private bool m_isBurnable;
	public bool  isBurnable { get { return m_isBurnable; } }

	private DragonTier m_minTierBurnFeedback;
	public  DragonTier minTierBurnFeedback { get { return m_minTierBurnFeedback; } }

	private DragonTier m_minTierBurn;
	public  DragonTier minTierBurn { get { return m_minTierBurn; } }

	private int m_burnFeedbackChance;
	public  int burnFeedbackChance { get { return m_burnFeedbackChance; } }

	private bool m_isDestructible;
	public bool  isDestructible { get { return m_isDestructible; } }

	private DragonTier m_minTierDestructionFeedback;
	public  DragonTier minTierDestructionFeedback { get { return m_minTierDestructionFeedback; } }

	private DragonTier m_minTierDestruction;
	public  DragonTier minTierDestruction { get { return m_minTierDestruction; } }


	//-----------------------------------------------------
	void Awake() {
		InitFromDef();
	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DECORATIONS, sku);

		// Simple data
		m_isBurnable 			= m_def.GetAsBool("isBurnable");
		m_minTierBurnFeedback 	= (DragonTier)m_def.GetAsInt("minTierBurnFeedback", (int)DragonTier.COUNT);
		m_minTierBurn			= (DragonTier)m_def.GetAsInt("minTierBurn", (int)DragonTier.COUNT);
		m_burnFeedbackChance	= m_def.GetAsInt("burnFeedbackChance", 100);

		m_isDestructible 				= m_def.GetAsBool("isDestructible");
		m_minTierDestructionFeedback 	= (DragonTier)m_def.GetAsInt("minTierDestructionFeedback", (int)DragonTier.COUNT);
		m_minTierDestruction			= (DragonTier)m_def.GetAsInt("minTierDestruction", (int)DragonTier.COUNT);

		m_maxHealth = 1f;
	}
}

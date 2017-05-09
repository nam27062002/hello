﻿using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Decoration : IEntity {	
	// Exposed to inspector
	[DecorationSkuList]
	[SerializeField] private string m_sku;
	public override string sku { get { return m_sku; } }

	//
	private bool m_isBurnable;
	public bool  isBurnable { get { return m_isBurnable; } }

	private int m_burnFeedbackChance;
	public  int burnFeedbackChance { get { return m_burnFeedbackChance; } }

	private DragonTier m_minTierBurnFeedback;
	public  DragonTier minTierBurnFeedback { get { return m_minTierBurnFeedback; } }

	private DragonTier m_minTierBurn;
	public  DragonTier minTierBurn { get { return m_minTierBurn; } }

	private DragonTier m_minTierDestructionFeedback;
	public  DragonTier minTierDestructionFeedback { get { return m_minTierDestructionFeedback; } }

	private DragonTier m_minTierDestruction;
	public  DragonTier minTierDestruction { get { return m_minTierDestruction; } }

	private DragonTier m_minTierDisintegrate;
	public  DragonTier minTierDisintegrate { get { return m_minTierDisintegrate; } }

	//-----------------------------------------------------
	protected override void Awake() {
		base.Awake();

		InitFromDef();
	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DECORATIONS, sku);

		// Simple data
		m_isBurnable 			= m_def.GetAsBool("isBurnable");
		m_burnFeedbackChance	= m_def.GetAsInt("burnFeedbackChance", 100);
		m_minTierBurnFeedback 	= (DragonTier)m_def.GetAsInt("minTierBurnFeedback", (int)DragonTier.COUNT);
		m_minTierBurn			= (DragonTier)m_def.GetAsInt("minTierBurn", (int)DragonTier.COUNT);

		m_minTierDestructionFeedback 	= (DragonTier)m_def.GetAsInt("minTierDestructionFeedback", (int)DragonTier.COUNT);
		m_minTierDestruction			= (DragonTier)m_def.GetAsInt("minTierDestruction", (int)DragonTier.COUNT);

		m_minTierDisintegrate = DragonTier.TIER_3;

		m_maxHealth = 1f;
	}
}

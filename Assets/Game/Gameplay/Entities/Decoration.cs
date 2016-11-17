using UnityEngine;
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

	private DragonTier m_burnFeedbackChance;
	public  DragonTier burnFeedbackChance { get { return m_burnFeedbackChance; } } 


	//-----------------------------------------------------
	void Awake() {
		InitFromDef();
	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DECORATIONS, sku);

		// Simple data
		m_isBurnable = m_def.GetAsBool("isBurnable");
		m_minTierBurnFeedback 	= (DragonTier)m_def.GetAsInt("minTierBurnFeedback");
		m_minTierBurn			= (DragonTier)m_def.GetAsInt("minTierBurn");
		m_burnFeedbackChance	= (DragonTier)m_def.GetAsInt("burnFeedbackChance");

		m_maxHealth = 1f;
	}
}

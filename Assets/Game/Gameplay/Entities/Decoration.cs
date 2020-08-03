using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Decoration : IEntity {	
	//-----------------------------------------------------
	// Exposed to inspector
	[DecorationSkuList]
	[SerializeField] private string m_sku;
	public override string sku { get { return m_sku; } }

	[SerializeField] private DragonTier m_tier = DragonTier.TIER_0;
	public DragonTier tier { get { return m_tier; } }

	[SerializeField] private bool m_disintegrable = true;
	public bool isDisintegrable { get { return m_disintegrable; }  }


	//-----------------------------------------------------
	private Reward m_reward;
	public Reward reward { get { return m_reward; }}

	private float m_baseRewardScore;


	//-----------------------------------------------------
	protected override void Awake() {
		base.Awake();
		InitFromDef();
	}

	private void InitFromDef() {
		// Get the definition
		m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DECORATIONS, sku);

		m_reward.score = m_def.GetAsInt("rewardScore", 0);

		m_maxHealth = 1f;
	}
}

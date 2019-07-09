using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Gelato : Entity {	
	//-----------------------------------------------------
	[Separator]
	[SerializeField] private float m_scoreMultiplier = 2f;
	[SerializeField] private float m_coinsMultiplier = 2f;
	[SerializeField] private float m_xpMultiplier = 2f;

	//-----------------------------------------------------	
	public void OverrideRewardFromDef(DefinitionNode _def) {
		BuildRewardFromDef(_def);
	}

	protected override void OnRewardCreated() {
		m_reward.score = (m_reward.score * m_scoreMultiplier);
		m_reward.coins = (m_reward.coins * m_coinsMultiplier);
		m_reward.xp = (m_reward.xp * m_xpMultiplier);
	}
}

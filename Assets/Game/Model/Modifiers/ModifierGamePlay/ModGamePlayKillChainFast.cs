﻿
public class ModGamePlayKillChainFast : ModifierGamePlay {
	public const string TARGET_CODE = "kill_chain_fast";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlayKillChainFast(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
		m_textParam = m_percentage + "%";
	}

	public override void Apply() {
		RewardManager.AddKillStreakPercentage(m_percentage);
	}

	public override void Remove() {
		RewardManager.AddKillStreakPercentage(-m_percentage);
	}
}

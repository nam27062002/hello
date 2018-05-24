
public class ModGamePlayMissionReward : ModifierGamePlay {
	public const string TARGET_CODE = "mission_reward";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlayMissionReward(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		Mission.AddSCMultiplier(m_percentage);
	}

	public override void Remove() {
		Mission.AddSCMultiplier(-m_percentage);
	}
}
